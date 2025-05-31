using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Play.Common.Settings;

namespace Play.Common.Configuration
{
    public static class Extensions
    {
        public static IHostBuilder ConfigureAzureKeyVault(this IHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                if (context.HostingEnvironment.IsProduction())
                {
                    var config = configBuilder.Build();
                    var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

                    configBuilder.AddAzureKeyVault(
                        new Uri($"https://{serviceSettings.KeyVaultName}.vault.azure.net/"),
                        new DefaultAzureCredential()
                    );
                }
            });
        }
    }
}