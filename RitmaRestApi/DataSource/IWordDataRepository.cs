using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using RestApiBase;
using RitmaRestApi.Models;
using RitmaRestApi.Models.DataSourceModels;

namespace RitmaRestApi.DataSource
{
    public interface IWordDataRepository :IDataRepository, IDisposable
    {
        IQueryable<Word> GetAllWords();
        IQueryable<WordSimilarity> GetAllWordSimilarities();
        void SaveNewWord(Word newWord);
        void SaveNewSimilarity(WordSimilarity neWordSimilarity);
        void AddSimilarities(IEnumerable<WordSimilarity> similaritiesForThatWord);
    }
}