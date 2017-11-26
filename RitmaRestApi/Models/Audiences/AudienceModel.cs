using System.ComponentModel.DataAnnotations;

namespace RitmaRestApi.Models.Audiences
{
    public class AudienceModel
    {
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
    }
}