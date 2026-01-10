using Microsoft.Extensions.Configuration;
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

        return elasticSearchOptions;
    }
}
