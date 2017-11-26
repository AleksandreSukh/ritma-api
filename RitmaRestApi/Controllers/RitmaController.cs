using System;
using System.Collections.Generic;
using System.Linq;
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

        [LogExecution]
        [Route(RitmaRoutes.Get)]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.GET)]
        public IHttpActionResult GetReport(string requestword)
        {
            using (var repo = _repoProvider.Invoke())
            {
                var rep = GetTop20Ritmas(requestword, repo);
                if (rep == null)
                {
                    return NotFound();
                }
                return Ok(rep);
            }
        }

        private string[] GetTop20Ritmas(string target, IWordDataRepository repo)
        {
            AddIf(repo, target, 20);
            var Words = repo.GetAllWords();
            var WordSimilarities = repo.GetAllWordSimilarities();
            var thisWordInDb = Words.FirstOrDefault(w => w.WordString == target);

            if (thisWordInDb != null)
            {
                var simils = WordSimilarities.Where(ws => ws.Word_Id == thisWordInDb.Id);
                var resultUnOrdered = simils.Join(Words, s => s.Word_Id1, w => w.Id, (s, w) => new { w.WordString, Simil = s.Similarity });
                var result = resultUnOrdered
                    .OrderByDescending(w => w.Simil).Select(w => w.WordString);
                return result.ToArray(); ;
            }
            return null;
        }

        [LogExecution]
        [HttpPost]
        [Route(RitmaRoutes.Get)]
        [Authorize(Roles = RoleNames.AdminOrUsers)]
        [AcceptVerbs(HttpVerbs.POST)]
        public IHttpActionResult Default(string requestword)
        { return GetReport(requestword); }


        //Core logic
        void AddIf(IWordDataRepository repo, string word, int topn)
        {
            if (!word.IsGeorgianWord())
                throw new ArgumentException($"Word {word} is not a valid Georgian word");
            var Words = repo.GetAllWords();
            if (!Words.Any(w => w.WordString == word))
            {
                var newWord = new Word() { WordString = word };
                repo.SaveNewWord(newWord);
                var a = Words.First(w => w.WordString == word);
                AddSimils(repo, a, topn);
            }
            else
            {
                var a = Words.First(w => w.WordString == word);
                if (!repo.GetAllWordSimilarities().Any(ws => ws.Word_Id == a.Id))
                    AddSimils(repo, a, topn);
            }


        }

        void AddSimils(IWordDataRepository repo, Word a, int topn)
        {
            var vss = new List<WordSimilarity>();
            foreach (var b in repo.GetAllWords().Where(w => w.Id != a.Id))
            {
                vss.Add(new WordSimilarity()
                {
                    Word_Id = a.Id,
                    Word_Id1 = b.Id,
                    Similarity = GeoWordMatcher.EvaluateRhymeSimilarity(a.WordString, b.WordString, true)
                });
            }
            var similaritiesForThatWord = vss.OrderByDescending(ws => ws.Similarity).Take(topn);
            repo.AddSimilarities(similaritiesForThatWord);
        }









    }
}