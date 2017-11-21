using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using SharedTemplate;

namespace RestApiClient
{
    public class ApiClientBase//<TData> where TData : IId
    {
        protected readonly Action<string> Logger;
        readonly string _userName;
        readonly string _password;
        readonly string _apiUrl;
        private readonly string _tokenSubUrl;

        public ApiClientBase(string userName, string password, string apiUrl, string tokenSubUrl, Action<string> logger)
        {
            _userName = userName;
            _password = password;
            _apiUrl = apiUrl;
            Logger = logger;
            _tokenSubUrl = tokenSubUrl;
        }
        private readonly object _tokenThreadSafeLock = new object();
        protected string LastToken
        {
            get { lock (_tokenThreadSafeLock) return _lastToken; }
            set { lock (_tokenThreadSafeLock) _lastToken = value; }
        }
        string _lastToken = string.Empty;

        //TODO:This method accumulates many requests if there is an authentication error and it should be fixed
        readonly object _tokenUpdaterLock = new object();

        protected async Task UpdateTokenAndScheduleNextUpdates(string invoker = null)
        {
            await UpdateTokenAndScheduleNextUpdates(token => { LastToken = token.access_token; }, invoker);
        }
        protected async Task UpdateTokenAndScheduleNextUpdates(Action<AuthToken> tokenOutput, string invoker = null)
        {
            Logger.Invoke(invoker + "Updating atuhorization token");
            var res = await FetchOauthTokenForUser(_userName, _password, _tokenSubUrl);

            lock (_tokenUpdaterLock)
            {
                Tuple<double, string> tryAgainAfterSeconds;
                if (res.IsSuccess)
                {
                    var t = res.Value;
                    tokenOutput.Invoke(t);

                    var expiresAfterSeconds = t.expires_in;

                    if (expiresAfterSeconds < 0)
                        throw new Exception($"Oups! didn't expect that server would return negative token lifespan:{expiresAfterSeconds}!");

                    var expiresAt = DateTime.Now.AddSeconds(expiresAfterSeconds);
                    Logger.Invoke($"Got authorization token:{Environment.NewLine}{t.access_token}{Environment.NewLine}Which expires in:{(expiresAt - DateTime.Now).ToVerboseStringHMS()}");

                    tryAgainAfterSeconds = new Tuple<double, string>(
                        item1: Math.Max(5, (expiresAt - DateTime.Now).TotalSeconds - 5),
                        item2: "Scheduler:");
                }
                else
                {
                    var er = res.Error;
                    Logger.Invoke(invoker + $"Authorization failed:{er}{Environment.NewLine}");
                    tryAgainAfterSeconds = new Tuple<double, string>(10, "One More Try:");
                }
                var runAfterTime = TimeSpan.FromSeconds(tryAgainAfterSeconds.Item1);
                Logger.Invoke("Next token update will be launched in:" + runAfterTime.ToVerboseStringHMS());
                var timer = new System.Timers.Timer(runAfterTime.TotalMilliseconds)
                { AutoReset = false };
                timer.Elapsed += (sender, e) => UpdateTokenAndScheduleNextUpdates(tokenOutput, tryAgainAfterSeconds.Item2);
                timer.Enabled = true;
            }
        }

        async Task<Result<AuthToken>> FetchOauthTokenForUser(string userName, string password, string tokenSubUrl)
        {
            try
            {
                using (var client = InitializeClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("username",userName),
                    new KeyValuePair<string, string>("password",password),
                    new KeyValuePair<string, string>("grant_type","password"),
                });
                    //content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var postResult = client.PostAsync(tokenSubUrl, content);
                    if (postResult.Result.IsSuccessStatusCode)
                    {
                        var cnt = await postResult.Result.Content.ReadAsStringAsync();
                        var token = (AuthToken)JsonConvert.DeserializeObject(cnt, typeof(AuthToken));
                        if (token == null) return Result.Fail<AuthToken>($"Couldn't read result as auth token:{cnt}");
                        return Result.Ok(token);
                    }
                    return Result.Fail<AuthToken>($"Error taking authentication token:{postResult.Result.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                return Result.Fail<AuthToken>(e.Message);
            }
        }

        private HttpClient InitializeClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            return client;
        }


        protected HttpClient GetAuthorizedHttpClient()
        {
            var client = InitializeClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", LastToken);
            return client;
        }
    }
}