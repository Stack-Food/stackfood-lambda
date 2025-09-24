using StackFood.Lambda.Application.Interfaces;
using StackFood.Lambda.Domain.Entities;

namespace StackFood.Lambda.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICognitoService _cognitoService;

        public CustomerService(ICognitoService cognitoService)
        {
            _cognitoService = cognitoService;
        }

        public Task<Customer?> GetByCpfAsync(string cpf)
        {
            if (cpf is not null)
            {
                return Task.FromResult<Customer?>(new Customer
                {
                    CPF = cpf,
                    Name = "João Teste",
                    Email = "joao@teste.com"
                });
            }

            return Task.FromResult<Customer?>(null);
        }
    }
}
