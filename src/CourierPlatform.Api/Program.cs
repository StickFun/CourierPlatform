using CourierPlatform.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var courierPlatformConnectionString = builder.Configuration.GetConnectionString("CourierPlatform");
ArgumentException.ThrowIfNullOrWhiteSpace(courierPlatformConnectionString, nameof(courierPlatformConnectionString));

builder.Services.AddOpenApi();
builder.Services.AddDbContext<CourierPlatformDbContext>(options =>
    options.UseNpgsql(courierPlatformConnectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
