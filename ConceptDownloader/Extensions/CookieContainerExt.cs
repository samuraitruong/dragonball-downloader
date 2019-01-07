using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConceptDownloader.Extensions
{
    public static class CookieContainerExt
    {
        public static IEnumerable<Cookie> GetAllCookies(this CookieContainer c)
        {
            Hashtable k = (Hashtable)c.GetType().GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
            foreach (DictionaryEntry element in k)
            {
                SortedList l = (SortedList)element.Value.GetType().GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(element.Value);
                foreach (var e in l)
                {
                    var cl = (CookieCollection)((DictionaryEntry)e).Value;
                    foreach (Cookie fc in cl)
                    {
                        yield return fc;
                    }
                }
            }
        }
        public static void Save(this CookieContainer cc, string filename)
        {
            string dir = Path.GetDirectoryName(filename);
            Directory.CreateDirectory(dir);
            if (File.Exists(filename)) File.Delete(filename);
            var lines = cc.GetAllCookies().Cast<Cookie>().Select(x => $"{x.Name};{x.Value};{x.Domain};{x.Path};{x.Secure}").ToList();
            File.WriteAllLines(filename, lines);
        }
        public static CookieContainer Load(this CookieContainer cc, string filename)
        {
            if (!File.Exists(filename)) return cc;

            var lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                var arr = line.Split(";".ToCharArray());
                cc.Add(new Cookie()
                {
                    Name = arr[0],
                    Path = arr[3],
                    Value = arr[1],
                    Domain = arr[2],
                    Secure = Convert.ToBoolean(arr[4].ToLower())
                });
            };
            return cc;
        }
    }
}
