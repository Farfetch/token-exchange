namespace IdentityServer4.Contrib.TokenExchange.Models
{
    using System.Collections.Generic;
    using System.Security.Claims;

    using IdentityServer4.Contrib.TokenExchange.Constants;
    using IdentityServer4.Models;
    using IdentityServer4.Validation;

    public class TokenExchangeGrantResult : GrantValidationResult
    {
        private static readonly Dictionary<string, object> ExchangeCustomResponse = new Dictionary<string, object>
        {
            { TokenExchangeConstants.ResponseParameters.IssuedTokenType, TokenExchangeConstants.TokenTypes.AccessToken }
        };

        public TokenExchangeGrantResult(Client client) 
            : base(ExchangeCustomResponse)
        {
            this.Client = client;
        }

        public TokenExchangeGrantResult(string subject, List<Claim> subjectClaims, Client client, string idp)
            : base(subject, TokenExchangeConstants.GrantTypes.TokenExchange, subjectClaims, idp, ExchangeCustomResponse)
        {
            this.Client = client;
        }

        public TokenExchangeGrantResult(TokenRequestErrors error, string errorDescription)
            : base(error, errorDescription)
        {
            this.IsError = true;
        }

        public Client Client { get; set; }
    }
}
