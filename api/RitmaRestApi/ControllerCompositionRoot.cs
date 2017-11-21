//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Http;
//using System.Web.Http.Controllers;
//using System.Web.Http.Dependencies;
//using System.Web.Http.Dispatcher;
//using System.Web.Routing;
//using ErrorReportingRestApi.Controllers;
//using ErrorReportingRestApi.Helpers;

//namespace ErrorReportingRestApi
//{
//    public class MyHttpControllerTypeResolver : IHttpControllerTypeResolver
//    {
//        IHttpController CreateController(HttpControllerContext controllerContext,
//            string controllerName);
//        void ReleaseController(IHttpController controller);
//        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
//        {
//            throw new NotImplementedException();
//        }
//    }
//    public class ErroApiResolver : IDependencyResolver
//    {
//        private IDependencyResolver _defaultResolver;

//        public ErroApiResolver(IDependencyResolver defaultResolver)
//        {
//            _defaultResolver = defaultResolver;
//        }

//        public object GetService(Type serviceType)
//        {
//            if (serviceType == typeof(HomeController))
//                return new HomeController();
//            if (serviceType == typeof(ReportsController))
//                return new ReportsController(DependencyRepository.Instance.ReportRepositoryProvider);
//            if (serviceType == typeof(AccountsController))
//                return new AccountsController(DependencyRepository.Instance.ReportRepositoryProvider);
//            return _defaultResolver.GetService(serviceType);
//        }

//        public IEnumerable<object> GetServices(Type serviceType)
//        {
//            return _defaultResolver.GetServices(serviceType);
//        }

//        public IDependencyScope BeginScope()
//        {
//            return _defaultResolver.BeginScope();
//        }

//        public void Dispose()
//        {
//            _defaultResolver.Dispose();
//        }
//    }
//}
