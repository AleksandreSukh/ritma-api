using System;
using System.Diagnostics;
using System.Web.Http;
using ConfigNet;
using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Owin;
using RitmaRestApi;
using RitmaRestApi.Adapters;
using RitmaRestApi.Helpers;
using TextLoggerNet.Loggers;

[assembly: OwinStartup(typeof(Startup))]

namespace RitmaRestApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var devMode = Debugger.IsAttached;
            var apiConfig = DependencyRepository.Instance.ApiConfig;

            var contextProvider = DependencyRepository.Instance.ContextProvider;

            using (contextProvider.Invoke().EnsureInitialDataExists()) { }

            app.CreatePerOwinContext(contextProvider);
            app.CreatePerOwinContext(() => new ReportsUserManager(new ReportsUserStore(contextProvider.Invoke().DbContext)));

            ConfigureOAuth(app, devMode, apiConfig);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                UnhandledExceptionLogger.UnhandledExceptionHandler(e, false, 1, useDirectrory: "_UnhandledEx");
            };


            var config = new HttpConfiguration();
            WebApiConfig.Register(config);

            // set the default page
            //app.UseWelcomePage(@"/index.html");
            app.UseErrorPage(ErrorPageOptions.ShowAll);
            app.UseWebApi(config);
        }
    }

}