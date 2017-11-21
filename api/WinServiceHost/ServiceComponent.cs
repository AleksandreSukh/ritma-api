using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using CommandLine;

namespace WinServiceHost
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        // ReSharper disable once InconsistentNaming
        public virtual string ServiceName => nameof(ServiceName);
        // ReSharper disable once InconsistentNaming
        public virtual string DisplayName => nameof(DisplayName);
        public ProjectInstaller()
        {
            AfterInstall += Installer_AfterInstall;

            var process = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
            if (DisplayName == null) return;
            if (ServiceName == null) return;

            var service = new ServiceInstaller
            {
                DisplayName = DisplayName,
                ServiceName = ServiceName,
                StartType = ServiceStartMode.Automatic,
            };
            Installers.Add(process);
            Installers.Add(service);
        }

        private void Installer_AfterInstall(object sender, InstallEventArgs e)
        {
            using (var sc = new ServiceController(ServiceName))
            {
                sc.Start();
                //
                var args = $"failure {ServiceName} reset=86400 actions= restart/60000/restart/60000//";
                System.Diagnostics.Process.Start("sc.exe", args);

            }
        }
    }
    public class ServiceInstallerHelper
    {
        public static bool InstallServiceAndExit(ref bool debugMode, LaunchTypeEnum launchType)
        {
            //var argsList = args.ToList();
            //if (argsList.Count == 0) return false;
            //if (argsList.Count != 1)
            //    throw new InvalidOperationException($"ServiceInstaller may only accept a single argument; Provided arguments were: {args}");
            Console.WriteLine($"Launching: {launchType}");
            //var arg = argsList.Single().ToLower();
            switch (launchType)
            {
                case LaunchTypeEnum.Service:
                    return false;
                case LaunchTypeEnum.Install:
                    InstallService();
                    return true;
                case LaunchTypeEnum.Uninstall:
                    UninstallService();
                    return true;
                case LaunchTypeEnum.Debug:
                    debugMode = true;
                    return false;
                default:
                    throw new InvalidOperationException($"Unsupported install type: {launchType}");
            }
        }
        public static void InstallService()
        {
            try { UninstallService(); } catch { /*Ignored*/ }
            ManagedInstallerClass.InstallHelper(new[] { GetEntryAssemblyLocation() });
        }
        public static void UninstallService() => ManagedInstallerClass.InstallHelper(new[] { "/u", GetEntryAssemblyLocation() });
        public static void DecreaseProcessPriority() => Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        public static string GetEntryAssemblyLocation() => Assembly.GetEntryAssembly().Location;
    }
    public enum LaunchTypeEnum
    {
        Service,
        Install,
        Uninstall,
        Debug
    }
    public class Options
    {
        [Option('l', Required = true, HelpText = "Specify Launch type")]
        public LaunchTypeEnum LaunchType { get; set; }
        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            var ass = Assembly.GetEntryAssembly().GetName();
            usage.AppendLine($"{ass.Name} {ass.Version}");
            usage.AppendLine("Following arguments accepted:");
            var options = this.GetType().GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(OptionAttribute)))
                .Select(p => p.CustomAttributes.First(a => a.AttributeType == typeof(OptionAttribute)))
                .Select(a => GetHelpForOption(a));

            var optionsString = string.Join(Environment.NewLine, options.ToArray());
            usage.AppendLine(optionsString);
            return usage.ToString();
        }

        private static string GetHelpForOption(CustomAttributeData a)
        {
            var helpArg = a.NamedArguments?.FirstOrDefault(n => n.MemberName == nameof(OptionAttribute.HelpText));
            var help = string.Empty;
            if (helpArg.HasValue)
                help = helpArg.Value.TypedValue.ToString();




            return string.Join(", ", a.ConstructorArguments.Select(n => $"{n.Value}").ToArray()) + " " + help;
        }

        //public string GetUsage()
        //{
        //    return $"Please specify launch type as one of the following:{string.Join(", ", Enum.GetNames(typeof(LaunchTypeEnum)).ToArray())}" + Environment.NewLine;
        //}
    }


    public class ServiceBaseHelper : System.ServiceProcess.ServiceBase
    {

        public void PublicOnStart(string[] args) => OnStart(args);
        public static void Init<T>(string[] args)
        {
            Options options = new Options();
            var parseResult = Parser.Default.ParseArguments(args, options);
            if (!parseResult)
            {
                throw new NotImplementedException();
                Console.WriteLine($"Couldn't process args: {string.Join(", ", args)}");
                Environment.Exit(1);
            };
            //var options = (parseResult as Parsed<Options>)?.Value;
            if (options == null) throw new InvalidOperationException("options may not be parsed to null");
            var debugMode = false;// Debugger.IsAttached;
            if (!debugMode && ServiceInstallerHelper.InstallServiceAndExit(ref debugMode, options.LaunchType)) return;
            var service = Activator.CreateInstance(typeof(T)) as ServiceBaseHelper;
            Debug.Assert(service != null, "service != null");
            if (debugMode)
            {
                service.PublicOnStart(args);
                Console.WriteLine("Service Started...");
                Console.WriteLine("-- press any key to exit... --");
                Console.Read();
            }
            else { Run(service); }
        }
    }
}