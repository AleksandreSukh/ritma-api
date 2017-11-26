using System;
using System.Collections.Generic;
using System.Linq;

namespace RestApiClient
{
    public static class TimeSpanVerbose
    {
        public static string ToVerboseStringHMS(this TimeSpan timeSpan)
        {
            var timeParts = new Dictionary<TimePartEnum, uint>();
            timeParts.Add(TimePartEnum.Days, (uint)timeSpan.Days);
            timeParts.Add(TimePartEnum.Hours, (uint)timeSpan.Hours);
            timeParts.Add(TimePartEnum.Minutes, (uint)timeSpan.Minutes);
            timeParts.Add(TimePartEnum.Seconds, (uint)timeSpan.Seconds);

            var nonZeroTimeParts = timeParts.Where(t => t.Value > 0).ToArray();

            var formattedString = string.Empty;
            for (int i = 0; i < nonZeroTimeParts.Length; i++)
            {
                var timeElement = nonZeroTimeParts[i];

                if (i != 0)
                    formattedString += " ";

                formattedString += FormatTimePart(timeElement);
                if (i == nonZeroTimeParts.Length - 2)
                    formattedString += " And";
            }
            //Return 0 Seconds if no time
            if (formattedString == string.Empty)
                formattedString += FormatTimePart(new KeyValuePair<TimePartEnum, uint>(TimePartEnum.Seconds, 0));

            return formattedString;
        }

        private static string FormatTimePart(KeyValuePair<TimePartEnum, uint> timeElement)
        {
            return $"{timeElement.Value} {timeElement.Key}";
        }

        enum TimePartEnum
        {
            Days,
            Hours,
            Minutes,
            Seconds
        }

    }
}