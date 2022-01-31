namespace IdentityServer.Contrib.TokenExchange.Interfaces
{
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Validation;

    using IdentityServer.Contrib.TokenExchange.Models;

    public interface IExtensionGrantResultBuilder
    {
        TokenExchangeGrantResult Build();

        IExtensionGrantResultBuilder WithLog(string msg);

        IExtensionGrantResultBuilder WithActor(TokenValidationResult actorToken);

        IExtensionGrantResultBuilder WithSubject(TokenValidationResult subjectToken);

        IExtensionGrantResultBuilder WithError(TokenRequestErrors error, string errorDescription);
    }
}
