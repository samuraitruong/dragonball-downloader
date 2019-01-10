using System;
using System.IO;
using Newtonsoft.Json;

namespace LinkFetcher
{
static class ObjectHelper
    {
        public static void Dump<T>(this T x)
        {
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            Console.WriteLine(json);
        }

        public static void WriteTo<T>(this T x, string filename)
        {
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            var path = Path.GetDirectoryName(filename);
            if(!Directory.Exists(path) && !string.IsNullOrEmpty(path)) { 
            Directory.CreateDirectory(path);
            }

            System.IO.File.WriteAllText(filename, json);
        }
    }
}
