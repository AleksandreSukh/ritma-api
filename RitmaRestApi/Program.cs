using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConfigNet;
using Microsoft.Owin.Hosting;
using RitmaRestApi.Helpers;
using WinServiceConsoleAppHybrid;

namespace RitmaRestApi
{
    [RunInstaller(true)]
    public class NewServiceInstaller : ProjectInstaller
    {
        public override string ServiceDisplayName { get; } = DependencyRepository.ServiceName;
        public override string ServiceServiceName { get; } = DependencyRepository.ServiceName;
    }



    public class Program : System.ServiceProcess.ServiceBase
    {
        static ServiceHelper _serviceHelper;


        static void Main(string[] args)
        {
            //To set low priority for resource hungry tasks
            //ServiceInstallerHelper.DecreaseProcessPriority();

            //If Debugger is attached or /d argument passed then it is debugmode
            var debugMode = Debugger.IsAttached;
            if (!debugMode && ServiceInstallerHelper.InstallServiceAndExit(ref debugMode, args)) return;

            var defaultLogger = DependencyRepository.Instance.Logger;
            var config = ConfigReader.ReadFromSettings<ApiConfig>();
            var baseAddress = config.BaseUrl;


            Action action = () =>
            {
                Task.Run(() =>
                {
                    using (WebApp.Start<Startup>(url: baseAddress))
                    {
                        defaultLogger.WriteLine("Server started....");
                        Thread.Sleep(-1);
                    }
                });

            };

            _serviceHelper = new ServiceHelper(action);

            if (debugMode)
            {
                var service = new Program();
                service.OnStart(null);
                Console.WriteLine("Service Started...");
                Console.WriteLine("<press any key to exit...>");
                Console.Read();
            }
            else
            { Run(new Program()); }
        }

        protected override void OnStart(string[] args) { _serviceHelper.OnStart(args); }
        protected override void OnStop() { _serviceHelper.OnStop(); }



    }
}
