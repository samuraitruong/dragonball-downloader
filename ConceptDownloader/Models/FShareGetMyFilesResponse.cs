using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConceptDownloader.Models
{
    public class FShareGetMyFilesResponse
    {
        [JsonProperty("parent")]
        public string Parent { get; set; }
        [JsonProperty("items")]
        public List<FShareFile> Items { get; set; }
        public FShareGetMyFilesResponse()
        {
            this.Items = new List<FShareFile>();
        }
    }
}
