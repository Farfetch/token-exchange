namespace IdentityServer4.Contrib.TokenExchange.Interfaces
{
    using System.Threading.Tasks;

    using IdentityServer4.Contrib.TokenExchange.Models;
    using IdentityServer4.Validation;

    public interface ITokenExchangeRequestValidator
    {
        Task<TokenExchangeValidation> ValidateAsync(ValidatedTokenRequest request);
    }
}
