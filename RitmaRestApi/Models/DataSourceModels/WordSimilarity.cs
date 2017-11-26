using System.ComponentModel.DataAnnotations;

namespace RitmaRestApi.Models.DataSourceModels
{
    public class WordSimilarity
    {
        [Key]
        public int Id { get; set; }

        public int Word_Id { get; set; }
        public int Word_Id1 { get; set; }
        public double Similarity { get; set; }
    }
}