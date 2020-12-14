namespace IdentityServer4.Contrib.TokenExchange.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    using IdentityModel;

    using IdentityServer4.Contrib.TokenExchange.Constants;

    public static class ClaimsExtensions
    {
        public static string Sub(this IEnumerable<Claim> claims)
        {
            return claims.GetValue(JwtClaimTypes.Subject);
        }

        public static string Act(this IEnumerable<Claim> claims)
        {
            return claims.GetValue(TokenExchangeConstants.ClaimTypes.Act);
        }

        public static string ClientAct(this IEnumerable<Claim> claims)
        {
            return claims.GetValue(TokenExchangeConstants.ClaimTypes.ClientAct);
        }

        public static string Idp(this IEnumerable<Claim> claims)
        {
            return claims.GetValue(JwtClaimTypes.IdentityProvider);
        }

        public static string AuthenticationMethod(this IEnumerable<Claim> claims)
        {
            return claims.GetValue(JwtClaimTypes.AuthenticationMethod);
        }

        private static string GetValue(this IEnumerable<Claim> claims, string type)
        {
            var claim = claims?.SingleOrDefault(x => x.Type == type);

            return claim?.Value;
        }
    }
}
