﻿namespace IdentityServer4.Contrib.TokenExchange
{
    using System.Threading.Tasks;

    using IdentityServer4.Contrib.TokenExchange.Constants;
    using IdentityServer4.Contrib.TokenExchange.Interfaces;
    using IdentityServer4.Validation;

    public class TokenExchangeGrant : IExtensionGrantValidator
    {
        private readonly ITokenExchangeRequestValidator requestValidator;
        private readonly IExtensionGrantResultBuilder resultBuilder;

        public TokenExchangeGrant(ITokenExchangeRequestValidator requestValidator, IExtensionGrantResultBuilder resultBuilder)
        {
            this.requestValidator = requestValidator;
            this.resultBuilder = resultBuilder;
        }

        public string GrantType => TokenExchangeConstants.GrantTypes.TokenExchange;

        public virtual async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var requestValidationResult = await this.requestValidator.ValidateAsync(context.Request).ConfigureAwait(false);

            if (requestValidationResult.IsError)
            {
                context.Result = this.resultBuilder
                    .WithLog(requestValidationResult.ErrorDescription)
                    .WithError(requestValidationResult.Error, requestValidationResult.ErrorDescription)
                    .Build();
                return;
            }

            var tokenExchangeResult = this.resultBuilder
                .WithSubject(requestValidationResult.SubjectTokenValidationResult)
                .WithActor(requestValidationResult.ActorTokenValidationResult)
                .Build();

            context.Result = tokenExchangeResult;
            context.Request.SetClient(tokenExchangeResult.Client);
        }
    }
}
