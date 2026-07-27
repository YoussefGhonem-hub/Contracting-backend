using Contracting.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Contracting.Infrustructure.Persistence;
public class AppDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appsettings.json");
        var config = configBuilder.Build();
        var connectionString = config.GetValue<string>("ConnectionStrings:DefaultConnection");

        // Connection string may be "ENC:..." (encrypted for production/uat) or plaintext (local dev) —
        // mirrors the same decrypt-if-needed logic used at runtime in DependencyInjection.AddInfrastructure.
        if (!string.IsNullOrEmpty(connectionString) && AppSettingsProtector.IsEncrypted(connectionString))
        {
            var encryptionKey = config.GetValue<string>("Encryption:Key");
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is encrypted but 'Encryption:Key' is missing from configuration.");
            connectionString = AppSettingsProtector.Decrypt(connectionString, encryptionKey);
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);


        return new ApplicationDbContext(optionsBuilder.Options);
    }
}