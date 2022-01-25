namespace Duende.IdentityServer.Contrib.TokenExchange.Validators
{
    using System.Linq;
    using System.Threading.Tasks;

    using IdentityModel;

    using Duende.IdentityServer.Contrib.TokenExchange.Config;
    using Duende.IdentityServer.Contrib.TokenExchange.Interfaces;
    using Duende.IdentityServer.Contrib.TokenExchange.Models;
    using Duende.IdentityServer.Validation;

    using Microsoft.Extensions.Logging;

    using static Duende.IdentityServer.Contrib.TokenExchange.Constants.TokenExchangeConstants;

    public class TokenExchangeRequestValidator : ITokenExchangeRequestValidator
    {
        private readonly ITokenValidator tokenValidator;
        private readonly ILogger logger;
        private readonly TokenExchangeOptions options;

        public TokenExchangeRequestValidator(ITokenValidator tokenValidator, ILogger<TokenExchangeRequestValidator> logger, TokenExchangeOptions options)
        {
            this.tokenValidator = tokenValidator;
            this.logger = logger;
            this.options = options;
        }

        public async Task<TokenExchangeValidation> ValidateAsync(ValidatedTokenRequest request)
        {
            var tokenExchangeValidation = new TokenExchangeValidation();

            var subjectTokenValidation = await this.ValidateTokenAsync(request, RequestParameters.SubjectTokenType, RequestParameters.SubjectToken).ConfigureAwait(false);
            tokenExchangeValidation.SubjectTokenValidationResult = subjectTokenValidation;

            if (subjectTokenValidation.IsError)
            {
                tokenExchangeValidation.SetErrors(subjectTokenValidation);
                return tokenExchangeValidation;
            }

            var actorTokenValidation = await this.ValidateActorTokenAsync(request).ConfigureAwait(false);
            tokenExchangeValidation.ActorTokenValidationResult = actorTokenValidation;

            if (actorTokenValidation.IsError)
            {
                tokenExchangeValidation.SetErrors(actorTokenValidation);
                return tokenExchangeValidation;
            }

            if (request.Client.ClientId != actorTokenValidation.Client.ClientId)
            {
                tokenExchangeValidation.SetErrors("Request client_id and actor_token client_id must match.");
                return tokenExchangeValidation;
            }

            return tokenExchangeValidation;
        }

        private async Task<TokenValidationResult> ValidateActorTokenAsync(ValidatedTokenRequest request)
        {
            var tokenValidationResult = await this.ValidateTokenAsync(request, RequestParameters.ActorTokenType, RequestParameters.ActorToken).ConfigureAwait(false);

            if (tokenValidationResult.IsError)
            {
                return tokenValidationResult;
            }

            return this.ValidateBlacklistedActorTokenClaims(tokenValidationResult);
        }

        private async Task<TokenValidationResult> ValidateTokenAsync(ValidatedTokenRequest request, string tokenTypeParameterName, string tokenParameterName)
        {
            var tokenType = request.Raw.Get(tokenTypeParameterName);
            if (!TokenTypes.AccessToken.Equals(tokenType))
            {
                var errorDescription = string.IsNullOrEmpty(tokenType)
                    ? $"Missing {tokenTypeParameterName}."
                    : $"Invalid {tokenTypeParameterName}.";

                return new TokenValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                    ErrorDescription = errorDescription
                };
            }

            var token = request.Raw.Get(tokenParameterName);
            if (string.IsNullOrEmpty(token))
            {
                return new TokenValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                    ErrorDescription = $"Missing {tokenParameterName}."
                };
            }

            var tokenValidationResult = await this.tokenValidator.ValidateAccessTokenAsync(token).ConfigureAwait(false);

            if (tokenValidationResult.IsError)
            {
                this.logger.LogError($"{tokenParameterName} validation failed.");
            }

            return tokenValidationResult;
        }

        private TokenValidationResult ValidateBlacklistedActorTokenClaims(TokenValidationResult tokenValidationResult)
        {
            var hasBlacklistedClaims = tokenValidationResult.Claims.Any(c => this.options.ActorClaimsBlacklist.Contains(c.Type));
            if (hasBlacklistedClaims)
            {
                return new TokenValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                    ErrorDescription = $"{RequestParameters.ActorToken} contains blacklisted claims."
                };
            }

            return tokenValidationResult;
        }
    }
}
