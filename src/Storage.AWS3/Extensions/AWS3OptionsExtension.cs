using Microsoft.Extensions.Configuration;
using Storage.AWS3.Internal;
using Storage.AWS3.Models;

namespace Storage.AWS3.Extensions;
public static class AWS3OptionsExtension
{
    public static AWS3Options GetAWSConfigurationOptions(this IConfiguration configuration)
    {
        // options pattern
        var elasticSearchOptions = configuration.GetSection("AWSConfiguration").Get<AWS3Options>();
        if (elasticSearchOptions is null)
        {
            throw new Exception("Missing 'AWS Configuration' configuration section from the appsettings.");
        }

        // Access/secret key may be "ENC:..." (encrypted for production) or plaintext (local dev) —
        // Decrypt() returns non-encrypted values unchanged, so both work with the same code path.
        if (AwsConfigDecryptor.IsEncrypted(elasticSearchOptions.AWSAccessKey) || AwsConfigDecryptor.IsEncrypted(elasticSearchOptions.AWSSecretKey))
        {
            var key = configuration["Encryption:Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("AWSConfiguration keys are encrypted but 'Encryption:Key' is missing from configuration.");

            elasticSearchOptions.AWSAccessKey = AwsConfigDecryptor.Decrypt(elasticSearchOptions.AWSAccessKey, key);
            elasticSearchOptions.AWSSecretKey = AwsConfigDecryptor.Decrypt(elasticSearchOptions.AWSSecretKey, key);
        }

        return elasticSearchOptions;
    }
}
