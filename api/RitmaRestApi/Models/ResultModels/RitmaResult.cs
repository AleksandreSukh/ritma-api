using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RestApiBase;
using RitmaApiSharedModel;

namespace RitmaRestApi.Models
{
    public class RitmaResult : LinkedResource, IRitmaResult,IUtcDated
    {
        [Key]
        public long Id { get; set; }

        public string[] ResultWords { get; set; }
        public string RequestWord { get; set; }
        public DateTime Date { get; set; }
        public KeyValuePair<string, string>[] Meta { get; set; }

        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }

        public DateTime DateUtc { get; set; }
    }
}