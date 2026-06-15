using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Auth;
using BarberShop.API.Entities;
using BarberShop.API.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ClientEntity = BarberShop.API.Entities.Client;

namespace BarberShop.API.Services.Auth;

public class AuthService : IAuthService
{
    private const string ActiveStatus = "ACTIVE";
    private readonly BarberShopDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly AccountLockoutSettings _accountLockoutSettings;
    private readonly AuthSettings _authSettings;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        BarberShopDbContext dbContext,
        IConfiguration configuration,
        IOptions<AccountLockoutSettings> accountLockoutOptions,
        IOptions<AuthSettings> authOptions,
        IEmailService emailService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _accountLockoutSettings = accountLockoutOptions.Value;
        _authSettings = authOptions.Value;
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Users.AnyAsync(
            user => user.Email.ToLower() == email,
            cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Status = ActiveStatus,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var clientRole = await GetClientRoleAsync(cancellationToken);

        _dbContext.Users.Add(user);
        _dbContext.Clients.Add(new ClientEntity
        {
            User = user,
            LoyaltyPoints = 0
        });
        _dbContext.UserRoles.Add(new UserRole
        {
            User = user,
            Role = clientRole
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            currentUser => currentUser.Email.ToLower() == email,
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Failed login attempt.");
            return new LoginResult();
        }

        var now = DateTime.UtcNow;
        if (IsAccountLocked(user, now))
        {
            _logger.LogWarning("Login attempt blocked because account is locked for user {UserId}.", user.Id);
            return new LoginResult { IsLocked = true };
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            await RecordFailedLoginAttemptAsync(user, now, cancellationToken);
            return new LoginResult();
        }

        if (!IsUserActive(user))
        {
            _logger.LogWarning("Failed login attempt for inactive user {UserId}.", user.Id);
            return new LoginResult();
        }

        ResetLockoutState(user);
        var authResponse = await CreateAuthResponseAsync(user, cancellationToken);

        _logger.LogInformation("Successful login reset lockout state for user {UserId}.", user.Id);

        return new LoginResult
        {
            AuthResponse = authResponse
        };
    }

    public async Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        var now = DateTime.UtcNow;
        if (refreshToken is null
            || refreshToken.RevokedAt is not null
            || refreshToken.ExpiresAt <= now
            || !IsUserActive(refreshToken.User))
        {
            return null;
        }

        refreshToken.RevokedAt = now;

        return await CreateAuthResponseAsync(refreshToken.User, cancellationToken);
    }

    public async Task<bool> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(
            token => token.TokenHash == tokenHash && token.RevokedAt == null,
            cancellationToken);

        if (refreshToken is null)
        {
            return false;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            currentUser => currentUser.Id == userId,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = await GetUserRolesAsync(user.Id, cancellationToken);

        return new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.Phone,
            Status = user.Status,
            Roles = roles
        };
    }

    public async Task<CurrentUserResponse> UpdateProfileAsync(
        int userId,
        ProfileUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            currentUser => currentUser.Id == userId,
            cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.PhoneNumber.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetCurrentUserAsync(user.Id, cancellationToken)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public async Task<bool> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            currentUser => currentUser.Id == userId,
            cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Password change failed because current password was invalid for user {UserId}.", user.Id);
            return false;
        }

        var now = DateTime.UtcNow;
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        var revokedRefreshTokenCount = await RevokeActiveRefreshTokensAsync(user.Id, now, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Password changed for user {UserId}. Revoked {RefreshTokenCount} active refresh tokens.",
            user.Id,
            revokedRefreshTokenCount);

        return true;
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var email = NormalizeEmail(request.Email);
        _logger.LogInformation("Password reset requested.");
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(
                currentUser => currentUser.Email.ToLower() == email,
                cancellationToken);

            if (user is null)
            {
                return;
            }

            _logger.LogInformation("Password reset token issued for user {UserId}.", user.Id);

            var resetToken = GenerateSecureToken();
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(GetPasswordResetTokenExpirationMinutes());

            var existingTokens = await _dbContext.PasswordResetTokens
                .Where(token => token.UserId == user.Id && token.UsedAt == null && token.ExpiresAt > now)
                .ToListAsync(cancellationToken);

            foreach (var existingToken in existingTokens)
            {
                existingToken.UsedAt = now;
            }

            _dbContext.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = HashToken(resetToken),
                CreatedAt = now,
                ExpiresAt = expiresAt
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            var resetLink = CreatePasswordResetLink(user.Email, resetToken);
            await _emailService.SendAsync(new EmailMessage
            {
                To = user.Email,
                Subject = "Reset your Barber Shop password",
                Body = CreatePasswordResetEmailBody(resetLink)
            });
        }
        finally
        {
            await ApplyForgotPasswordMinimumResponseTimeAsync(stopwatch, cancellationToken);
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var tokenHash = HashToken(request.Token);
        var resetToken = await _dbContext.PasswordResetTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash
                    && token.User.Email.ToLower() == email
                    && token.UsedAt == null,
                cancellationToken);

        var now = DateTime.UtcNow;
        if (resetToken is null || resetToken.ExpiresAt <= now)
        {
            _logger.LogWarning("Password reset failed because token was invalid or expired.");
            return false;
        }

        resetToken.User.PasswordHash = _passwordHasher.HashPassword(resetToken.User, request.NewPassword);
        resetToken.UsedAt = now;
        var revokedRefreshTokenCount = await RevokeActiveRefreshTokensAsync(resetToken.User.Id, now, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset completed for user {UserId}.", resetToken.User.Id);
        _logger.LogInformation(
            "Revoked {RefreshTokenCount} active refresh tokens after password reset for user {UserId}.",
            revokedRefreshTokenCount,
            resetToken.User.Id);

        return true;
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var roles = await GetUserRolesAsync(user.Id, cancellationToken);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = GenerateSecureToken();

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays())
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            User = new CurrentUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.Phone,
                Status = user.Status,
                Roles = roles
            }
        };
    }

    private async Task<Role> GetClientRoleAsync(CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(
            currentRole => currentRole.Name.ToUpper() == RoleNames.CLIENT,
            cancellationToken);

        if (role is null)
        {
            throw new KeyNotFoundException("Required CLIENT role does not exist in the database.");
        }

        return role;
    }

    private async Task RecordFailedLoginAttemptAsync(
        User user,
        DateTime now,
        CancellationToken cancellationToken)
    {
        user.FailedLoginAttempts += 1;
        _logger.LogWarning("Failed login attempt for user {UserId}.", user.Id);

        if (user.FailedLoginAttempts >= GetMaxFailedLoginAttempts())
        {
            user.LockoutCount += 1;
            user.LockoutEnd = now.AddMinutes(GetLockoutDurationMinutes(user.LockoutCount));
            user.FailedLoginAttempts = 0;

            _logger.LogWarning(
                "Account locked for user {UserId}. LockoutCount: {LockoutCount}.",
                user.Id,
                user.LockoutCount);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void ResetLockoutState(User user)
    {
        user.FailedLoginAttempts = 0;
        user.LockoutCount = 0;
        user.LockoutEnd = null;
    }

    private static bool IsAccountLocked(User user, DateTime now)
    {
        return user.LockoutEnd is not null && user.LockoutEnd > now;
    }

    private int GetMaxFailedLoginAttempts()
    {
        return _accountLockoutSettings.MaxFailedAttempts > 0
            ? _accountLockoutSettings.MaxFailedAttempts
            : 5;
    }

    private int GetLockoutDurationMinutes(int lockoutCount)
    {
        var durations = _accountLockoutSettings.DurationsMinutes;
        if (durations.Length == 0)
        {
            return 60;
        }

        var durationIndex = Math.Min(Math.Max(lockoutCount, 1), durations.Length) - 1;
        var duration = durations[durationIndex];

        return duration > 0 ? duration : 60;
    }

    private int GetRefreshTokenExpirationDays()
    {
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays");
        if (expirationDays <= 0)
        {
            throw new InvalidOperationException("Jwt:RefreshTokenExpirationDays must be greater than 0.");
        }

        return expirationDays;
    }

    private int GetPasswordResetTokenExpirationMinutes()
    {
        return _authSettings.PasswordResetTokenExpirationMinutes > 0
            ? _authSettings.PasswordResetTokenExpirationMinutes
            : 30;
    }

    private async Task<int> RevokeActiveRefreshTokensAsync(int userId, DateTime revokedAt, CancellationToken cancellationToken)
    {
        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId
                && token.RevokedAt == null
                && token.ExpiresAt > revokedAt)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAt = revokedAt;
        }

        return activeRefreshTokens.Count;
    }

    private async Task ApplyForgotPasswordMinimumResponseTimeAsync(
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        var minimumResponseTime = _authSettings.ForgotPasswordMinimumResponseMilliseconds;
        if (minimumResponseTime <= 0 || stopwatch.ElapsedMilliseconds >= minimumResponseTime)
        {
            return;
        }

        var remainingDelay = minimumResponseTime - (int)stopwatch.ElapsedMilliseconds;
        await Task.Delay(remainingDelay, cancellationToken);
    }

    private string CreatePasswordResetLink(string email, string resetToken)
    {
        var resetPasswordUrl = string.IsNullOrWhiteSpace(_authSettings.ResetPasswordUrl)
            ? "http://localhost:5173/reset-password"
            : _authSettings.ResetPasswordUrl.Trim();

        return $"{resetPasswordUrl}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";
    }

    private string CreatePasswordResetEmailBody(string resetLink)
    {
        var expirationMinutes = GetPasswordResetTokenExpirationMinutes();

        return $"""
            A password reset was requested for your Barber Shop account.

            Use this link to reset your password:
            {resetLink}

            This link expires after {expirationMinutes} minutes.

            If you did not request a password reset, you can ignore this email.
            """;
    }

    private async Task<IReadOnlyCollection<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken)
    {
        return await _dbContext.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .Select(userRole => userRole.Role.Name)
            .ToListAsync(cancellationToken);
    }

    private static bool IsUserActive(User user)
    {
        return string.IsNullOrWhiteSpace(user.Status)
            || string.Equals(user.Status, ActiveStatus, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .Replace("=", string.Empty, StringComparison.Ordinal);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
