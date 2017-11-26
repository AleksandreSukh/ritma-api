using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.AspNet.Identity;
using TextLoggerNet.Interfaces;

namespace RitmaRestApi.Helpers
{
    public class LogExecutionAttribute : ActionFilterAttribute
    {
        private ILogger _logger => DependencyRepository.Instance.Logger;
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            _logger.WriteLine($"Executing action {actionContext.ActionDescriptor.ActionName}. For user:{actionContext.RequestContext.Principal.Identity.GetUserName()} at:{DateTime.Now} ");
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            _logger.WriteLine($"Executed action {actionExecutedContext.ActionContext.ActionDescriptor.ActionName}. For user:{actionExecutedContext.ActionContext.RequestContext.Principal.Identity.GetUserName()} at:{DateTime.Now} ");
            if (actionExecutedContext.Exception != null)
            {
                _logger.WriteLine(actionExecutedContext.Exception);
                _logger.WriteLine(actionExecutedContext.ActionContext.RequestContext.ToString());
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}