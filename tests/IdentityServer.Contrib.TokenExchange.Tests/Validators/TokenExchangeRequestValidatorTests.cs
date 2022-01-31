namespace IdentityServer.Contrib.TokenExchange.Tests.Validators
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Validation;

    using IdentityServer.Contrib.TokenExchange.Config;
    using IdentityServer.Contrib.TokenExchange.Constants;
    using IdentityServer.Contrib.TokenExchange.Tests.Extensions;
    using IdentityServer.Contrib.TokenExchange.Validators;

    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using static IdentityServer.Contrib.TokenExchange.Constants.TokenExchangeConstants;

    [TestClass]
    public class TokenExchangeRequestValidatorTests
    {
        private const string ValidSubjectToken = "valid_subject_token";
        private const string ValidActorToken = "valid_actor_token";
        private const string InvalidSubjectToken = "invalid_subject_token";
        private const string InvalidActorToken = "invalid_actor_token";
        private const string InvalidTokenType = "invalid_token_type";

        private readonly Client client = new Client { ClientId = "testClient", AllowedScopes = new List<string> { "dummyScope" } };

        private readonly TokenExchangeOptions tokenExchangeOptions = new TokenExchangeOptions
        {
            ActorClaimsBlacklist = new List<string> { "blacklistedClaim" }
        };

        private Mock<ITokenValidator> tokenValidatorMock;
        private Mock<ILogger<TokenExchangeRequestValidator>> loggerMock;
        private TokenExchangeRequestValidator target;

        [TestInitialize]
        public void Setup()
        {
            this.tokenValidatorMock = new Mock<ITokenValidator>();
            this.loggerMock = new Mock<ILogger<TokenExchangeRequestValidator>>();
            this.ArrangeSuccessAccessTokensValidation(ValidSubjectToken, ValidActorToken);
            this.ArrangeFailedAccessTokensValidation(InvalidSubjectToken, InvalidActorToken);

            this.target = new TokenExchangeRequestValidator(this.tokenValidatorMock.Object, this.loggerMock.Object, this.tokenExchangeOptions);
        }

        [TestMethod]
        public async Task ValidateAsync_ValidRequest_ReturnsValidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, TokenTypes.AccessToken, ValidActorToken, TokenTypes.AccessToken);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsFalse(act.IsError);
        }

        [TestMethod]
        public async Task ValidateAsync_BlacklistedClaimInActorToken_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, TokenTypes.AccessToken, ValidActorToken, TokenTypes.AccessToken);
            this.tokenValidatorMock
                .Setup(x => x.ValidateAccessTokenAsync(ValidActorToken, It.IsAny<string>()))
                .ReturnsAsync(
                    new TokenValidationResult
                    {
                        Client = new Client { ClientId = "testClient", AllowedScopes = new List<string> { "dummyScope" } },
                        IsError = false,
                        Claims = new List<Claim>
                        {
                            new Claim("blacklistedClaim", "12345")
                        }
                    });

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual("invalid_token : actor_token contains blacklisted claims.", act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_ClientIdDifferentFromActorClientId_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, TokenTypes.AccessToken, ValidActorToken, TokenTypes.AccessToken);
            request.Client.ClientId = "invalidClientId";

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual("Request client_id and actor_token client_id must match.", act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_NullSubjectTokenType_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, null, ValidActorToken, TokenTypes.AccessToken);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual("invalid_token : Missing subject_token_type.", act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_InvalidSubjectTokenType_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, InvalidTokenType, ValidActorToken, TokenTypes.AccessToken);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual("invalid_token : Invalid subject_token_type.", act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_NullActorTokenType_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, TokenTypes.AccessToken, ValidActorToken, null);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual("invalid_token : Missing actor_token_type.", act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_InvalidActorTokenType_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, TokenTypes.AccessToken, ValidActorToken, InvalidTokenType);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual("invalid_token : Invalid actor_token_type.", act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_NullSubjectToken_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var request = this.CreateValidatedTokenRequest(null, TokenTypes.AccessToken, ValidActorToken, TokenTypes.AccessToken);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual("invalid_token : Missing subject_token.", act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_NullActorToken_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var expectedErrorDescription = "invalid_token : Missing actor_token.";

            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, TokenTypes.AccessToken, null, TokenTypes.AccessToken);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual(expectedErrorDescription, act.ErrorDescription);
        }

        [TestMethod]
        public async Task ValidateAsync_InvalidSubjectToken_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var expectedErrorDescription = "invalid_token : Token validation failed.";
            var request = this.CreateValidatedTokenRequest(InvalidSubjectToken, TokenTypes.AccessToken, ValidActorToken, TokenTypes.AccessToken);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual(expectedErrorDescription, act.ErrorDescription);
            this.loggerMock.VerifyLogError("subject_token validation failed.");
        }

        [TestMethod]
        public async Task ValidateAsync_InvalidActorToken_ReturnsInvalidGrantValidationResult()
        {
            // Arrange
            var expectedErrorDescription = "invalid_token : Token validation failed.";
            var request = this.CreateValidatedTokenRequest(ValidSubjectToken, TokenTypes.AccessToken, InvalidActorToken, TokenTypes.AccessToken);

            // Act
            var act = await this.target.ValidateAsync(request).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(act);
            Assert.IsTrue(act.IsError);
            Assert.AreEqual(TokenRequestErrors.InvalidRequest, act.Error);
            Assert.AreEqual(expectedErrorDescription, act.ErrorDescription);
            this.loggerMock.VerifyLogError("actor_token validation failed.");
        }

        private ValidatedTokenRequest CreateValidatedTokenRequest(string subjectToken, string subjectTokenType, string actorToken, string actorTokenType)
        {
            var requestParameters = new NameValueCollection
            {
                { "subject_token", subjectToken },
                { "subject_token_type", subjectTokenType },
                { "actor_token", actorToken },
                { "actor_token_type", actorTokenType }
            };

            var request = new ValidatedTokenRequest
            {
                GrantType = TokenExchangeConstants.GrantTypes.TokenExchange,
                Client = this.client,
                Raw = requestParameters,
            };
            return request;
        }

        private void ArrangeSuccessAccessTokensValidation(string subjectToken, string actorToken)
        {
            this.tokenValidatorMock
                .Setup(x => x.ValidateAccessTokenAsync(subjectToken, It.IsAny<string>()))
                .ReturnsAsync(
                    new TokenValidationResult
                    {
                        IsError = false,
                        Claims = new List<Claim>()
                    });

            this.tokenValidatorMock
                .Setup(x => x.ValidateAccessTokenAsync(actorToken, It.IsAny<string>()))
                .ReturnsAsync(
                    new TokenValidationResult
                    {
                        Client = new Client { ClientId = "testClient", AllowedScopes = new List<string> { "dummyScope" } },
                        IsError = false,
                        Claims = new List<Claim>()
                    });
        }

        private void ArrangeFailedAccessTokensValidation(string subjectToken, string actorToken)
        {
            this.tokenValidatorMock
                .Setup(x => x.ValidateAccessTokenAsync(subjectToken, It.IsAny<string>()))
                .ReturnsAsync(
                    new TokenValidationResult
                    {
                        IsError = true,
                        Error = "invalid_token",
                        Claims = new List<Claim>()
                    });

            this.tokenValidatorMock
                .Setup(x => x.ValidateAccessTokenAsync(actorToken, It.IsAny<string>()))
                .ReturnsAsync(
                    new TokenValidationResult
                    {
                        IsError = true,
                        Error = "invalid_token",
                        Claims = new List<Claim>()
                    });
        }
    }
}
