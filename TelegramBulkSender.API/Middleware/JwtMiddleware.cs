using Microsoft.AspNetCore.Http;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuthService authService, JwtTokenService tokenService)
    {
        if (!(context.User.Identity?.IsAuthenticated ?? false) && context.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
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
            }
        }

        await _next(context);
    }
}
