using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using SharedTemplate;

namespace RestApiClient
{
    public class ApiAdministrationClient : ApiClientBase
    {
        public ApiAdministrationClient(string userName, string password, string apiUrl, string tokenSubUrl, Action<string> logger) : base(userName, password, apiUrl, tokenSubUrl, logger)
        {
        }

        private const string registrationSubUrl = "api/accounts/register";

        public async Task<Result> RegisterNewUserAsync(string userName, string password, string rolesCommaSeparated, string email = null)
        {
            await UpdateTokenAndScheduleNextUpdates(invoker: "UserRegistrator");
            using (var client = GetAuthorizedHttpClient())
            {
                var mediaType = client.DefaultRequestHeaders.Accept.First().MediaType;
                var econding = Encoding.UTF8;
                var encodingStr = client.DefaultRequestHeaders.AcceptEncoding.FirstOrDefault()?.Value;
                if (encodingStr != null)
                    econding = Encoding.GetEncoding(encodingStr);
                var obj = new RegistrationForm() { UserName = userName, Password = password, RolesCommaSeparated = rolesCommaSeparated, Email = email };
                StringContent content = new StringContent(JsonConvert.SerializeObject(obj), econding, mediaType);
                // HTTP POST
                Logger.Invoke($"Posting at:{registrationSubUrl}{Environment.NewLine}Content:{content}");
                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsync(registrationSubUrl, content);
                }
                catch (Exception e)
                {
                    return Result.Fail(e.Message);
                }
                if (response.IsSuccessStatusCode)
                {
                    return Result.Ok();
                }
                else
                {
                    Logger.Invoke("User registration failed");
                    var failedResult = await response.Content.ReadAsStringAsync();
                    Logger.Invoke(failedResult);
                    return Result.Fail(response.ReasonPhrase);
                }
            }
        }
        public class RegistrationForm : IRegistrationForm
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string RolesCommaSeparated { get; set; }
            public string Email { get; set; }
        }

    }
}