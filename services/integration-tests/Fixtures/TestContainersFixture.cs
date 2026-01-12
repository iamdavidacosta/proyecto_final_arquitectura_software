using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;
using Xunit;

namespace IntegrationTests.Fixtures;

/// <summary>
/// Shared test fixture for integration tests that require MySQL and RabbitMQ containers.
/// This fixture is shared across test classes to reduce container startup overhead.
/// </summary>
public class TestContainersFixture : IAsyncLifetime
{
    private MySqlContainer? _mysqlContainer;
    private RabbitMqContainer? _rabbitMqContainer;

    public string MySqlConnectionString { get; private set; } = string.Empty;
    public string RabbitMqHost { get; private set; } = string.Empty;
    public int RabbitMqPort { get; private set; }
    public string RabbitMqUsername { get; private set; } = "guest";
    public string RabbitMqPassword { get; private set; } = "guest";

    public async Task InitializeAsync()
    {
        // Start MySQL container
        _mysqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("fileshare_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(3306, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
            .Build();

        await _mysqlContainer.StartAsync();
        MySqlConnectionString = _mysqlContainer.GetConnectionString();

        // Start RabbitMQ container
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.12-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithPortBinding(5672, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        await _rabbitMqContainer.StartAsync();
        RabbitMqHost = _rabbitMqContainer.Hostname;
        RabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
    }

    public async Task DisposeAsync()
    {
        if (_mysqlContainer != null)
        {
            await _mysqlContainer.DisposeAsync();
        }

        if (_rabbitMqContainer != null)
        {
            await _rabbitMqContainer.DisposeAsync();
        }
    }
}

/// <summary>
/// Collection definition for tests that share the TestContainersFixture.
/// </summary>
[CollectionDefinition("TestContainers")]
public class TestContainersCollection : ICollectionFixture<TestContainersFixture>
{
    // This class has no code, and is never created.
    // Its purpose is to be the place to apply [CollectionDefinition]
    // and all the ICollectionFixture<> interfaces.
}
