using Microsoft.EntityFrameworkCore;

namespace CourierPlatform.Api.Infrastructure.Database;

public class CourierPlatformDbContext(DbContextOptions<CourierPlatformDbContext> options)
    : DbContext(options)
{
}
