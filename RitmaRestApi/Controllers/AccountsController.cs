using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using RestApiBase;
using RitmaRestApi.DataSource;
using RitmaRestApi.Helpers;
using RitmaRestApi.Models;
using SharedTemplate;

namespace RitmaRestApi.Controllers
{
    [RoutePrefix(WebApiConfig.ApiRoot + "/accounts")]
    public class AccountsController : MyControllerBase
    {
        private Func<IWordDataRepository> _repoProvider = DependencyRepository.Instance.ReportRepositoryProvider;

        [Authorize(Roles = RoleNames.Admin)]
        [AcceptVerbs(HttpVerbs.POST, HttpVerbs.PUT)]
        [Route("Register")]
        [Description("Register user using username and password")]
        public async Task<IHttpActionResult> Register([FromBody]RegistrationForm registrationForm)
        {
            if (registrationForm == null)
                return BadRequest("Registration form must be provided");
            if (
                !string.IsNullOrEmpty(registrationForm.UserName) &&
                !string.IsNullOrEmpty(registrationForm.Password) &&
                !string.IsNullOrEmpty(registrationForm.RolesCommaSeparated)
                )
                return await Register(registrationForm.UserName, registrationForm.Password, registrationForm.Email, registrationForm.RolesCommaSeparated);
            return BadRequest(
                $"{nameof(registrationForm.UserName)}, {nameof(registrationForm.Password)}, {nameof(registrationForm.RolesCommaSeparated)} are all recuired");

        }

        async Task<IHttpActionResult> Register(string userName, string password, string email, string roleName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            using (var repo = _repoProvider.Invoke())
            {
                var res = repo.CreateUser(userName, password, email, roleName);
                return res.Succeeded ? (IHttpActionResult)Ok() : new StatusCodeResult(HttpStatusCode.ServiceUnavailable, this);
            }
        }


    }
}
