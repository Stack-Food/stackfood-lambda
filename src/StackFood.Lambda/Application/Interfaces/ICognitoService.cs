namespace StackFood.Lambda.Application.Interfaces
{
    public interface ICognitoService
    {
        Task<bool> VerifyCpfExist(string cpf);
    }
}
