using System;

namespace RestApiClient
{
    /// <summary>
    /// NOTE! Sealed for a reason: (To make it valueType-like immutable object)
    /// </summary>
    public sealed class ApiContext
    {
        public ApiContext(string apiCredentials, string apiUrl, string apiControllerUrl, string apiTokenUrl)
        {
            ApiCredentials = apiCredentials;
            ApiUrl = apiUrl;
            ApiControllerUrl = apiControllerUrl;
            ApiTokenUrl = apiTokenUrl;
        }

        public readonly string ApiCredentials;
        public readonly string ApiUrl;
        public readonly string ApiControllerUrl;
        public readonly string ApiTokenUrl;
    }
}