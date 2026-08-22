using CourierPlatform.Api.Infrastructure.Database;
using CourierPlatform.Api.IntegrationTests.Fixture;
using Microsoft.EntityFrameworkCore;

namespace CourierPlatform.Api.IntegrationTests.Infrastructure.Database;

public sealed class CourierPlatformDbContextTests(PostgreSqlFixture postgres) : IClassFixture<PostgreSqlFixture>
{

    [Fact]
    public async Task ConnectsToDatabaseWithConnectionString()
    {
        var options = new DbContextOptionsBuilder<CourierPlatformDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .Options;

        await using var dbContext = new CourierPlatformDbContext(options);

        Assert.True(await dbContext.Database.CanConnectAsync());
    }
}
