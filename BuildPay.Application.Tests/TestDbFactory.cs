using BuildPay.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Tests;

internal static class TestDbFactory
{
    public static BuildPayDbContext Create(string name)
    {
        var options = new DbContextOptionsBuilder<BuildPayDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new BuildPayDbContext(options);
    }
}

