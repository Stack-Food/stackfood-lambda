using StackFood.Lambda.Domain.Entities;

namespace StackFood.Lambda.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer?> GetByCpfAsync(string cpf);
    }
}