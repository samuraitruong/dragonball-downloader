﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ConceptDownloader.Models;
//
//    var fShareCloneFileResponse = FShareCloneFileResponse.FromJson(jsonString);

namespace ConceptDownloader.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class FShareCloneFileResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("linkcode")]
        public string Linkcode { get; set; }
    }

    public partial class FShareCloneFileResponse
    {
        public static FShareCloneFileResponse FromJson(string json) => JsonConvert.DeserializeObject<FShareCloneFileResponse>(json, ConceptDownloader.Models.FShareCloneFileResponseConverter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this FShareCloneFileResponse self) => JsonConvert.SerializeObject(self, ConceptDownloader.Models.FShareCloneFileResponseConverter.Settings);
    }

    internal static class FShareCloneFileResponseConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}