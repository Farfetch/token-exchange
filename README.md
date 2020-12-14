# Token Exchange

This framework extends Identity Server capabilities by implementing support for Token Exchange following the specifications defined in the [RFC 8693 - OAuth 2.0 Token Exchange](https://www.rfc-editor.org/rfc/rfc8693.html).

## Features
Currenlty, only a partial implementation of the RFC is available with focus on the security token delegation:
#### <i></i> Supported
 - Token Exchange Delegation semantics;
 - Supported request parameters: subject_token, actor_token, subject_token_type, actor_token_type; 
    - subject_token_type and actor token types are limited to urn:ietf:params:oauth:token-type:access_token;

#### <i></i> Not Supported
- Token Exchange Impersonation semantics;
- 'requested_token_type' is not supported and therefore ignored
- 'may_act' claim is not supported

#### <i></i> Extensions to the RFC
- 'client_act' claim: The prefix 'client_' is added to the 'act' claim in the response when a delegation is performed and the subject token only contains client details and no subject. This prefix is only added if your IdentityServer is configured to add the prefix "client_" to the Client claim. 


## Naming Conventions
-------------
This framework is designed taking into consideration [IdentityServer 4 contribution guilines](https://identityserver4.readthedocs.io/en/latest/intro/contributing.html) therefore the project and namespaces follow the pattern `IdentityServer4.Contrib.TokenExchange`.

## Usage
-------------

#### <i></i> Prerequisites
This framework consists of a nuget package designed to be installed and used together with an authentication server using [Identity Server 4](https://github.com/IdentityServer/IdentityServer4).

- It requires an authentication server built with Identity Server 4 >= 2.5.4

#### <i></i> Installing

For you to able to start using the Token Exchange framework, you first need to install packages in your project.
You can do it via NuGet Package Manager via the interface or the console.

```
PM> Install-Package IdentityServer4.Contrib.TokenExchange
```

#### <i></i> Configuration

The TokenExchange Framework provides a set of configurations that can be defined in the register of the framework. This configurations allows us to configure:

- ActorClaimsToInclude: Sets the claims that must be in the Actor token;
- ActorClaimsBlacklist: Sets a blacklist of claims that if they are found in the Actor Token the token will not be accepted;
- SubjectClaimsToExclude: Sets a list of claims that will be excluded from the resulting token of the exchange;

```csharp
var tokenExchangeOptions = new TokenExchangeOptions
            {
                ActorClaimsToInclude = new List<string> { TokenExchangeConstants.ClaimTypes.TenantId },
                ActorClaimsBlacklist = new List<string> { JwtClaimTypes.Subject },
                SubjectClaimsToExclude = new List<string> { JwtClaimTypes.AuthenticationMethod }
            };
	...
	...

services.AddTokenExchange(tokenExchangeOptions);
```

#### <i></i> Building locally

To build a package of Token Exchange framework locally, you can use the following commands:

```
dotnet build
dotnet pack
```
After the execution of the commands above a new package will be created at `src\IdentityServer4.Contrib.TokenExchange\bin\Debug\IdentityServer4.Contrib.TokenExchange.1.0.0.nupkg`

#### Testing locally

How to run the automated tests:

```bash
dotnet test
```

Built with
-------------

- [Dotnet Core](https://dotnet.microsoft.com/download#/current) 
- [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)

Restrictions and Cautions
-------------
Describe the restrictions and Cautions around this project

- [x] PII Compliance
- [x] GDPR Compliance


## Changelogs

See [Changelog](CHANGELOG.md)

## Contributing

Read the [Contributing guidelines](.github/CONTRIBUTING.md)

### Disclaimer

By sending us your contributions, you are agreeing that your contribution is made subject to the terms of our [Contributor Ownership Statement](https://github.com/Farfetch/.github/blob/master/COS.md)

## Maintainers

List of [Maintainers](MAINTAINERS.md)

## License

[MIT](LICENSE)
