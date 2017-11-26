using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RitmaRestApi.Helpers;

namespace RitmaRestApi
{
    public static class HttpVerbs
    {
        public const string GET = nameof(GET);
        public const string POST = nameof(POST);
        public const string PUT = nameof(PUT);
    }

    public static class WebApiConfig
    {
        public static class RouteConfigNames
        {
            public const string GetById = nameof(GetById);
            //public const string CallAction = nameof(CallAction);
            //public const string ApplyAction = nameof(ApplyAction);
            //public const string ApplyCustomAction = nameof(ApplyCustomAction);
        }

        public const string ApiRoot = "api";

        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new PreflightRequestsHandler());
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                        name: RouteConfigNames.GetById,
                        routeTemplate: ApiRoot + "/{controller}/{id}",
                        defaults: new { id = RouteParameter.Optional }
                    );

            //Configure filters
            config.Filters.Add(new HostAuthenticationAttribute("bearer"));
            config.Filters.Add(new AuthorizeAttribute());
            config.Filters.Add(new LogExecutionAttribute());
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings =
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
            config.Formatters.Add(new XmlMediaTypeFormatter());

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            Database.SetInitializer(new Initializer());


        }
    }

    public class PreflightRequestsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("Origin") && request.Method.Method == "OPTIONS")
            {
                var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Headers", "Origin, Content-Type, Accept, Authorization");
                response.Headers.Add("Access-Control-Allow-Methods", "*");
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            var res = base.SendAsync(request, cancellationToken);
            var result = res.Result;
            return res;
        }
    }
}