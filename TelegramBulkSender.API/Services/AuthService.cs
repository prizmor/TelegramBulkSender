using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class AuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(ApplicationDbContext dbContext, IConfiguration configuration, JwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
    }

    public async Task EnsureRootUserAsync()
    {
        var rootPassword = _configuration.GetValue<string>("ROOT_PASSWORD") ?? throw new InvalidOperationException("ROOT_PASSWORD is not configured");
        var rootUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == "root");
        if (rootUser == null)
        {
            rootUser = new User
            {
                Username = "root",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(rootPassword),
                CreatedAt = DateTime.UtcNow,
                IsRoot = true
            };

            _dbContext.Users.Add(rootUser);
            await _dbContext.SaveChangesAsync();
            return;
        }

        if (string.IsNullOrEmpty(rootUser.PasswordHash))
        {
            rootUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rootPassword);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<(string accessToken, string refreshToken, DateTime expires)?> AuthenticateAsync(string username, string password)
    {
        var user = await _dbContext.Users.Include(u => u.Sessions).SingleOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        var (accessToken, expires) = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var session = new UserSession
        {
            UserId = user.Id,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            LastActivityAt = DateTime.UtcNow
        };
        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return (accessToken, refreshToken, expires);
    }

    public async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var session = await _dbContext.UserSessions.Include(s => s.User).SingleOrDefaultAsync(s => s.RefreshToken == refreshToken);
        if (session == null || session.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        session.LastActivityAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return session.User;
    }

    public async Task<IEnumerable<User>> GetUsersAsync() => await _dbContext.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync();

    public async Task<User> CreateUserAsync(string username, string password, bool isRoot)
    {
        if (await _dbContext.Users.AnyAsync(x => x.Username == username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            IsRoot = isRoot
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task UpdatePasswordAsync(int userId, string newPassword)
    {
        var user = await _dbContext.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found");
        if (user.IsRoot)
        {
            throw new InvalidOperationException("Root user cannot be deleted");
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SignOutAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        var session = await _dbContext.UserSessions.SingleOrDefaultAsync(s => s.RefreshToken == refreshToken);
        if (session != null)
        {
            _dbContext.UserSessions.Remove(session);
            await _dbContext.SaveChangesAsync();
        }
    }
}
