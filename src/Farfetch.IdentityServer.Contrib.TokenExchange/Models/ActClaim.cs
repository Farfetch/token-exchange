namespace IdentityServer4.Contrib.TokenExchange.Models
{
    using Newtonsoft.Json;

    public class ActClaim
    {
        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }
    }
}