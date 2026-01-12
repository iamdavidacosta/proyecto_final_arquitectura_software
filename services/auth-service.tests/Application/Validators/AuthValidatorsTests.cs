using AuthService.Application.DTOs;
using AuthService.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace AuthService.Tests.Application.Validators;

public class AuthValidatorsTests
{
    private readonly RegisterRequestValidator _registerValidator;
    private readonly LoginRequestValidator _loginValidator;
    private readonly RefreshTokenRequestValidator _refreshTokenValidator;

    public AuthValidatorsTests()
    {
        _registerValidator = new RegisterRequestValidator();
        _loginValidator = new LoginRequestValidator();
        _refreshTokenValidator = new RefreshTokenRequestValidator();
    }

    #region RegisterRequestValidator Tests

    [Fact]
    public void RegisterValidator_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _registerValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void RegisterValidator_InvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _registerValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("nouppercase123!")]
    [InlineData("NOLOWERCASE123!")]
    [InlineData("NoDigitsHere!")]
    public void RegisterValidator_InvalidPassword_ShouldHaveError(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _registerValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void RegisterValidator_EmptyFirstName_ShouldHaveError(string? firstName)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@example.com",
            Password = "SecurePassword123!",
            FirstName = firstName!,
            LastName = "Doe"
        };

        // Act
        var result = _registerValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void RegisterValidator_EmptyLastName_ShouldHaveError(string? lastName)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = lastName!
        };

        // Act
        var result = _registerValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    #endregion

    #region LoginRequestValidator Tests

    [Fact]
    public void LoginValidator_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "valid@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = _loginValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void LoginValidator_InvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "SecurePassword123!"
        };

        // Act
        var result = _loginValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void LoginValidator_EmptyPassword_ShouldHaveError(string? password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "valid@example.com",
            Password = password!
        };

        // Act
        var result = _loginValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region RefreshTokenRequestValidator Tests

    [Fact]
    public void RefreshTokenValidator_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token-here"
        };

        // Act
        var result = _refreshTokenValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void RefreshTokenValidator_EmptyToken_ShouldHaveError(string? token)
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = token!
        };

        // Act
        var result = _refreshTokenValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    #endregion
}
