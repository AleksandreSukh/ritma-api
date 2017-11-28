namespace RitmaRestApi
{
    public sealed class ApiConfig
    {
        public readonly string AdminPassword;
        public readonly string BaseUrl;
        public readonly string Issuer;
        public readonly string Secret;
        public readonly string TokenEndpointPath;
        public readonly bool AllowInsecureHttp;
        public readonly int AccessTokenExpireTimeMinutes;
        public readonly int EvalTopNSimiilarities;

        public ApiConfig(string baseUrl, string issuer, string secret, string tokenEndpointPath, bool allowInsecureHttp, int accessTokenExpireTimeMinutes, string adminPassword, int evalTopNSimiilarities)
        {
            BaseUrl = baseUrl;
            Issuer = issuer;
            Secret = secret;
            TokenEndpointPath = tokenEndpointPath;
            AllowInsecureHttp = allowInsecureHttp;
            AccessTokenExpireTimeMinutes = accessTokenExpireTimeMinutes;
            AdminPassword = adminPassword;
            EvalTopNSimiilarities = evalTopNSimiilarities;
        }

    }
}