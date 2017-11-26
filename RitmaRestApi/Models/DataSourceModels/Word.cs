using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmaRestApi.Models.DataSourceModels
{
    public class Word
    {
        [Key]
        public int Id { get; set; }
        public string WordString { get; set; }
    }
}
