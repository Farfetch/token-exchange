namespace Microsoft.Extensions.DependencyInjection
{
    using Duende.IdentityServer.Contrib.TokenExchange;
    using Duende.IdentityServer.Contrib.TokenExchange.Builders;
    using Duende.IdentityServer.Contrib.TokenExchange.Config;
    using Duende.IdentityServer.Contrib.TokenExchange.Interfaces;
    using Duende.IdentityServer.Contrib.TokenExchange.Validators;
    using Duende.IdentityServer.Validation;

    public static class TokenExchangeDependencyInjectionExtensions
    {
        /// <summary>
        /// Adds the support for token exchange grant.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddTokenExchange(this IIdentityServerBuilder builder)
        {
            builder.Services.AddSingleton<TokenExchangeOptions>();
            builder.Services.AddScoped<IExtensionGrantResultBuilder, TokenExchangeResultBuilder>();
            builder.Services.AddScoped<ITokenExchangeRequestValidator, TokenExchangeRequestValidator>();
            builder.Services.AddScoped<IExtensionGrantValidator, TokenExchangeGrant>();

            return builder;
        }

        /// <summary>
        /// Adds the support for token exchange grant.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="options">Defines the options for the token-exchange setup.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddTokenExchange(this IIdentityServerBuilder builder, TokenExchangeOptions options)
        {
            builder.Services.AddSingleton(options);
            builder.Services.AddScoped<IExtensionGrantResultBuilder, TokenExchangeResultBuilder>();
            builder.Services.AddScoped<ITokenExchangeRequestValidator, TokenExchangeRequestValidator>();
            builder.Services.AddScoped<IExtensionGrantValidator, TokenExchangeGrant>();

            return builder;
        }
    }
}
