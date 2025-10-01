using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StackFood.Lambda.Application.Interfaces;
using StackFood.Lambda.Domain.Entities;
using System.Net;

namespace StackFood.Lambda.Application.Handlers
{
    public class CustomerHandler
    {
        private readonly ICognitoService _cognitoService;

        public CustomerHandler(ICognitoService cognitoService)
        {
            _cognitoService = cognitoService;
        }

        public async Task<APIGatewayProxyResponse> HandleCreateCustomer(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var body = JsonConvert.DeserializeObject<Customer>(request.Body ?? "{}");

                if (body is null || string.IsNullOrWhiteSpace(body.CPF) ||
                    string.IsNullOrWhiteSpace(body.Email) ||
                    string.IsNullOrWhiteSpace(body.Name))
                {
                    return BuildResponse(HttpStatusCode.BadRequest, new { message = "CPF, Email e Nome são obrigatórios." });
                }

                await _cognitoService.CreateUserAsync(body.CPF, body.Email, body.Name);

                return BuildResponse(HttpStatusCode.Created, new { message = "Cliente criado com sucesso!" });
            }
            catch (UsernameExistsException)
            {
                return BuildResponse(HttpStatusCode.Conflict, new { message = "Usuário já existe no Cognito." });
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao criar cliente: {ex.Message}");
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
