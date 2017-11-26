using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CSharpFunctionalExtensions;
using SharedTemplate;

namespace RestApiClient
{
    public class ApiManagerEx<TData> : ApiManager<TData>, IApiManagerEx<TData> where TData : IId
    {
        private readonly Subject<TData> _eventSequence = new Subject<TData>();
        public IObservable<TData> EventSequence => _eventSequence;

        readonly object _disFilStpLock = new object();
        private bool _distinctFilterSetUp = false;

        bool DistinctFilterSetUp
        {
            get { lock (_disFilStpLock) return _distinctFilterSetUp; }
            set { lock (_disFilStpLock) _distinctFilterSetUp = value; }
        }

        public void SubmitDistinct(TData obj, Func<TData, TData, bool> areSimilar)
        {
            SetupDistinctFilter(areSimilar);
            _eventSequence.OnNext(obj);
        }

        void SetupDistinctFilter(Func<TData, TData, bool> areSimilar)
        {
            if (!DistinctFilterSetUp)
            {
                EventSequence.Scan((acc, i) => areSimilar(acc, i) ? acc : i)
                    .DistinctUntilChanged()
                    .Subscribe(SubmitObject);
                DistinctFilterSetUp = true;
            }
        }

        public ApiManagerEx(ApiContext config, Action<string> textLogger, Func<TData> pingerSampleCreator) : base(config, textLogger, pingerSampleCreator)
        {
        }
    }
    public class ApiManager<TData> : IApiManager<TData> where TData : IId
    {
        readonly ConcurrentQueue<TData> _cache = new ConcurrentQueue<TData>();
        ApiCrudClient<TData> _client;
        readonly object _lockConnected = new object();


        bool _conected;
        bool Connected
        {
            get { lock (_lockConnected) return _conected; }
            set { lock (_lockConnected) _conected = value; }
        }

        readonly Action<string> _textLogger;

        void Log(string text)
        {
            _textLogger?.Invoke($"{this.GetType().Name}:{text}");
        }


        public ApiManager(ApiContext config, Action<string> textLogger, Func<TData> pingerSampleCreator)
        {
            _textLogger = textLogger;
            var credentials =
                CredentialParser.ParseCredentials(config
                    .ApiCredentials);
            var userName = credentials.UserName;
            var password = credentials.Password;


            _client = new ApiCrudClient<TData>(
                userName: userName,
                password: password,
                apiUrl: config.ApiUrl,
                subUrl: config.ApiControllerUrl,
                tokenSubUrl: config.ApiTokenUrl,
                logger: log => { _textLogger?.Invoke(log); }// log.Dump()}
                ,
                pingerSampleCreator: pingerSampleCreator,
                pingIntervalSeconds: 20);
            _client.PingSequence.Subscribe(ok =>
            {
                Connected = ok;
                Log(ok ? "Connectin OK!" : "Connection Failure!");
            });


            Observable.Interval(TimeSpan.FromSeconds(20))
                .Subscribe(num =>
                {
                    Log($"Checking cache. Iteration:{num}");
                    if (Connected)
                    {
                        Flush();
                    }
                    else
                    {
                        Log("Connection Unavailable");
                    }
                });

        }

        public void SubmitObject(TData obj)
        {
            _cache.Enqueue(obj);

            if (Connected) Flush();
        }



        async void Flush()
        {
            var objectCountInCache = _cache.Count;
            if (objectCountInCache > 0)
                Log($"About to flush {objectCountInCache} objects");

            while (!_cache.IsEmpty)
            {
                TData obj;
                if (_cache.TryDequeue(out obj))
                    try
                    {
                        await _client.CreateObjectAsync(obj)
                            .OnSuccess(res => Log($"Object with id:{res.Id} submitted successfully!"))
                            .OnFailure(
                                er =>
                                {
                                    _cache.Enqueue(obj);
                                    Log($"FAILED submitting object with Id:{obj.Id}!");
                                }
                            );
                    }
                    catch (Exception e)
                    {
                        Log(e.ToString());
                        _cache.Enqueue(obj);
                    }
            }
            var countAfter = _cache.Count;
            var totalFlushed = objectCountInCache - countAfter;
            if (totalFlushed > 0)
            {
                Log($"Total {totalFlushed} objects flushed");
                if (countAfter > 0)
                    Log($"Total {countAfter} objects remaining");
            }
        }

    }
}