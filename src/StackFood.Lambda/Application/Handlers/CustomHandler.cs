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
                context.Logger.LogInformation("Iniciando a criação do cliente.");

                // Logando a entrada do corpo da requisição
                context.Logger.LogInformation($"Corpo da requisição: {request.Body}");

                var body = JsonConvert.DeserializeObject<Customer>(request.Body ?? "{}");

                if (body is null)
                {
                    context.Logger.LogWarning("O corpo da requisição está vazio ou malformado.");
                    return BuildResponse(HttpStatusCode.BadRequest, new { message = "Corpo da requisição inválido." });
                }

                // Verificando e logando se algum campo obrigatório está faltando
                if (string.IsNullOrWhiteSpace(body.CPF) ||
                    string.IsNullOrWhiteSpace(body.Email) ||
                    string.IsNullOrWhiteSpace(body.Name))
                {
                    context.Logger.LogWarning($"Dados obrigatórios faltando: CPF: {body.CPF}, Email: {body.Email}, Nome: {body.Name}");
                    return BuildResponse(HttpStatusCode.BadRequest, new { message = "CPF, Email e Nome são obrigatórios." });
                }

                context.Logger.LogInformation("Tentando criar o cliente no Cognito.");

                await _cognitoService.CreateUserAsync(body.CPF, body.Email, body.Name);

                context.Logger.LogInformation("Cliente criado com sucesso!");

                return BuildResponse(HttpStatusCode.Created, new { message = "Cliente criado com sucesso!" });
            }
            catch (UsernameExistsException ex)
            {
                // Logando o erro específico de conflito de usuário
                context.Logger.LogError($"Erro ao criar cliente: Usuário já existe no Cognito. {ex.Message}");
                return BuildResponse(HttpStatusCode.Conflict, new { message = "Usuário já existe no Cognito." });
            }
            catch (Exception ex)
            {
                // Logando qualquer outro erro geral
                context.Logger.LogError($"Erro inesperado ao criar cliente: {ex.Message}");

                // Se for um erro relacionado ao Cognito, logamos um erro específico
                if (ex.Message.Contains("User pool") || ex.Message.Contains("does not exist"))
                {
                    context.Logger.LogError("Erro específico: O pool de usuários do Cognito não existe ou não está configurado corretamente.");
                }

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
