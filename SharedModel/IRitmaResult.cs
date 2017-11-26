using System;
using System.Collections.Generic;
using SharedTemplate;

namespace RitmaApiSharedModel
{
    public interface IRitmaResult : IId
    {
        string[] ResultWords { get; set; }
        string RequestWord { get; set; }
        DateTime Date { get; set; }
        KeyValuePair<string, string>[] Meta { get; set; }
    }
}