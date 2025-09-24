using Amazon;
using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackFood.Lambda.Application.Interfaces;
using StackFood.Lambda.Infrastructure.Services;

namespace StackFood.Lambda
{
    public static class Startup
    {
        public static IServiceProvider BuildServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            var cognitoSettings = new CognitoSettings();
            configuration.GetSection("Cognito").Bind(cognitoSettings);

            if (cognitoSettings == null)
            {
                throw new InvalidOperationException("Cognito settings are missing or invalid in configuration.");
            }

            services.AddSingleton(cognitoSettings);

            services.AddSingleton<IAmazonCognitoIdentityProvider>(sp =>
            {
                var s = sp.GetRequiredService<CognitoSettings>();
                var region = RegionEndpoint.GetBySystemName(s.Region);
                return new AmazonCognitoIdentityProviderClient(region);
            });

            services.AddScoped<ICognitoService, CognitoService>();

            return services.BuildServiceProvider();
        }
    }
}
