using Amazon.Lambda.Core;

namespace StackFood.Lambda.Application.Interfaces
{
    public interface ICognitoService
    {
        Task<string> CreateUserAsync(string cpf, string email, string name);

        /// <summary>
        /// Autentica um cliente no Cognito a partir do CPF.
        /// Retorna o token JWT caso seja encontrado.
        /// </summary>
        Task<string> AuthenticateAsync(string? cpf, ILambdaContext context, string? password = null );
    }
}
