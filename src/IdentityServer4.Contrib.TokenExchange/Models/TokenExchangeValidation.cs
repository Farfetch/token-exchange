namespace IdentityServer4.Contrib.TokenExchange.Models
{
    using IdentityServer4.Models;
    using IdentityServer4.Validation;

    public class TokenExchangeValidation
    {
        private const string GenericErrorDescription = "Token validation failed.";

        public TokenValidationResult SubjectTokenValidationResult { get; set; }

        public TokenValidationResult ActorTokenValidationResult { get; set; }

        public bool IsError { get; private set; }

        public TokenRequestErrors Error { get; private set; }

        public string ErrorDescription { get; private set; }

        public void SetErrors(TokenValidationResult tokenValidation)
        {
            this.Error = TokenRequestErrors.InvalidRequest;
            this.IsError = true;
            this.ErrorDescription = $"{tokenValidation.Error} : { tokenValidation.ErrorDescription ?? GenericErrorDescription }";
        }

        public void SetErrors(string errorDescription)
        {
            this.Error = TokenRequestErrors.InvalidRequest;
            this.IsError = true;
            this.ErrorDescription = errorDescription;
        }
    }
}
