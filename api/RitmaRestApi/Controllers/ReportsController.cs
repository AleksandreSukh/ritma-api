using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using RitmaRestApi.DataSource;
using RitmaRestApi.Helpers;
using RitmaRestApi.Models;
using SharedTemplate;

namespace RitmaRestApi.Controllers
{
    public static class ReportRoutes
    {
        public const string Save = nameof(Save);
        public const string Delete = "{id}/delete";
        public const string GetById = "{id}";
        public const string ApplyCustomAction = "{id}/{actionName}";
    }
    [RoutePrefix(WebApiConfig.ApiRoot + "/reports")]
    public class ReportsController : ApiController
    {
        private readonly Func<IReportRepository> _repoProvider = DependencyRepository.Instance.ReportRepositoryProvider;

        [LogExecution]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.GET)]
        public IHttpActionResult GetAllReports()
        {
            using (var repo = _repoProvider.Invoke())
            {
                if (!User.IsInRole(RoleNames.Admin)) //In case user is not admin (Only admin sees all records
                    return Ok(repo.GetAll()
                        .ToList() //To apply user filter after querying database
                        .Where(rep => rep.User != null && rep.User.UserName != null &&
                                      rep.User.UserName.Equals(User.Identity.GetUserName())));
                return Ok(repo.GetAll());
            }
        }
        [LogExecution]
        [Authorize(Roles = RoleNames.AdminOrUsers)] // Require authenticated requests.
        [Route(ReportRoutes.Save)]
        public IHttpActionResult Save([FromBody]RitmaResult ritmaResult)
        {
            if (ritmaResult == null)
            {
                return BadRequest();
            }
            using (var repo = DependencyRepository.Instance.ReportRepositoryProvider.Invoke())
            {
                var userInDb = repo.GetUser(User.Identity.Name);
                ritmaResult.User = userInDb;
                repo.AddReport(ritmaResult);
                return Ok(ritmaResult);
            }
        }
        [LogExecution]
        [HttpPost]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.POST)]
        [Route(ReportRoutes.Delete)]
        public IHttpActionResult Delete(int id)
        {
            using (var repo = _repoProvider.Invoke())
            {
                var rep = GetReportOfThisUser(id, repo);
                if (rep == null)
                {
                    return NotFound();
                }
                if (repo.DeleteReport(rep))
                    return Ok(rep);
                return new StatusCodeResult(HttpStatusCode.InternalServerError, this);
            }
        }
        [LogExecution]
        [HttpPost]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.POST)]
        [Route(ReportRoutes.ApplyCustomAction)]
        public IHttpActionResult CustomActions(int id, string actionName)
        {
            var action = actionName.ToLowerInvariant();
            if (action == "delete")
                return Delete(id);
            else if (action == "details")
            {
                return Ok("სგ სგ");
            }
            return NotFound();
        }
        [LogExecution]
        [Route(ReportRoutes.GetById)]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.GET)]
        public IHttpActionResult GetReport(long id)
        {
            using (var repo = _repoProvider.Invoke())
            {
                var rep = GetReportOfThisUser(id, repo);
                if (rep == null)
                {
                    return NotFound();
                }
                return Ok(rep);
            }
        }
        [LogExecution]
        [HttpPost]
        [Route(ReportRoutes.GetById)]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.POST)]
        public IHttpActionResult Default(int id)
        { return GetReport(id); }


        private RitmaResult GetReportOfThisUser(long id, IReportRepository repo)
        {
            var rep = repo.GetAllAQueryable()
                .Where(p => p.Id == id)
                .ToList() //because user filter cannot be applied to database query
                .FirstOrDefault(p => User.IsInRole(RoleNames.Admin) ||
                                     p.User.UserName.Equals(User.Identity.GetUserName()));
            return rep;
        }
    }
}