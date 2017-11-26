using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextLoggerNet.Loggers;

namespace WinServiceConsoleAppHybrid
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        // ReSharper disable once InconsistentNaming
        public virtual string ServiceServiceName => nameof(ServiceServiceName);
        // ReSharper disable once InconsistentNaming
        public virtual string ServiceDisplayName => nameof(ServiceDisplayName);
        public ProjectInstaller()
        {
            AfterInstall += Installer_AfterInstall;

            var process = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
            if (ServiceDisplayName == null) return;
            if (ServiceServiceName == null) return;

            var service = new ServiceInstaller
            {
                DisplayName = ServiceDisplayName,
                ServiceName = ServiceServiceName,
                StartType = ServiceStartMode.Automatic,
            };
            Installers.Add(process);
            Installers.Add(service);
        }

        private void Installer_AfterInstall(object sender, InstallEventArgs e)
        {
            using (var sc = new ServiceController(ServiceServiceName))
            {
                sc.Start();
                //
                var args = $"failure {ServiceServiceName} reset=86400 actions= restart/60000/restart/60000//";
                System.Diagnostics.Process.Start("sc.exe", args);

            }
        }
    }


    public class ServiceInstallerHelper
    {
        public static bool InstallServiceAndExit(ref bool debugMode, IEnumerable<string> args)
        {
            var argsList = args.ToList();

            if (argsList.Count == 0)
            {
                //debugMode = true;
                return false; //If no arguments are provided, execute in Service mode
            }

            if (argsList.Count != 1)
                throw new InvalidOperationException($"ServiceInstaller may only accept a single argument; Provided arguments were: {args}");

            var arg = argsList.Single().ToLower();
            switch (arg)
            {
                case "/i":
                    ServiceHelper.InstallService();
                    return true;
                case "/u":
                    ServiceHelper.UninstallService();
                    return true;
                case "/d":
                    debugMode = true;
                    return false;
                default:
                    throw new InvalidOperationException($"Unsupported argument: {arg}");
            }
        }

        public static void DecreaseProcessPriority()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        }
    }

    public class ServiceHelper
    {
        readonly Action _mainAction;
        public ServiceHelper(Action mainAction)
        {
            _mainAction = mainAction;
        }

        public void OnStart(string[] args)
        {
            //We don't want to break dependencies on files stored beside main executable so we set working directory to application executable dir
            Directory.SetCurrentDirectory(Path.GetDirectoryName(GetEntryAssemblyLocation()));

            _mainAction();
        }
        public void OnStop() { }
        public static void InstallService()
        {
            try { UninstallService(); }
            catch {/*Ignored*/}

            ManagedInstallerClass.InstallHelper(GetParams());
        }

        private static string[] GetParams() => new[]
        {
            $"/ServiceName={Path.GetFileNameWithoutExtension(GetEntryAssemblyLocation())}", GetEntryAssemblyLocation()
        };



        private static string GetEntryAssemblyLocation()
        {
            var returnValue = Assembly.GetEntryAssembly().Location;
            return returnValue;
        }

        public static void UninstallService()
        { ManagedInstallerClass.InstallHelper(new[] { "/u", GetEntryAssemblyLocation() }); }

    }
}
