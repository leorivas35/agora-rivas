using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public sealed class AgoraRivasContextFactory : IDesignTimeDbContextFactory<AgoraRivasContext>
{
    public AgoraRivasContext CreateDbContext(string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<AgoraRivasContextFactory>(optional: true) // local dev
            .Build();

                   

        // Only if you need to run migrations against a remote DB using a vault secret
        // var vaultUri = config["KeyVaultUri"];
        // if (!string.IsNullOrEmpty(vaultUri))
        // {
        //     builder.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
        //     configuration = builder.Build(); // rebuild so the vault source is included
        // }            

        string connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionBuilder = new DbContextOptionsBuilder<AgoraRivasContext>();     
        optionBuilder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AgoraRivasContextFactory).Assembly.FullName));       
        
        return new AgoraRivasContext(optionBuilder.Options);
    }
}
