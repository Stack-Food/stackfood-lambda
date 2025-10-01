using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using StackFood.Lambda.Application.Interfaces;

namespace StackFood.Lambda.Infrastructure.Services
{
    public class CognitoService : ICognitoService
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly CognitoSettings _settings;

        public CognitoService(IAmazonCognitoIdentityProvider cognitoClient, CognitoSettings settings)
        {
            _cognitoClient = cognitoClient;
            _settings = settings;
        }

        public async Task<string> AuthenticateAsync(string cpf, string? password = null)
        {
            if (String.IsNullOrEmpty(cpf)) 
            {
                return await AuthenticateGuestAsync();
            }

            var user = await _cognitoClient.AdminGetUserAsync(new AdminGetUserRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = cpf
            });

            if (user == null)
            {
                throw new UnauthorizedAccessException("Usuário não encontrado no Cognito");
            }

            var request = new InitiateAuthRequest
            {
                ClientId = _settings.ClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", cpf },
                    { "PASSWORD", password ?? _settings.DefaultPassword }
                }
            };

            var response = await _cognitoClient.InitiateAuthAsync(request);

            return response.AuthenticationResult.IdToken;
        }

        public async Task<string> AuthenticateGuestAsync()
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

            var response = await _cognitoClient.InitiateAuthAsync(request);
            return response.AuthenticationResult.IdToken;
        }

        public async Task<string> CreateUserAsync(string cpf, string email, string name)
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
                TemporaryPassword = "123#Temporary",
                MessageAction = MessageActionType.SUPPRESS
            };

            await _cognitoClient.AdminCreateUserAsync(request);

            await _cognitoClient.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = cpf,
                Password = _settings.DefaultPassword,
                Permanent = true
            });

            return cpf;
        }
    }
}
