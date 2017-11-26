using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpFunctionalExtensions;
using RestApiClient;
using SharedTemplate;
using TextLoggerNet.Loggers;
using Utils;

namespace ApiAdminClientTest
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Application.Run(new RegisterUser());
           // Main2(args);
            //Console.ReadKey();

        }
        static async void Main2(string[] args)
        {
            var logger = new ConsoleLoggerEasy();
            LinqPadLikeExtensions.Init(s => logger.WriteLine(s));

            var client = new ApiAdministrationClient("admin",
                "asddqwee1233",
                apiUrl: "http://localhost:8083/",
                tokenSubUrl: "oauth2/token",
                logger: log =>
                {

                    logger.WriteLine(log);
                });
            var res = await client.RegisterNewUserAsync("Usr", "UsrPassword!@#$", RoleNames.Users);
            if (res.IsSuccess)
                Console.WriteLine("Success");
            else Console.WriteLine(res.Error);


            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
