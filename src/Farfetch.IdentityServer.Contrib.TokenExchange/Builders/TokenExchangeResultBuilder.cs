namespace Farfetch.IdentityServer.Contrib.TokenExchange.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    using Duende.IdentityServer;
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Validation;

    using Farfetch.IdentityServer.Contrib.TokenExchange.Config;
    using Farfetch.IdentityServer.Contrib.TokenExchange.Constants;
    using Farfetch.IdentityServer.Contrib.TokenExchange.Extensions;
    using Farfetch.IdentityServer.Contrib.TokenExchange.Interfaces;
    using Farfetch.IdentityServer.Contrib.TokenExchange.Models;

    using IdentityModel;

    using IdentityServer4.Contrib.TokenExchange.Models;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using Newtonsoft.Json.Linq;

    public class TokenExchangeResultBuilder : IExtensionGrantResultBuilder
    {
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        private readonly string successMessage = "Successful Token Exchange Request.";
        private readonly ILogger logger;
        private readonly TokenExchangeOptions options;

        private string logMessage;
        
        private TokenRequestErrors error = TokenRequestErrors.InvalidRequest;
        private bool isError = true;
        private string errorDescription = "Invalid Token Exchange Request";

        private Client actorClient;
        private Client subjectClient;

        private string subject;
        private List<Claim> subjectUserClaims;
        private List<Claim> actorUserClaims;

        public TokenExchangeResultBuilder(ILogger<TokenExchangeResultBuilder> logger, TokenExchangeOptions options)
        {
            this.logger = logger;
            this.options = options;
            this.subjectUserClaims = new List<Claim>();
            this.actorUserClaims = new List<Claim>();
        }
        
        private bool IsClientToClientDelegation => !this.isError && string.IsNullOrEmpty(this.subject);

        public TokenExchangeGrantResult Build()
        {
            if (this.isError)
            {
                this.logger.LogError(this.logMessage ?? this.errorDescription);

                return new TokenExchangeGrantResult(this.error, this.errorDescription);
            }

            this.logger.LogInformation(this.successMessage);

            if (this.IsClientToClientDelegation)
            {
                this.BuildClientActClaim(this.options.ActorClaimsToInclude);
                return new TokenExchangeGrantResult(this.subjectClient);
            }

            this.BuildActClaim(this.options.ActorClaimsToInclude);

            var filteredSubjectClaims = this.subjectUserClaims.Where(c => !this.options.SubjectClaimsToExclude.Any(t => t.Contains(c.Type))).ToList();

            return new TokenExchangeGrantResult(
                this.subject,
                filteredSubjectClaims,
                this.subjectClient,
                this.subjectUserClaims.Idp() ?? IdentityServerConstants.LocalIdentityProvider);
        }

        public IExtensionGrantResultBuilder WithLog(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                this.logMessage = msg;
            }

            return this;
        }

        public IExtensionGrantResultBuilder WithActor(TokenValidationResult actorToken)
        {
            if (actorToken?.Claims == null || actorToken.Client == null)
            {
                throw new InvalidOperationException("Client proprieties are missing");
            }

            this.isError = false;

            this.actorClient = actorToken.Client;
            this.actorUserClaims = actorToken.Claims.ToList();

            return this;
        }

        public IExtensionGrantResultBuilder WithSubject(TokenValidationResult subjectToken)
        {
            if (subjectToken?.Claims == null || subjectToken.Client?.Claims == null)
            {
                throw new InvalidOperationException("Subject proprieties are missing");
            }

            this.isError = false;

            this.subject = subjectToken.Claims.Sub();
            this.subjectUserClaims = subjectToken.Claims.ToList();
            this.subjectClient = subjectToken.Client;

            return this;
        }

        public IExtensionGrantResultBuilder WithError(TokenRequestErrors error, string errorDescription)
        {
            this.error = error;
            this.errorDescription = errorDescription;
            return this;
        }

        private void BuildActClaim(IEnumerable<string> claimTypesToInclude)
        {
            var act = AddClaimsFromClaimTypes(this.actorUserClaims, claimTypesToInclude);

            var lastClientId = GetClientIdFromActorIfNotLast(this.subjectUserClaims.Act());

            if (string.IsNullOrEmpty(lastClientId))
            {
                // This piece of code is necessary because of an unexpected behavior of GrantValidationResult class (From Duende).
                // That is: When nothing is changed and just is returned what was receive, the content of the Act Claim is returned as a string type which makes the token get wrong.
                // To solve that we get the Act Claim content before remove itself and adds it with the correct type (json)
                var currentAct = this.subjectUserClaims.Act();

                this.subjectUserClaims.Remove(this.subjectUserClaims.FirstOrDefault(c => TokenExchangeConstants.ClaimTypes.Act.Equals(c.Type)));

                this.subjectUserClaims.Add(
                        new Claim(
                            TokenExchangeConstants.ClaimTypes.Act,
                            currentAct,
                            IdentityServerConstants.ClaimValueTypes.Json
                        )
                    );

                return;
            }

            act.TryAddNonEmptyString(JwtClaimTypes.ClientId, lastClientId);
            act.TryAddNonEmptyJObject(TokenExchangeConstants.ClaimTypes.Act, GetFromExistingClaim(TokenExchangeConstants.ClaimTypes.Act, this.subjectUserClaims.Act()));

            var newAct = new Claim(
                TokenExchangeConstants.ClaimTypes.Act,
                JsonConvert.SerializeObject(act, this.jsonSettings),
                IdentityServerConstants.ClaimValueTypes.Json
                );

            this.subjectUserClaims.Add(newAct);
        }

        private void BuildClientActClaim(IEnumerable<string> claimTypesToInclude)
        {
            var act = AddClaimsFromClaimTypes(this.actorUserClaims, claimTypesToInclude);

            var lastClientId = GetClientIdFromActorIfNotLast(this.subjectUserClaims.ClientAct());

            if (string.IsNullOrEmpty(lastClientId))
            {
                this.subjectClient.Claims.Add(new ClientClaim(TokenExchangeConstants.ClaimTypes.Act, 
                    this.subjectUserClaims.ClientAct(), IdentityServerConstants.ClaimValueTypes.Json));
                return;
            }

            act.TryAddNonEmptyString(JwtClaimTypes.ClientId, lastClientId);
            act.TryAddNonEmptyJObject(TokenExchangeConstants.ClaimTypes.ClientAct, GetFromExistingClaim(TokenExchangeConstants.ClaimTypes.ClientAct, this.subjectUserClaims.ClientAct()));

            var newClientAct = new ClientClaim(
                TokenExchangeConstants.ClaimTypes.Act, 
                JsonConvert.SerializeObject(act, this.jsonSettings), 
                IdentityServerConstants.ClaimValueTypes.Json
                );

            this.subjectClient.Claims.Add(newClientAct);
        }

        private Dictionary<string, object> AddClaimsFromClaimTypes(List<Claim> claims, IEnumerable<string> claimTypesToInclude)
        {
            var act = new Dictionary<string, object>();
            foreach (var claimType in claimTypesToInclude)
            {
                var claim = claims.SingleOrDefault(c => claimType.Equals(c.Type));
                if (claim != null)
                {
                    act.Add(claimType, claim.Value);
                }
            }

            return act;
        }

        private string GetClientIdFromActorIfNotLast(string claims)
        {
            if (string.IsNullOrEmpty(claims))
            {
                return this.actorClient.ClientId;
            }

            if (!IsLastClientId(claims))
            {
                return this.actorClient.ClientId;
            }

            return string.Empty;
        }

        private bool IsLastClientId(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            var actClaim = JsonConvert.DeserializeObject<ActClaim>(data, this.jsonSettings);

            var isLastId = !string.IsNullOrEmpty(actClaim?.ClientId) &&
                           actClaim.ClientId.Equals(this.actorClient.ClientId);

            return isLastId;
        }

        private JObject GetFromExistingClaim(string claimType, string claims)
        {
            if (string.IsNullOrEmpty(claims))
            {
                return new JObject();
            }

            this.subjectUserClaims.Remove(this.subjectUserClaims.FirstOrDefault(c => c.Type.Equals(claimType)));

            return JsonConvert.DeserializeObject<JObject>(claims, this.jsonSettings);
        }
    }
}
