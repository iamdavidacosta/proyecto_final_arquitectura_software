using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthService.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AuthServiceImpl _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _authService = new AuthServiceImpl(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _jwtServiceMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashedPassword");

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("accessToken");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refreshToken");

        // Act
        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("accessToken");
        result.RefreshToken.Should().Be("refreshToken");
        result.Email.Should().Be(request.Email);

        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => 
            u.Email == request.Email && 
            u.FirstName == request.FirstName && 
            u.LastName == request.LastName), 
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowUserAlreadyExistsException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await _authService.Invoking(s => s.RegisterAsync(request, CancellationToken.None))
            .Should().ThrowAsync<UserAlreadyExistsException>()
            .WithMessage($"User with email {request.Email} already exists");
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedPassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("accessToken");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refreshToken");

        // Act
        var result = await _authService.LoginAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("accessToken");
        result.RefreshToken.Should().Be("refreshToken");
        result.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SecurePassword123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(request, CancellationToken.None))
            .Should().ThrowAsync<InvalidCredentialsException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedPassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(request, CancellationToken.None))
            .Should().ThrowAsync<InvalidCredentialsException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUserNotActiveException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "SecurePassword123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedPassword",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(request, CancellationToken.None))
            .Should().ThrowAsync<UserNotActiveException>()
            .WithMessage("User account is not active");
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "validRefreshToken"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedPassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = request.RefreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("newAccessToken");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("newRefreshToken");

        // Act
        var result = await _authService.RefreshTokenAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("newAccessToken");
        result.RefreshToken.Should().Be("newRefreshToken");

        _refreshTokenRepositoryMock.Verify(x => x.RevokeAsync(refreshToken.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowInvalidRefreshTokenException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "expiredRefreshToken"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedPassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = request.RefreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            IsRevoked = false
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act & Assert
        await _authService.Invoking(s => s.RefreshTokenAsync(request, CancellationToken.None))
            .Should().ThrowAsync<InvalidRefreshTokenException>()
            .WithMessage("Refresh token is expired or invalid");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ShouldThrowInvalidRefreshTokenException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "revokedRefreshToken"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashedPassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = request.RefreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true // Revoked
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act & Assert
        await _authService.Invoking(s => s.RefreshTokenAsync(request, CancellationToken.None))
            .Should().ThrowAsync<InvalidRefreshTokenException>()
            .WithMessage("Refresh token is expired or invalid");
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_WithValidUserId_ShouldRevokeAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _authService.LogoutAsync(userId, CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(
            x => x.RevokeAllUserTokensAsync(userId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
