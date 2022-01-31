namespace IdentityServer.Contrib.TokenExchange.Tests.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    using Duende.IdentityServer.Extensions;
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Validation;

    using IdentityModel;

    using IdentityServer.Contrib.TokenExchange.Builders;
    using IdentityServer.Contrib.TokenExchange.Config;
    using IdentityServer.Contrib.TokenExchange.Constants;
    using IdentityServer.Contrib.TokenExchange.Extensions;
    using IdentityServer.Contrib.TokenExchange.Tests.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    [TestClass]
    public class TokenExchangeResultBuilderTests
    {
        private const string InvalidRequestError = "invalid_request";

        private readonly Mock<ILogger<TokenExchangeResultBuilder>> loggerMock = new Mock<ILogger<TokenExchangeResultBuilder>>();

        [TestMethod]
        public void Build_ValidRequest_SuccessResult()
        {
            // Arrange
            var expectedIdp = "subjectIdp";
            var expectedAuthenticationMethod = TokenExchangeConstants.GrantTypes.TokenExchange;
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            var expectedSuccessMessage = "Successful Token Exchange Request.";

            var actorTokenValidationResult = SuccessActorTokenValidationResult();
            var subjectTokenValidationResult = SuccessSubjectTokenValidationResult();
            var subClaims = subjectTokenValidationResult.Claims.ToList();
            subClaims.Add(new Claim("idp", expectedIdp));
            subjectTokenValidationResult.Claims = subClaims;

            // Act
            var result = target
                .WithActor(actorTokenValidationResult)
                .WithSubject(subjectTokenValidationResult)
                .Build();

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(TokenExchangeConstants.TokenTypes.AccessToken, result.CustomResponse[TokenExchangeConstants.ResponseParameters.IssuedTokenType]);
            Assert.AreEqual(expectedIdp, result.Subject.GetIdentityProvider());
            Assert.AreEqual(expectedAuthenticationMethod, result.Subject.Claims.AuthenticationMethod());
            this.loggerMock.VerifyLogInfo(expectedSuccessMessage);
        }

        [TestMethod]
        public void Build_ValidRequest_SuccessResultMapsSubjectClaims()
        {
            // Arrange
            var options = new TokenExchangeOptions
            {
                ActorClaimsToInclude = new List<string> { JwtClaimTypes.Subject, TokenExchangeConstants.ClaimTypes.TenantId },
                SubjectClaimsToExclude = new List<string> { "customClaimTypeToExclude" }
            };

            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, options);

            var expectedClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, "testSubject"),
                new Claim(JwtClaimTypes.IdentityProvider, "subjectIdp"),
                new Claim("customClaim1", "value1"),
                new Claim("customClaim2", "value2"),
            };

            var actorTokenValidationResult = SuccessActorTokenValidationResult();
            var subjectTokenValidationResult = SuccessSubjectTokenValidationResult();
            
            var subjectTokenClaims = new List<Claim> { new Claim("customClaimTypeToExclude", "customClaimToExclude") };
            subjectTokenClaims.AddRange(expectedClaims);
            subjectTokenValidationResult.Claims = subjectTokenClaims;
            
            // Act
            var result = target
                .WithActor(actorTokenValidationResult)
                .WithSubject(subjectTokenValidationResult)
                .Build();

            // Assert
            Assert.IsFalse(result.IsError);
            foreach (var expectedClaim in expectedClaims)
            {
                var resultClaim = result.Subject.Claims.SingleOrDefault(c => c.Type.Equals(expectedClaim.Type));
                Assert.IsNotNull(resultClaim);
                Assert.AreEqual(expectedClaim.Value, resultClaim.Value);
            }

            Assert.IsFalse(result.Subject.Claims.Any(c => c.Type.Equals("customClaimTypeToExclude")));
        }

        [TestMethod]
        public void Build_ValidSubject_SuccessResultMapsNewActClaim()
        {
            // Arrange
            var expectedActClaim = "{\"client_id\":\"client_id_from_actor\",\"sub\":\"aClientSub\",\"tenantId\":\"2\",\"aCustomClaimToMap\":\"aCustomClaimValueToMap\"}";

            var options = new TokenExchangeOptions
            {
                ActorClaimsToInclude = new List<string> { JwtClaimTypes.Subject, TokenExchangeConstants.ClaimTypes.TenantId, "aCustomClaimToMap" },
                SubjectClaimsToExclude = new List<string>()
            };

            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, options);

            var actorTokenValidationResult = SuccessActorTokenValidationResult();
            var actorTokenClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, "aClientSub"),
                new Claim(TokenExchangeConstants.ClaimTypes.TenantId, "2"),
                new Claim("aCustomClaimToMap", "aCustomClaimValueToMap"),
                new Claim("aCustomClaimToExclude", "aCustomClaimValueToExclude"),
            };
            actorTokenValidationResult.Claims = actorTokenClaims;

            var subjectTokenValidationResult = SuccessSubjectTokenValidationResult();

            // Act
            var result = target
                .WithActor(actorTokenValidationResult)
                .WithSubject(subjectTokenValidationResult)
                .Build();

            // Assert
            Assert.IsFalse(result.IsError);
            var actClaim = result.Subject.Claims.SingleOrDefault(c => c.Type.Equals(TokenExchangeConstants.ClaimTypes.Act));
            Assert.IsNotNull(actClaim);
            Assert.AreEqual(expectedActClaim, actClaim.Value);
        }

        [TestMethod]
        public void Build_ValidSubject_SuccessResultMapsExistingActClaim()
        {
            // Arrange
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            var expectedActClaim = "{\"client_id\":\"client_id_from_actor\",\"sub\":\"subActor\",\"tenantId\":\"1\",\"act\":{\"client_id\":\"api1\"}}";

            var actorTokenValidationResult = SuccessActorTokenValidationResult();
            var subjectTokenValidationResult = SuccessSubjectTokenValidationResult();
            var subjectClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, "testSubject"),
                new Claim(TokenExchangeConstants.ClaimTypes.Act, "{\"client_id\":\"api1\"}")
            };
            subjectTokenValidationResult.Claims = subjectClaims;

            // Act
            var result = target
                .WithActor(actorTokenValidationResult)
                .WithSubject(subjectTokenValidationResult)
                .Build();

            // Assert
            Assert.IsFalse(result.IsError);
            var actClaim = result.Subject.Claims.SingleOrDefault(c => c.Type.Equals(TokenExchangeConstants.ClaimTypes.Act));
            Assert.IsNotNull(actClaim);
            Assert.AreEqual(expectedActClaim, actClaim.Value);
        }

        [TestMethod]
        public void Build_NoSubject_SuccessResultMapsNewActClaimToClient()
        {
            // Arrange
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            var expectedActClaim = "{\"client_id\":\"client_id_from_actor\",\"tenantId\":\"1\"}";

            var actorTokenValidationResult = new TokenValidationResult
            {
                Client = new Client
                {
                    ClientId = "client_id_from_actor"
                },
                Claims = new List<Claim> { new Claim("tenantId", "1") },
            };

            var subjectTokenValidationResult = new TokenValidationResult
            {
                Client = new Client
                {
                    ClientId = "client_id_from_subject"
                },
                Claims = new List<Claim>
                {
                    new Claim("tenantId", "2"),
                }
            };

            // Act
            var result = target
                .WithActor(actorTokenValidationResult)
                .WithSubject(subjectTokenValidationResult)
                .Build();

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Subject);
            Assert.AreEqual(subjectTokenValidationResult.Client.ClientId, result.Client.ClientId);
            var actClaim = result.Client.Claims.SingleOrDefault(c => c.Type.Equals(TokenExchangeConstants.ClaimTypes.Act));
            Assert.IsNotNull(actClaim);
            Assert.AreEqual(expectedActClaim, actClaim.Value);
        }

        [TestMethod]
        public void Build_NoSubject_SuccessResultMapsExistingActClaimToClient()
        {
            // Arrange
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            var expectedActClaim = "{\"client_id\":\"client_id_from_actor\",\"tenantId\":\"1\",\"client_act\":{\"client_id\":\"api2\",\"client_act\":{\"client_id\":\"api1\"}}}";

            var actorTokenValidationResult = new TokenValidationResult
            {
                Client = new Client
                {
                    ClientId = "client_id_from_actor",
                },
                Claims = new List<Claim> { new Claim("tenantId", "1") },
            };

            var subjectTokenValidationResult = new TokenValidationResult
            {
                Client = new Client
                {
                    ClientId = "client_id_from_subject"
                },
                Claims = new List<Claim>
                {
                    new Claim("tenantId", "2"),
                    new Claim(TokenExchangeConstants.ClaimTypes.ClientAct, "{\"client_id\":\"api2\",\"client_act\":{\"client_id\":\"api1\"}}"),
                }
            };

            // Act
            var result = target
                .WithActor(actorTokenValidationResult)
                .WithSubject(subjectTokenValidationResult)
                .Build();

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.IsNull(result.Subject);
            Assert.AreEqual(subjectTokenValidationResult.Client.ClientId, result.Client.ClientId);
            var actClaim = result.Client.Claims.SingleOrDefault(c => c.Type.Equals(TokenExchangeConstants.ClaimTypes.Act));
            Assert.IsNotNull(actClaim);
            Assert.AreEqual(expectedActClaim, actClaim.Value);
        }

        [TestMethod]
        public void Build_ValidRequest_SuccessResultMapsClientFromSubject()
        {
            // Arrange
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            var actorTokenValidationResult = SuccessActorTokenValidationResult();
            var subjectTokenValidationResult = SuccessSubjectTokenValidationResult();

            // Act
            var result = target
                .WithActor(actorTokenValidationResult)
                .WithSubject(subjectTokenValidationResult)
                .Build();

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(subjectTokenValidationResult.Client.ClientId, result.Client.ClientId);
            foreach (var expectedClaim in subjectTokenValidationResult.Client.Claims)
            {
                var resultClaim = result.Client.Claims.SingleOrDefault(c => c.Type.Equals(expectedClaim.Type));
                Assert.IsNotNull(resultClaim);
                Assert.AreEqual(expectedClaim.Value, resultClaim.Value);
            }
        }

        [TestMethod]
        public void Build_InvalidSubject_ThrowsException()
        {
            // Arrange
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());

            var subjectTokenValidationResult = new TokenValidationResult
            {
            };

            // Act
            var result = Assert.ThrowsException<InvalidOperationException>(() => target
                .WithSubject(subjectTokenValidationResult)
                .Build());

            // Assert
            Assert.AreEqual("Subject proprieties are missing", result.Message);
        }

        [TestMethod]
        public void Build_InvalidWithErrorDescription_ErrorResult()
        {
            // Arrange
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            var expectedErrorDescription = "Invalid test request";

            // Act
            var result = target
                .WithError(TokenRequestErrors.InvalidRequest, expectedErrorDescription)
                .Build();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(InvalidRequestError, result.Error);
            Assert.AreEqual(expectedErrorDescription, result.ErrorDescription);
            this.loggerMock.VerifyLogError(expectedErrorDescription);
        }

        [TestMethod]
        public void Build_ErrorDescription_ErrorResult()
        {
            // Arrange
            var target = new TokenExchangeResultBuilder(this.loggerMock.Object, new TokenExchangeOptions());
            var expectedErrorDescription = "Invalid test request no subject";

            // Act
            var result = target
                .WithError(TokenRequestErrors.InvalidRequest, expectedErrorDescription)
                .Build();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(InvalidRequestError, result.Error);
            Assert.AreEqual(expectedErrorDescription, result.ErrorDescription);
            this.loggerMock.VerifyLogError(expectedErrorDescription);
        }

        private static TokenValidationResult SuccessSubjectTokenValidationResult()
        {
            return new TokenValidationResult
            {
                Client = new Client
                {
                    ClientId = "client_id_from_subject",
                    Claims = new List<ClientClaim> { new("a", "a1"), new("b", "b1") }
                },
                Claims = new List<Claim> { new Claim(JwtClaimTypes.Subject, "subSubject"), new Claim("tenantId", "2") }
            };
        }

        private static TokenValidationResult SuccessActorTokenValidationResult()
        {
            return new TokenValidationResult
            {
                Client = new Client
                {
                    ClientId = "client_id_from_actor"
                },
                Claims = new List<Claim> { new Claim(JwtClaimTypes.Subject, "subActor"), new Claim("tenantId", "1") }
            };
        }
    }
}
