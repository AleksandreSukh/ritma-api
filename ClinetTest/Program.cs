using System;
using System.Collections.Generic;
using RestApiClient;
using RitmaApiSharedModel;
using TextLoggerNet.Loggers;
using Utils;

namespace ClinetTest
{
    class Program
    {
        static void Main()
        {
            var logger = new ConsoleLoggerEasy();
            LinqPadLikeExtensions.Init(s => logger.WriteLine(s));



            var client = new ApiCrudClient<RitmaResult>(
                userName: "test",
                password: "asdfqwer1234!@#$",
                apiUrl: "http://localhost:8083/",
                subUrl: "api/reports",
                tokenSubUrl: "oauth2/token",
                logger: log =>
                {
                    //Console.ForegroundColor = ConsoleColor.Gray;
                    logger.WriteLine(log);
                    //Console.ResetColor();
                    //log.Dump();
                }
,
                pingerSampleCreator: () =>
                    new RitmaResult()
                    {
                        //Body = @"Ping",
                        //Title = "Ping",
                        Date = DateTime.Now
                    },
                pingIntervalSeconds: 20);
            while (true)
            {
                var ex = new InvalidOperationException("Moxda pizdeci", new AccessViolationException("ap! ap!"));
                var res = client.CreateObjectAsync(new RitmaResult()
                {
                    //Body = ex.ToString(),
                    Date = DateTime.Now,
                    Meta = new KeyValuePair<string, string>[]
                        {new KeyValuePair<string, string>("დამატებითი ინფო", "რაიმე")}
                });
                if (res.Result.IsFailure)
                    logger.WriteLine(res.Result.Error);
                else logger.WriteLine("Error reported successfully");
                client.PingSequence.Subscribe(ok => logger.WriteLine(ok ? "-------- Connectin OK!" : "-------- Connection Failure!"));
                //Observable.Interval(TimeSpan.FromSeconds(5))
                //        .Subscribe(l =>
                //            {
                //                client.GetUpdatedResult()
                //                .OnSuccess(() => Console.WriteLine("Connectin OK!"))
                //                    .OnFailure(er => Console.WriteLine($"Connection Failure!:{er}"));
                //            });

                //Observable.Interval(TimeSpan.FromSeconds(5))
                //    .Select(l => client.GetUpdatedResult().IsSuccess)
                //    .Scan((acc, i) => acc == i ? acc : i).DistinctUntilChanged()
                //    .Subscribe(ok => { Console.WriteLine(ok ? "Connectin OK!" : $"Connection Failure!"); });




                Console.WriteLine("Press Enter to repeat");
                Console.ReadKey();
            }

        }


    }

    public class RitmaResult : IRitmaResult
    {
        public long Id { get; set; }
        public string[] ResultWords { get; set; }
        public string RequestWord { get; set; }
        public DateTime Date { get; set; }
        public KeyValuePair<string, string>[] Meta { get; set; }
    }
}
