using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackFood.Lambda.Application.Interfaces;
using StackFood.Lambda.Utils;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StackFood.Lambda;

public class Function(IServiceProvider provider)
{
    private readonly ICustomerService _customerService = provider.GetRequiredService<ICustomerService>();
    private readonly ICognitoService _cognitoService = provider.GetRequiredService<ICognitoService>();

    public Function()
        : this(Startup.BuildServiceProvider()) { }

    /// <summary>
    /// A simple function that takes a string and returns both the upper and lower case version of the string.
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Body ?? "{}");

            if (!body.TryGetValue("cpf", out var cpf) || string.IsNullOrWhiteSpace(cpf))
                return BuildResponse(HttpStatusCode.BadRequest, "CPF é obrigatório");

            var exist = await _cognitoService.VerifyCpfExist(cpf);
            if (!exist)
                return BuildResponse(HttpStatusCode.NotFound, "Cliente não encontrado");

            var customer = await _customerService.GetByCpfAsync(cpf);
            if (customer is null)
                return BuildResponse(HttpStatusCode.NotFound, "Dados do cliente não encontrados");

            var token = JwtHelper.GenerateToken(customer);

            return BuildResponse(HttpStatusCode.OK, new { customer, token });
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro interno: {ex.Message}");
            return BuildResponse(HttpStatusCode.InternalServerError, "Erro interno no servidor");
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