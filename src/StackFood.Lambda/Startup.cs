using Microsoft.Extensions.DependencyInjection;
using StackFood.Lambda.Application.Interfaces;
using StackFood.Lambda.Infrastructure.Services;

namespace StackFood.Lambda
{
    public static class Startup
    {
        public static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICognitoService, CognitoService>();

            return services.BuildServiceProvider();
        }
    }
}
