using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StackFood.Lambda.Application.Interfaces;
using StackFood.Lambda.Domain.Dto;
using StackFood.Lambda.Domain.Entities;
using System.Net;

namespace StackFood.Lambda.Application.Handlers
{
    public class AuthHandler
    {
        private readonly ICognitoService _cognitoService;

        public AuthHandler(ICognitoService cognitoService)
        {
            _cognitoService = cognitoService;
        }

        public async Task<APIGatewayProxyResponse> HandleAuthAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var body = JsonConvert.DeserializeObject<AuthRequest>(request.Body ?? "{}");

                var token = await _cognitoService.AuthenticateAsync(body.CPF);

                return BuildResponse(HttpStatusCode.OK, new
                {
                    message = "Cliente autenticado com sucesso!",
                    token
                });
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao autenticar: {ex.Message}");
                return BuildResponse(HttpStatusCode.InternalServerError, new { message = "Erro interno no servidor." });
            }
        }

        private static APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode, object body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)statusCode,
                Body = JsonConvert.SerializeObject(body),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
