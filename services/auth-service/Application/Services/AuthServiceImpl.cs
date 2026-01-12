using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthServiceImpl> _logger;

    public AuthServiceImpl(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AuthServiceImpl> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            throw new ConflictException("Email is already registered");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid credentials for {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refresh token attempt");

        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken == null || !storedToken.IsValid())
        {
            _logger.LogWarning("Refresh token invalid or expired");
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", storedToken.UserId);
        }

        // Revoke old token
        storedToken.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<ValidateTokenResponse> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!_jwtService.ValidateToken(token, out var userId))
        {
            return new ValidateTokenResponse(false, null);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return new ValidateTokenResponse(false, null);
        }

        return new ValidateTokenResponse(true, new UserInfo(user.Id, user.Email, user.Role));
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);

        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshToken,
            expirationMinutes * 60,
            "Bearer",
            new UserInfo(user.Id, user.Email, user.Role)
        );
    }
}
