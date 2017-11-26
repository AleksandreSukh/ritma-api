using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace RitmaRestApi.Controllers
{
    public class HomeController : ApiController
    {
        // [Authorize(Roles = RoleNames.AdminOrUsers)] // Require authenticated requests.
        // [HttpPost]
        [AcceptVerbs("GET")]
        public IHttpActionResult Index()
        {
            IEnumerable<TypeInfo> Types = Assembly.GetExecutingAssembly().DefinedTypes.Where(type => type != null && type.IsPublic && type.IsClass && !type.IsAbstract && typeof(ApiController).IsAssignableFrom(type));
            var result = Types.Select(t => MyControllerBase.GetLinkedPage(t)).ToList();
            return Ok(result);
        }
    }
}
