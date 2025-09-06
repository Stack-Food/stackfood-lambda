using StackFood.Lambda.Application.Interfaces;

namespace StackFood.Lambda.Infrastructure.Services
{
    public class CognitoService : ICognitoService
    {
        public Task<bool> VerifyCpfExist(string cpf)
        {
            return Task.FromResult(cpf == "12345678900");
        }
    }
}
