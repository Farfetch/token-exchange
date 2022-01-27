namespace Duende.IdentityServer.Contrib.TokenExchange.Interfaces
{
    using Duende.IdentityServer.Contrib.TokenExchange.Models;
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Validation;

    public interface IExtensionGrantResultBuilder
    {
        TokenExchangeGrantResult Build();

        IExtensionGrantResultBuilder WithLog(string msg);

        IExtensionGrantResultBuilder WithActor(TokenValidationResult actorToken);

        IExtensionGrantResultBuilder WithSubject(TokenValidationResult subjectToken);

        IExtensionGrantResultBuilder WithError(TokenRequestErrors error, string errorDescription);
    }
}
