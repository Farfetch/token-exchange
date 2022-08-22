namespace Farfetch.IdentityServer.Contrib.TokenExchange.Tests.Builder
{
    
    public static class TestConstants
    {
        // Act Claim
        public const string ExpectedActClaim =
            "{\"sub\":\"subActor\",\"tenantId\":\"1\",\"client_id\":\"client_id_from_actor\",\"act\":{\"client_id\":\"api1\",\"act\":{\"client_id\":\"api2\"}}}";

        public const string SubjectActClaim =
            "{\"client_id\":\"api1\",\"act\":{\"client_id\":\"api2\"}}";

        public const string SubjectActClaimWithSameClientIdAtLast =
            "{\"sub\":\"subActor\",\"tenantId\":\"1\",\"client_id\":\"client_id_from_actor\",\"act\":{\"client_id\":\"api1\",\"act\":{\"client_id\":\"api2\"}}}";

        // Client Act Claim
        public const string ExpectedClientActClaim =
            "{\"tenantId\":\"1\",\"client_id\":\"client_id_from_actor\",\"client_act\":{\"client_id\":\"api1\",\"client_act\":{\"client_id\":\"api2\"}}}";

        public const string SubjectClientActClaim =
            "{\"client_id\":\"api1\",\"client_act\":{\"client_id\":\"api2\"}}";

        public const string SubjectClientActClaimWithSameClientIdAtLast =
            "{\"tenantId\":\"1\",\"client_id\":\"client_id_from_actor\",\"client_act\":{\"client_id\":\"api1\",\"client_act\":{\"client_id\":\"api2\"}}}";

    }
}
