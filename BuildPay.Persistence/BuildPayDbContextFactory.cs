using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BuildPay.Persistence;

public class BuildPayDbContextFactory : IDesignTimeDbContextFactory<BuildPayDbContext>
{
    public BuildPayDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../BuildPay.API");
        
        // If the API directory doesn't exist (e.g., when running from the API folder), use current directory
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var builder = new DbContextOptionsBuilder<BuildPayDbContext>();
        var connectionString = "Host=localhost;Database=buildpay_db;Username=postgres;Password=123456!";

        builder.UseNpgsql(connectionString);

        return new BuildPayDbContext(builder.Options);
    }
}
