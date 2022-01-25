namespace Duende.IdentityServer.Contrib.TokenExchange.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using IdentityModel;

    using Duende.IdentityServer.Contrib.TokenExchange.Builders;
    using Duende.IdentityServer.Contrib.TokenExchange.Config;
    using Duende.IdentityServer.Contrib.TokenExchange.Extensions;
    using Duende.IdentityServer.Contrib.TokenExchange.Interfaces;
    using Duende.IdentityServer.Contrib.TokenExchange.Models;
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Validation;

    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    [TestClass]
    public class TokenExchangeGrantTests
    {
        private IExtensionGrantResultBuilder resultBuilder;
        private Mock<ITokenExchangeRequestValidator> requestValidatorMock;
        private Mock<ILogger<TokenExchangeResultBuilder>> loggerMock;
        private TokenExchangeValidation errorResult;
        private TokenExchangeValidation successResult;

        [TestInitialize]
        public void Setup()
        {
            this.loggerMock = new Mock<ILogger<TokenExchangeResultBuilder>>();
            this.resultBuilder = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            this.requestValidatorMock = new Mock<ITokenExchangeRequestValidator>();
            this.errorResult = CreateErrorValidation();
            this.successResult = CreateSuccessValidation();
        }

        [TestMethod]
        public async Task Build_ValidRequest_SuccessResult()
        {
            // Arrange
            var expectedSub = this.successResult.SubjectTokenValidationResult.Claims.Sub();
            var expectedClientId = this.successResult.SubjectTokenValidationResult.Client.ClientId;
            var expectedClientClaim = "customValueFromSubject";
            
            var tokenExchangeContext = new ExtensionGrantValidationContext()
            {
                Request = new ValidatedTokenRequest
                {
                    Client = new Client
                    {
                        ClientId = "dummyClientFromRequest",
                    },
                    ClientClaims = new List<Claim> { new Claim("client_custom_type", "customValueFromRequest") }
                }
            };
            this.requestValidatorMock.Setup(x => x.ValidateAsync(tokenExchangeContext.Request))
                .ReturnsAsync(this.successResult);

            var target = new TokenExchangeGrant(this.requestValidatorMock.Object, this.resultBuilder);

            // Act
            await target.ValidateAsync(tokenExchangeContext).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(tokenExchangeContext.Result.IsError);
            Assert.AreEqual(expectedSub, tokenExchangeContext.Result.Subject.Claims.Sub());
            Assert.AreEqual(expectedClientId, tokenExchangeContext.Request.Client.ClientId);
            Assert.AreEqual(expectedClientClaim, tokenExchangeContext.Request.ClientClaims.FirstOrDefault(c => c.Type == "client_custom_type").Value);
            this.requestValidatorMock.Verify(v => v.ValidateAsync(It.IsAny<ValidatedTokenRequest>()), Times.Once());
        }

        [TestMethod]
        public async Task Build_InvalidRequest_ErrorResult()
        {
            // Arrange
            var tokenExchangeContext = new ExtensionGrantValidationContext();
            this.requestValidatorMock.Setup(x => x.ValidateAsync(tokenExchangeContext.Request))
                .ReturnsAsync(this.errorResult);

            var target = new TokenExchangeGrant(this.requestValidatorMock.Object, this.resultBuilder);

            // Act
            await target.ValidateAsync(tokenExchangeContext).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(tokenExchangeContext.Result.IsError);
            Assert.AreEqual("invalid_request", tokenExchangeContext.Result.Error);
            Assert.AreEqual(this.errorResult.ErrorDescription, tokenExchangeContext.Result.ErrorDescription);
            this.requestValidatorMock.Verify(v => v.ValidateAsync(It.IsAny<ValidatedTokenRequest>()), Times.Once());
        }

        private static TokenExchangeValidation CreateSuccessValidation()
        {
            return new TokenExchangeValidation
            {
                SubjectTokenValidationResult = new TokenValidationResult
                {
                    IsError = false,
                    Client = new Client
                    {
                        ClientId = "SubjectDummyClientId",
                        Claims = new List<ClientClaim>
                        {
                            new ("client_custom_type", "customValueFromSubject")
                        }
                    },
                    Claims = new List<Claim>
                    {
                        new (JwtClaimTypes.Subject, "dummySub")
                    }
                },
                ActorTokenValidationResult = new TokenValidationResult
                {
                    IsError = false,
                    Client = new Client
                    {
                        ClientId = "ActorDummyClientId",
                    },
                    Claims = new List<Claim>()
                },
            };
        }

        private static TokenExchangeValidation CreateErrorValidation()
        {
            var errorResult = new TokenValidationResult
            {
                IsError = true,
                Error = "invalid request",
                ErrorDescription = "dummy description",
            };

            var validation = new TokenExchangeValidation
            {
                SubjectTokenValidationResult = new TokenValidationResult { IsError = true },
                ActorTokenValidationResult = new TokenValidationResult { IsError = false },
            };

            validation.SetErrors(errorResult);

            return validation;
        }
    }
}