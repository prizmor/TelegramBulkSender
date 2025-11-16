using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, AuthService authService, JwtTokenService tokenService)
    {
        var tokenRefreshed = false;

        if (context.Request.Cookies.TryGetValue("access_token", out var accessToken))
        {
            tokenRefreshed = AttachUserToContext(context, accessToken);
        }

        if (!tokenRefreshed && context.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            var user = await authService.ValidateRefreshTokenAsync(refreshToken);
            if (user != null)
            {
                var (accessToken, expires) = tokenService.GenerateAccessToken(user);
                context.Response.Cookies.Append("access_token", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = expires
                });

                context.Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                tokenRefreshed = AttachUserToContext(context, accessToken);
            }
        }

        await _next(context);
    }

    private bool AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration.GetValue<string>("JWT_SECRET") ?? throw new InvalidOperationException("JWT secret is not configured");
            var key = Encoding.UTF8.GetBytes(secret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken)
            {
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                             ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (int.TryParse(userId, out var parsedUserId))
                {
                    context.Items["UserId"] = parsedUserId;
                    context.Items["UserRole"] = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
                    return true;
                }
            }
        }
        catch
        {
            // ignore invalid tokens and fall back to refresh logic
        }

        return false;
    }
}
