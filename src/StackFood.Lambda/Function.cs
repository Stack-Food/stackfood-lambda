using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using StackFood.Lambda.Application.Handlers;

// Assembly attribute to enable Lambda JSON serialization
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StackFood.Lambda
{
    public class Function
    {
        private readonly AuthHandler _authHandler;
        private readonly CustomerHandler _customerHandler;

        public Function()
        {
            var provider = Startup.BuildServiceProvider();
            _authHandler = ActivatorUtilities.CreateInstance<AuthHandler>(provider);
            _customerHandler = ActivatorUtilities.CreateInstance<CustomerHandler>(provider);
        }

        /// <summary>
        /// Handler principal da Lambda
        /// Roteia requests baseado no path
        /// </summary>
        public Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Path))
            {
                context.Logger.LogError("Path is null or empty");
                return Task.FromResult(new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Invalid request: path is missing",
                });
            }

            return request.Path.ToLower() switch
            {
                "/auth" => _authHandler.HandleAuthAsync(request, context),
                "/customer" => _customerHandler.HandleCreateCustomer(request, context),
                _ => Task.FromResult(new APIGatewayProxyResponse
                {
                    StatusCode = 404,
                    Body = "Endpoint not found",
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                })
            };
        }

    }
}
