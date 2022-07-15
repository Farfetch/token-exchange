namespace Farfetch.IdentityServer.Contrib.TokenExchange.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    using Duende.IdentityServer;
    using Duende.IdentityServer.Extensions;
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
            if (!this.isError)
            {
                this.logger.LogInformation(this.successMessage);

                var act = this.BuildActClaim(this.options.ActorClaimsToInclude);

                if (this.IsClientToClientDelegation)
                {
                    var actClientClaim = new ClientClaim(act.Type, act.Value, act.ValueType);
                    this.subjectClient.Claims.Add(actClientClaim);
                    return new TokenExchangeGrantResult(this.subjectClient);
                }

                this.subjectUserClaims.Add(act);
                var filteredSubjectClaims = this.subjectUserClaims.Where(c => !this.options.SubjectClaimsToExclude.Any(t => t.Contains(c.Type))).ToList();

                return new TokenExchangeGrantResult(
                    this.subject,
                    filteredSubjectClaims,
                    this.subjectClient,
                    this.subjectUserClaims.Idp() ?? IdentityServerConstants.LocalIdentityProvider);
            }

            this.logger.LogError(this.logMessage ?? this.errorDescription);

            return new TokenExchangeGrantResult(this.error, this.errorDescription);
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

        private Claim BuildActClaim(IEnumerable<string> claimTypesToInclude)
        {
            var act = new Dictionary<string, object>();

            var existingActClaim = this.subjectUserClaims.Act();

            var lastClientId = string.Empty;

            if (!existingActClaim.IsNullOrEmpty())
            {
                var actClaimObject = JsonConvert.DeserializeObject<ActClaim>(existingActClaim, this.jsonSettings);

                lastClientId = actClaimObject.LastClientId;
            }

            if (existingActClaim.IsNullOrEmpty() || this.actorClient.ClientId != lastClientId)
            {
                act.Add(JwtClaimTypes.ClientId, this.actorClient.ClientId);
            }

            foreach (var claimType in claimTypesToInclude)
            {
                var claim = this.actorUserClaims.SingleOrDefault(c => claimType.Equals(c.Type));
                if (claim != null)
                {
                    act.Add(claimType, claim.Value);
                }
            }

            if (!string.IsNullOrEmpty(existingActClaim) && this.actorClient.ClientId != lastClientId)
            {
                this.subjectUserClaims.Remove(this.subjectUserClaims.FirstOrDefault(c => TokenExchangeConstants.ClaimTypes.Act.Equals(c.Type)));
                act.Add(TokenExchangeConstants.ClaimTypes.Act, JsonConvert.DeserializeObject(existingActClaim, this.jsonSettings));
            }

            var existingClientActClaim = this.subjectUserClaims.ClientAct();
            if (!string.IsNullOrEmpty(existingClientActClaim))
            {
                this.subjectUserClaims.Remove(this.subjectUserClaims.FirstOrDefault(c => TokenExchangeConstants.ClaimTypes.ClientAct.Equals(c.Type)));
                act.Add(TokenExchangeConstants.ClaimTypes.ClientAct, JsonConvert.DeserializeObject(existingClientActClaim, this.jsonSettings));
            }

            var actClaim = new Claim(TokenExchangeConstants.ClaimTypes.Act, JsonConvert.SerializeObject(act, this.jsonSettings), IdentityServerConstants.ClaimValueTypes.Json);
            return actClaim;
        }
    }
}
