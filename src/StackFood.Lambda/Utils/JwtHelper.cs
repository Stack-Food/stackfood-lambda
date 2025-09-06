using StackFood.Lambda.Domain.Entities;
using System.Text;

namespace StackFood.Lambda.Utils
{
    public static class JwtHelper
    {
        public static string GenerateToken(Customer customer)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{customer.CPF}:{customer.Name}"));
        }
    }
}
