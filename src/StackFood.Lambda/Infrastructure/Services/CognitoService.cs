using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using StackFood.Lambda.Application.Interfaces;
using Amazon.Lambda.Core; // para logs
using System.Text.Json;
using System.Runtime;

namespace StackFood.Lambda.Infrastructure.Services
{
    public class CognitoService : ICognitoService
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly CognitoSettings _settings;
        private readonly ILambdaLogger? _logger;

        public CognitoService(IAmazonCognitoIdentityProvider cognitoClient, CognitoSettings settings, ILambdaLogger? logger = null)
        {
            _cognitoClient = cognitoClient;
            _settings = settings;
            _logger = logger;
        }

        public async Task<string> AuthenticateAsync(string cpf,  ILambdaContext context, string? password = null)
        {
                context.Logger.LogInformation("=== Iniciando AuthenticateAsync ===");
            context.Logger.LogInformation($"CPF recebido: {cpf}");
                context.Logger.LogInformation($"UserPoolId: {_settings.UserPoolId}");
                context.Logger.LogInformation($"ClientId: {_settings.ClientId}");
                context.Logger.LogInformation($"Password usando default? {password == null}");

            try
            {
                if (string.IsNullOrEmpty(cpf))
                {
                    context.Logger.LogInformation("CPF vazio, autenticando como convidado...");
                    return await AuthenticateGuestAsync();
                }

                // Buscar usuário
                context.Logger.LogInformation("Buscando usuário no Cognito...");
                var user = await _cognitoClient.AdminGetUserAsync(new AdminGetUserRequest
                {
                    UserPoolId = _settings.UserPoolId,
                    Username = cpf
                });

                if (user == null)
                {
                    _logger?.LogWarning("Usuário não encontrado no Cognito.");
                    throw new UnauthorizedAccessException("Usuário não encontrado no Cognito");
                }
                var pass = _settings.DefaultPassword;
                context.Logger.LogInformation($"Usuário encontrado. Status: {user.UserStatus}");
                context.Logger.LogInformation($"Password DO LEMOS? {pass}");

                // Criar request de autenticação
                var request = new InitiateAuthRequest
                {
                    ClientId = _settings.ClientId,
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", cpf },
                        { "PASSWORD", _settings.DefaultPassword }
                    }
                };

                context.Logger.LogInformation("Enviando InitiateAuthAsync para Cognito...");

                var response = await _cognitoClient.InitiateAuthAsync(request);

                context.Logger.LogInformation("Autenticação Cognito concluída com sucesso.");
                context.Logger.LogInformation($"Token recebido (IdToken length): {response.AuthenticationResult?.IdToken?.Length}");

                return response.AuthenticationResult.IdToken;
            }
            catch (NotAuthorizedException ex)
            {
                _logger?.LogError($"[Cognito] Credenciais inválidas para o usuário {cpf}. Erro: {ex.Message}");
                throw;
            }
            catch (UserNotFoundException ex)
            {
                _logger?.LogError($"[Cognito] Usuário não encontrado: {cpf}. Erro: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[Cognito] Erro inesperado: {ex.GetType().Name} - {ex.Message}");
                _logger?.LogError(ex.StackTrace);
                throw;
            }
        }

        public async Task<string> AuthenticateGuestAsync()
        {
            _logger?.LogInformation("=== Iniciando AuthenticateGuestAsync ===");

            try
            {
                var request = new InitiateAuthRequest
                {
                    ClientId = _settings.ClientId,
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", _settings.GuestUsername },
                        { "PASSWORD", _settings.GuestPassword }
                    }
                };

                _logger?.LogInformation($"Autenticando usuário convidado: {_settings.GuestUsername}");

                var response = await _cognitoClient.InitiateAuthAsync(request);

                _logger?.LogInformation("Autenticação do convidado concluída com sucesso.");
                _logger?.LogInformation($"Token convidado (IdToken length): {response.AuthenticationResult?.IdToken?.Length}");

                return response.AuthenticationResult.IdToken;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[Cognito] Erro ao autenticar convidado: {ex.Message}");
                throw;
            }
        }

        public async Task<string> CreateUserAsync(string cpf, string email, string name)
        {
            _logger?.LogInformation("=== Iniciando CreateUserAsync ===");
            _logger?.LogInformation($"Criando usuário: CPF={cpf}, Email={email}, Nome={name}");

            try
            {
                var request = new AdminCreateUserRequest
                {
                    UserPoolId = _settings.UserPoolId,
                    Username = cpf,
                    UserAttributes =
                    [
                        new() { Name = "email", Value = email },
                        new() { Name = "name", Value = name },
                        new() { Name = "email_verified", Value = "true" }
                    ],
                    TemporaryPassword = _settings.DefaultPassword,
                    MessageAction = MessageActionType.SUPPRESS
                };

                await _cognitoClient.AdminCreateUserAsync(request);

                _logger?.LogInformation("Usuário criado. Definindo senha permanente...");

                await _cognitoClient.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
                {
                    UserPoolId = _settings.UserPoolId,
                    Username = cpf,
                    Password = _settings.DefaultPassword,
                    Permanent = true
                });

                _logger?.LogInformation("Senha permanente definida com sucesso.");
                return cpf;
            }
            catch (UsernameExistsException ex)
            {
                _logger?.LogError($"[Cognito] Usuário já existe: {cpf}. Erro: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[Cognito] Erro inesperado em CreateUserAsync: {ex.Message}");
                throw;
            }
        }
    }
}
