namespace IdentityServer4.Contrib.TokenExchange.Interfaces
{
    using IdentityServer4.Contrib.TokenExchange.Models;
    using IdentityServer4.Models;
    using IdentityServer4.Validation;

    public interface IExtensionGrantResultBuilder
    {
        TokenExchangeGrantResult Build();

        IExtensionGrantResultBuilder WithLog(string msg);

        IExtensionGrantResultBuilder WithActor(TokenValidationResult actorToken);

        IExtensionGrantResultBuilder WithSubject(TokenValidationResult subjectToken);

        IExtensionGrantResultBuilder WithError(TokenRequestErrors error, string errorDescription);
    }
}
