using System;
using System.Collections.Generic;
using ConfigNet;
using RestApiClient;
using RitmaApiSharedModel;
using TextLoggerNet.Loggers;
using Utils;

namespace ApiManagerExTestSample
{

    class Program
    {
        static void Main()
        {
            var logger = new ConsoleLoggerEasy();
            LinqPadLikeExtensions.Init(s => logger.WriteLine(s));

            var config = ConfigReader.ReadFromSettings<ApiContext>();


            var apiManager = new ApiManagerEx<RitmaResult>(config, s => logger.WriteLine(s), () =>
                 new RitmaResult()
                 {
                    
                     Date = DateTime.Now
                 });


            var rand = new Random();
            while (true)
            {
                Console.WriteLine("Enter body of report");
                var reportText = rand.Next(1000, 9999).ToString();
                //apiManager.SubmitDistinct(new RitmaResult() { Title = "beef", Body = reportText }, (res, report1) => res.Title == report1.Title);
                Console.WriteLine("RitmaResult submitted. Press Enter to repeat");
                System.Threading.Thread.Sleep(2000);
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
