using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using RestApiBase;
using RitmaRestApi.Models;
using RitmaRestApi.Models.DataSourceModels;

namespace RitmaRestApi.DataSource
{
    public class WordDataRepository : IWordDataRepository
    {
        private readonly ISourceDbContext _context;

        public WordDataRepository(Func<ISourceDbContext> contextProvider)
        {
            this._context = contextProvider.Invoke();
        }
        public WordDataRepository(ISourceDbContext context)
        {
            this._context = context;
        }

        #region Queries
        public IQueryable<WordSimilarity> GetAllWordSimilarities() => _context.WordSimilarities.AsQueryable();

        public ApplicationUser GetUser(string userName) => _context.UsersQueriable.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));

        public Dictionary<string, Word> GetAllWords()
        {
            if (cachedWords == null)
                cachedWords = _context.Words.ToDictionary(w => w.WordString, w => w);
            return cachedWords;
        }

        private static Dictionary<string, Word> cachedWords = null;

        #endregion

        #region Commands
        public void AddSimilarities(IEnumerable<WordSimilarity> similaritiesForThatWord) => _context.WordSimilarities.AddRange(similaritiesForThatWord);

        public void RemoveSimilarities(Func<WordSimilarity, bool> predicate) => _context.WordSimilarities.RemoveRange(_context.WordSimilarities.Where(predicate));

        public void Save() => _context.SaveChanges();

        public void AddNewWord(Word newWord) => _context.Words.Add(newWord);

        public IdentityResult CreateUser(string userName, string password, string email, string roleName) => _context.DbContext.CreateUser(
            userName: userName,
            password: password,
            roleName: roleName,
            email: email);

        public void Dispose() => _context?.Dispose();

        #endregion
    }

}