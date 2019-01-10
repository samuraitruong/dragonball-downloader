using System;
using System.Text.RegularExpressions;
using System.Web;

namespace System
{
    public static class StringExtensions
    {
        public static string ToMovieShortName(this string input)
        {
            var replacedInput = HttpUtility.UrlDecode(input.ToLower());
            replacedInput = replacedInput.Replace("ultra.hd", string.Empty);
            replacedInput = replacedInput.Replace("bluray", string.Empty);
            replacedInput = replacedInput.Replace("blu-ray", string.Empty);
            replacedInput = replacedInput.Replace("fullhd", string.Empty);
            replacedInput = replacedInput.Replace("full.hd", string.Empty);
            replacedInput = replacedInput.Replace("repack", string.Empty);
            replacedInput = replacedInput.Replace("unrated", string.Empty);
            replacedInput = replacedInput.Replace("extended", string.Empty);
            replacedInput = replacedInput.Replace("vie", string.Empty);
            replacedInput = replacedInput.Replace("internal", "");
            replacedInput = replacedInput.Replace("uhd", "");
            replacedInput = replacedInput.Replace("ultrahd", "");
            replacedInput = replacedInput.Replace("imax", string.Empty);
            replacedInput = replacedInput.Replace("dubbed", string.Empty);
            replacedInput = replacedInput.Replace("ac3", string.Empty);
            replacedInput = replacedInput.Replace(".dl", string.Empty);
            replacedInput = replacedInput.Replace("german", string.Empty);
            replacedInput = replacedInput.Replace("limited", string.Empty);
            replacedInput = replacedInput.Replace("directors.cut", "");
            replacedInput = replacedInput.Replace("theatrical.cut.", string.Empty);
            replacedInput = replacedInput.Replace(" ", ".");
            replacedInput = replacedInput.Replace("-", ".");
            replacedInput = replacedInput.Replace("_", ".");
            replacedInput = replacedInput.Replace("4k", "");
            replacedInput = replacedInput.Replace("remastered", "");
            replacedInput = replacedInput.Replace("mastered.in", "");
            replacedInput = Regex.Replace(replacedInput, "\\.{2,}", ".");
            replacedInput = Regex.Replace(replacedInput, "\\d{3,}p", "|");
            //replacedInput = replacedInput.Replace("1080p", "|");
            //replacedInput = replacedInput.Replace("720p", "|");
            //replacedInput = replacedInput.Replace("2160p", "|");

            var name= replacedInput.Split('|')[0];
            if (name.Length == input.Length) return input;
            return name.TrimEnd('.');

        }
    }
}
