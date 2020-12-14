namespace IdentityServer4.Contrib.TokenExchange.Constants
{
    public static class TokenExchangeConstants
    {
        public static class GrantTypes
        {
            public const string TokenExchange = "urn:ietf:params:oauth:grant-type:token-exchange";
        }

        public static class TokenTypes
        {
            public const string AccessToken = "urn:ietf:params:oauth:token-type:access_token";
        }

        public static class ClaimTypes
        {
            public const string Act = "act";
            public const string ClientAct = "client_act";
            public const string TenantId = "tenantId";
        }

        public static class RequestParameters
        {
            public const string SubjectToken = "subject_token";
            public const string SubjectTokenType = "subject_token_type";
            public const string ActorToken = "actor_token";
            public const string ActorTokenType = "actor_token_type";
        }

        public static class ResponseParameters
        {
            public const string IssuedTokenType = "issued_token_type";
        }
    }
}
