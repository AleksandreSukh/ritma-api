using System;

namespace RestApiClient
{
    /// <summary>
    /// Parses "Username;Password" to <see cref="ApiCredentials"/> objcet with useraname and password
    /// </summary>
    public static class CredentialParser
    {
        public static ApiCredentials ParseCredentials(string credentialsString)
        {
            if (string.IsNullOrEmpty(credentialsString))
                throw new NotSupportedException();
            if (!credentialsString.Contains(";"))
                throw new NotSupportedException();
            var split = credentialsString.Split(';');
            if (split.Length != 2)
                throw new NotSupportedException();
            return new ApiCredentials(split[0], split[1]);

        }
    }
}