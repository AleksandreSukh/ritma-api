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


        public IQueryable<WordSimilarity> GetAllWordSimilarities()
        {
            return _context.WordSimilarities.AsQueryable();
        }

        public void SaveNewWord(Word newWord)
        {
            //newWord.DateUtc = DateTime.UtcNow;
            _context.Words.Add(newWord);
            _context.SaveChanges();
        }
        public void SaveNewSimilarity(WordSimilarity newSimilarity)
        {
            //newWord.DateUtc = DateTime.UtcNow;
            _context.WordSimilarities.Add(newSimilarity);
            _context.SaveChanges();
        }

        public void AddSimilarities(IEnumerable<WordSimilarity> similaritiesForThatWord)
        {
            _context.WordSimilarities.AddRange(similaritiesForThatWord);
            _context.SaveChanges();
        }


        public ApplicationUser GetUser(string userName)
        {
            return _context.UsersQueriable.FirstOrDefault(
            u => u.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
        }


        public IdentityResult CreateUser(string userName, string password, string email, string roleName)
        {
            var res = _context.DbContext.CreateUser(
                userName: userName,
                password: password,
                roleName: roleName,
                email: email);
            if (res.Succeeded)
            {
                _context.SaveChanges();
                return IdentityResult.Success;
            }
            return res;
        }

        public IQueryable<Word> GetAllWords()
        {
            return _context.Words.AsQueryable();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

}