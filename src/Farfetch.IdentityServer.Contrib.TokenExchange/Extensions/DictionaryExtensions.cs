namespace Farfetch.IdentityServer.Contrib.TokenExchange.Extensions
{
    using System.Collections.Generic;

    using Newtonsoft.Json.Linq;

    public static class DictionaryExtensions
    {
        public static bool TryAddNonEmptyString(this Dictionary<string, object> dictionary, string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            dictionary.Add(key, value);

            return true;
        }

        public static bool TryAddNonEmptyJObject(this Dictionary<string, object> dictionary, string key, JObject value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!value.HasValues)
            {
                return false;
            }

            dictionary.Add(key, value);

            return true;
        }
    }
}
