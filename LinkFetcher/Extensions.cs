using System;
using System.Collections.Generic;

namespace LinkFetcher
{
    public static class Extensions
    {
        public static string ToHumandReadable(this long number)
        {
            double doubleValue = (double)number;
            string result = "";
            List<string> units = new List<string> {"bytes", "KB", "MB", "GB", "TB", "PB" };
            int count = 0;
            do
            {
                result = $"{doubleValue: 0.00} {units[count]}";
                doubleValue = doubleValue / 1024;
                count++;
            }
            while (doubleValue > 1);
            return result;
        }
    }
}
