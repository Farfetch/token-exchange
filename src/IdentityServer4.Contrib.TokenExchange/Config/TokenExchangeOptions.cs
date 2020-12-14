namespace IdentityServer4.Contrib.TokenExchange.Config
{
    using System.Collections.Generic;

    using IdentityModel;

    using IdentityServer4.Contrib.TokenExchange.Constants;

    public class TokenExchangeOptions
    {
        public TokenExchangeOptions()
        {
            // defaults
            this.ActorClaimsToInclude = new List<string> { JwtClaimTypes.Subject, TokenExchangeConstants.ClaimTypes.TenantId };

            this.ActorClaimsBlacklist = new List<string>();

            this.SubjectClaimsToExclude = new List<string> { JwtClaimTypes.AuthenticationMethod };
        }

        public IEnumerable<string> ActorClaimsToInclude { get; set; }

        public IEnumerable<string> ActorClaimsBlacklist { get; set; }

        public IEnumerable<string> SubjectClaimsToExclude { get; set; }
    }
}
