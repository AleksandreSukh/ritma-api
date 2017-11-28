using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using RestApiBase;
using RitmaRestApi.Models;
using RitmaRestApi.Models.DataSourceModels;

namespace RitmaRestApi.DataSource
{
    public interface IWordDataRepository : IDataRepository, IDisposable
    {
        Dictionary<string, Word> GetAllWords();
        IQueryable<WordSimilarity> GetAllWordSimilarities();
        void AddNewWord(Word newWord);
        void AddSimilarities(IEnumerable<WordSimilarity> similaritiesForThatWord);
        void RemoveSimilarities(Func<WordSimilarity, bool> predicate);
        void Save();
    }
}