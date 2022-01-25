namespace Duende.IdentityServer.Contrib.TokenExchange.Interfaces
{
    using System.Threading.Tasks;

    using Duende.IdentityServer.Contrib.TokenExchange.Models;
    using Duende.IdentityServer.Validation;

    public interface ITokenExchangeRequestValidator
    {
        Task<TokenExchangeValidation> ValidateAsync(ValidatedTokenRequest request);
    }
}
