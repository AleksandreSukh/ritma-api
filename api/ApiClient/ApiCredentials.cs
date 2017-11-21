namespace RestApiClient
{
    public struct ApiCredentials
    {
        public ApiCredentials(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public string UserName { get; }
        public string Password { get; }
    }
}