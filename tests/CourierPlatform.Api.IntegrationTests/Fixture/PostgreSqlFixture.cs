using Testcontainers.PostgreSql;

namespace CourierPlatform.Api.IntegrationTests.Fixture;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("courier_platform")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
