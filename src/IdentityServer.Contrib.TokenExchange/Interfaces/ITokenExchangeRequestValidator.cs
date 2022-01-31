namespace IdentityServer.Contrib.TokenExchange.Interfaces
{
    using System.Threading.Tasks;

    using Duende.IdentityServer.Validation;

    using IdentityServer.Contrib.TokenExchange.Models;

    public interface ITokenExchangeRequestValidator
    {
        Task<TokenExchangeValidation> ValidateAsync(ValidatedTokenRequest request);
    }
}
