using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using SharedTemplate;

namespace RestApiClient
{
    public class ApiCrudClient<TData> : ApiClientBase where TData : IId
    {
        private readonly Subject<bool> _pingSequence = new Subject<bool>();

        /// <summary>
        /// Here is usage sample
        /// client.PingSequence.Subscribe(ok => Console.WriteLine(ok ? "Connectin OK!" : $"Connection Failure!"));
        /// </summary>
        public IObservable<bool> PingSequence => _pingSequence;

        public Result GetNewPingResult()
        {
            return PingController().Result;
        }

        void SetUpConnectionStatusUpdater(int pingIntervalSeconds)
        {
            Observable.Interval(TimeSpan.FromSeconds(pingIntervalSeconds))
                .Select(l => GetNewPingResult().IsSuccess)
                .Scan((acc, i) => acc == i ? acc : i).DistinctUntilChanged()
                .Subscribe(ok => { _pingSequence.OnNext(ok); });
        }

        private readonly string _subUrl;
        private readonly Func<TData> _pingerSampleCreator;

        public ApiCrudClient(string userName, string password, string apiUrl, string subUrl, string tokenSubUrl, Action<string> logger, Func<TData> pingerSampleCreator, int pingIntervalSeconds) : base(userName, password, apiUrl, tokenSubUrl, logger)
        {
            _subUrl = subUrl;
            _pingerSampleCreator = pingerSampleCreator;
            SetUpConnectionStatusUpdater(pingIntervalSeconds);
        }

        public async Task<Result<TData>> CreateObjectAsync(TData rep)
        {
            Logger.Invoke("Creating dataPacket");
            var res = await SaveObject(rep);
            if (res.IsFailure)
            {
                if (res.Error.ToLowerInvariant() == "unauthorized")
                {
                    Logger.Invoke("Authorization error occured. Authorizing with current username and password");

                    await UpdateTokenAndScheduleNextUpdates();
                    return await CreateObjectAsync(rep);
                }
                return Result.Fail<TData>($"Error:\"{res.Error}\" Couldn't be resolved.");
            }
            return Result.Ok(res.Value);
        }
        public async Task<Result<TData>> DeleteObjectAsync(long reportId)
        {
            Logger.Invoke($"Deleting dataPacket with Id:{reportId}");
            var res = await DeleteObject(reportId);
            if (res.IsFailure)
            {
                if (res.Error.ToLowerInvariant() == "unauthorized")
                {
                    Logger.Invoke("Authorization error occured. Authorizing with current username and password");

                    await UpdateTokenAndScheduleNextUpdates();
                    return await DeleteObjectAsync(reportId);
                }
                return Result.Fail<TData>($"Error:\"{res.Error}\" Couldn't be resolved.");

            }
            return Result.Ok(res.Value);
        }

        public async Task<Result<IEnumerable<TData>>> GetAllObjects()
        {
            using (var client = GetAuthorizedHttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(_subUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var reports = JsonConvert.DeserializeObject<TData[]>(data);
                    if (reports != null)
                        return Result.Ok(reports.AsEnumerable());
                    return Result.Fail<IEnumerable<TData>>($"Response data:\"{data}\" couldn't be parsed as {typeof(TData[]).Name}");
                }
                return Result.Fail<IEnumerable<TData>>(response.ReasonPhrase);
            }
        }

        async Task<Result<TData>> SaveObject(TData obj)
        {
            var postUrl = $"{_subUrl}/save";

            using (var client = GetAuthorizedHttpClient())
            {
                var mediaType = client.DefaultRequestHeaders.Accept.First().MediaType;
                var econding = Encoding.UTF8;
                var encodingStr = client.DefaultRequestHeaders.AcceptEncoding.FirstOrDefault()?.Value;
                if (encodingStr != null)
                    econding = Encoding.GetEncoding(encodingStr);
                StringContent content = new StringContent(JsonConvert.SerializeObject(obj), econding, mediaType);
                // HTTP POST
                Logger.Invoke($"Posting at:{postUrl}{Environment.NewLine}Content:{content}");
                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsync(postUrl, content, new CancellationTokenSource(1000).Token);
                }
                catch (Exception e)
                {
                    return Result.Fail<TData>(e.Message);
                }
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    obj = JsonConvert.DeserializeObject<TData>(data);
                }
                else
                {
                    Logger.Invoke("Post failed");
                    var failedResult = await response.Content.ReadAsStringAsync();
                    Logger.Invoke(failedResult);
                    return Result.Fail<TData>(response.ReasonPhrase);
                }
            }
            return Result.Ok(obj);

        }

        async Task<Result<TData>> DeleteObject(long reportId)
        {
            var postUrl = $"{_subUrl}/{reportId}/Delete";

            using (var client = GetAuthorizedHttpClient())
            {
                Logger.Invoke($"Posting at:{postUrl}{Environment.NewLine}");
                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsync(postUrl, null);
                }
                catch (Exception e)
                {
                    return Result.Fail<TData>(e.Message);
                }
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    return Result.Ok(JsonConvert.DeserializeObject<TData>(data));
                }
                Logger.Invoke("Post failed");
                var failedResult = await response.Content.ReadAsStringAsync();
                Logger.Invoke(failedResult);
                return Result.Fail<TData>(response.ReasonPhrase);
            }
        }


        public async Task<Result<TData>> PingControllerAsync()
        {
            return await CreateObjectAsync(_pingerSampleCreator.Invoke()).OnSuccess(rep => DeleteObjectAsync(rep.Id));
        }
        public async Task<Result<TData>> PingController()
        {
            return await PingControllerAsync();
        }
    }
}