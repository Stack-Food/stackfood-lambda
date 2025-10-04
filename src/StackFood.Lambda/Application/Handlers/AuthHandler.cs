using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using StackFood.Lambda.Application.Interfaces;
using StackFood.Lambda.Domain.Dto;
using System.Net;
using System.Text;

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
            context.Logger.LogInformation("=== Iniciando AuthHandler ===");

            try
            {
                // Log geral da requisição
                context.Logger.LogInformation($"Path recebido: {request.Path}");
                context.Logger.LogInformation($"HTTP Method: {request.HttpMethod}");
                context.Logger.LogInformation($"isBase64Encoded: {request.IsBase64Encoded}");
                context.Logger.LogInformation($"Headers: {JsonConvert.SerializeObject(request.Headers)}");

                // Decodificar body se vier em base64
                string requestBody = request.IsBase64Encoded
                    ? Encoding.UTF8.GetString(Convert.FromBase64String(request.Body))
                    : request.Body;

                context.Logger.LogInformation($"Body recebido (raw): {request.Body}");
                context.Logger.LogInformation($"Body decodificado: {requestBody}");

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    context.Logger.LogWarning("Corpo da requisição está vazio.");
                    return BuildResponse(HttpStatusCode.BadRequest, new { message = "Body vazio." });
                }

                // Desserializar corpo
                AuthRequest? body = null;
                try
                {
                    body = JsonConvert.DeserializeObject<AuthRequest>(requestBody);
                }
                catch (Exception jsonEx)
                {
                    context.Logger.LogError($"Erro ao desserializar JSON: {jsonEx.Message}");
                    return BuildResponse(HttpStatusCode.BadRequest, new { message = "JSON inválido." });
                }

                if (body == null || string.IsNullOrWhiteSpace(body.CPF))
                {
                    context.Logger.LogWarning($"Campo CPF ausente ou inválido. Body desserializado: {JsonConvert.SerializeObject(body)}");
                    return BuildResponse(HttpStatusCode.BadRequest, new { message = "Campo CPF é obrigatório." });
                }

                context.Logger.LogInformation($"Tentando autenticar usuário com CPF: {body.CPF}");

                var token = await _cognitoService.AuthenticateAsync(body.CPF);

                context.Logger.LogInformation("Autenticação concluída com sucesso.");

                return BuildResponse(HttpStatusCode.OK, new
                {
                    message = "Cliente autenticado com sucesso!",
                    token
                });
            }
            catch (Exception ex)
            {
                // Logar erro completo (tipo + mensagem + stacktrace)
                context.Logger.LogError($"[ERRO] Tipo: {ex.GetType().Name}");
                context.Logger.LogError($"[ERRO] Mensagem: {ex.Message}");
                context.Logger.LogError($"[ERRO] StackTrace: {ex.StackTrace}");

                return BuildResponse(HttpStatusCode.InternalServerError, new
                {
                    message = "Erro interno no servidor.",
                    exception = ex.Message
                });
            }
        }

        private static APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode, object body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)statusCode,
                Body = JsonConvert.SerializeObject(body),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }
}
