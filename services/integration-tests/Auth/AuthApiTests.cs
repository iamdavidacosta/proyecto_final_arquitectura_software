using FluentAssertions;
using IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace IntegrationTests.Auth;

/// <summary>
/// Integration tests for the Authentication API endpoints.
/// These tests verify the full request/response cycle including database interactions.
/// </summary>
[Collection("TestContainers")]
public class AuthApiTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly AuthWebApplicationFactory _factory;

    public AuthApiTests(AuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnTokens()
    {
        // Arrange
        var registerRequest = new
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(responseContent).RootElement;

        result.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("email").GetString().Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid()}@example.com";
        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        // Register first time
        await _client.PostAsync("/api/auth/register", content);

        // Act - Register second time with same email
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "invalid-email",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange - First register a user
        var email = $"login_{Guid.NewGuid()}@example.com";
        var password = "SecurePassword123!";

        var registerRequest = new
        {
            Email = email,
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        await _client.PostAsync("/api/auth/register", new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json"));

        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(responseContent).RootElement;

        result.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange - First register a user
        var email = $"wrongpw_{Guid.NewGuid()}@example.com";

        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        await _client.PostAsync("/api/auth/register", new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json"));

        var loginRequest = new
        {
            Email = email,
            Password = "WrongPassword456!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "AnyPassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange - First register and get tokens
        var email = $"refresh_{Guid.NewGuid()}@example.com";

        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var registerResponse = await _client.PostAsync("/api/auth/register", new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json"));

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var tokens = JsonDocument.Parse(registerContent).RootElement;
        var refreshToken = tokens.GetProperty("refreshToken").GetString();

        var refreshRequest = new
        {
            RefreshToken = refreshToken
        };

        var content = new StringContent(
            JsonSerializer.Serialize(refreshRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(responseContent).RootElement;

        result.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBe(refreshToken); // New token
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new
        {
            RefreshToken = "invalid-refresh-token"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(refreshRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

/// <summary>
/// Custom WebApplicationFactory for Auth Service integration tests.
/// Configures the test environment with in-memory or test container databases.
/// </summary>
public class AuthWebApplicationFactory : WebApplicationFactory<AuthService.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AuthService.Infrastructure.Persistence.AuthDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using an in-memory database for testing
            services.AddDbContext<AuthService.Infrastructure.Persistence.AuthDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestAuthDb_" + Guid.NewGuid());
            });

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AuthService.Infrastructure.Persistence.AuthDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
