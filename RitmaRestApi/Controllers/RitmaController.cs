using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using GeorgianLanguageClasses;
using RitmaRestApi.DataSource;
using RitmaRestApi.Helpers;
using RitmaRestApi.Models.DataSourceModels;
using SharedTemplate;

namespace RitmaRestApi.Controllers
{
    public static class RitmaRoutes
    {
        public const string Get = "{requestword}";
    }
    [RoutePrefix(WebApiConfig.ApiRoot + "/ritma")]
    public class RitmaController : ApiController
    {
        private readonly Func<IWordDataRepository> _repoProvider = DependencyRepository.Instance.ReportRepositoryProvider;
        private readonly ApiConfig _config = DependencyRepository.Instance.ApiConfig;
        [LogExecution]
        [Route(RitmaRoutes.Get)]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.GET)]
        public IHttpActionResult GetWords(string requestword)
        {
            if (!requestword.IsGeorgianWord())
                return BadRequest($"Word {requestword} is not a valid Georgian word");

            var topn = _config.EvalTopNSimiilarities;

            using (var repo = _repoProvider.Invoke())
            {
                var rep = GetTopNRitmas(requestword, repo, topn);
                if (rep == null)
                {
                    return NotFound();
                }
                return Ok(rep);
            }
        }

        private string[] GetTopNRitmas(string target, IWordDataRepository repo, int topn)
        {
            var words = repo.GetAllWords();
            var vss = new ConcurrentDictionary<string, double>();
            Parallel.ForEach(words, b => vss.GetOrAdd(b.Key, s => GeoWordMatcher.EvaluateRhymeSimilarity(target, s, true)));
            return vss.OrderByDescending(w => w.Value).Take(topn).Select(w => w.Key).ToArray();
        }

        [LogExecution]
        [HttpPost]
        [Route(RitmaRoutes.Get)]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.POST)]
        public IHttpActionResult Default(string requestword)
        { return GetWords(requestword); }
    }

}