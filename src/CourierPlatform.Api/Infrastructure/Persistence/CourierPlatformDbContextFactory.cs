using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CourierPlatform.Api.Infrastructure.Persistence;

public sealed class CourierPlatformDbContextFactory
    : IDesignTimeDbContextFactory<CourierPlatformDbContext>
{
    public CourierPlatformDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CourierPlatformDbContext>()
            .UseNpgsql()
            .Options;

        return new CourierPlatformDbContext(options);
    }
}
