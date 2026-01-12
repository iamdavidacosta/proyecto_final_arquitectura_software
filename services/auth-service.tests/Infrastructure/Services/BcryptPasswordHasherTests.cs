using AuthService.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AuthService.Tests.Infrastructure.Services;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _passwordHasher;

    public BcryptPasswordHasherTests()
    {
        _passwordHasher = new BcryptPasswordHasher();
    }

    [Fact]
    public void Hash_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2"); // BCrypt hash prefix
    }

    [Fact]
    public void Hash_SamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _passwordHasher.Hash(password);
        var hash2 = _passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2); // Due to salt
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(string.Empty, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("verylongpasswordthatexceeds50characters!@#$%^&*()")]
    [InlineData("password with spaces")]
    [InlineData("Contraseña con ñ y acentos áéíóú")]
    [InlineData("🔐 emoji password 🔑")]
    public void Hash_WithVariousPasswords_ShouldWorkCorrectly(string password)
    {
        // Act
        var hash = _passwordHasher.Hash(password);
        var verified = _passwordHasher.Verify(password, hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        verified.Should().BeTrue();
    }
}
