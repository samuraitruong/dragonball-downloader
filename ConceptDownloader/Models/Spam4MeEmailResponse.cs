using System;
namespace ConceptDownloader.Models
{
   
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Spam4MeEmailResponse
    {
        [JsonProperty("list")]
        public List<List> List { get; set; }

        [JsonProperty("count")]
        public string Count { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("ts")]
        public long Ts { get; set; }

        [JsonProperty("sid_token")]
        public string SidToken { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        //[JsonProperty("auth")]
        //public Auth Auth { get; set; }
    }


    public partial class List
    {
        [JsonProperty("mail_id")]
        public string MailId { get; set; }

        [JsonProperty("mail_from")]
        public string MailFrom { get; set; }

        [JsonProperty("mail_subject")]
        public string MailSubject { get; set; }

        [JsonProperty("mail_excerpt")]
        public string MailExcerpt { get; set; }

        [JsonProperty("mail_timestamp")]
        public string MailTimestamp { get; set; }

        [JsonProperty("mail_read")]
        public string MailRead { get; set; }

        [JsonProperty("mail_date")]
        public DateTimeOffset MailDate { get; set; }

        [JsonProperty("att")]
        public string Att { get; set; }

        [JsonProperty("mail_size")]
        public string MailSize { get; set; }
    }

    public partial class Stats
    {
        [JsonProperty("sequence_mail")]
        public string SequenceMail { get; set; }

        [JsonProperty("created_addresses")]
        public long CreatedAddresses { get; set; }

        [JsonProperty("received_emails")]
        public string ReceivedEmails { get; set; }

        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("total_per_hour")]
        public string TotalPerHour { get; set; }
    }

    public partial class Spam4MeEmailResponse
    {
        public static Spam4MeEmailResponse FromJson(string json) => JsonConvert.DeserializeObject<Spam4MeEmailResponse>(json, Spam4MeEmailResponseConverter.Settings);
    }

    public static class GuerrillaCheckEmailResponseSerialize
    {
        public static string ToJson(this Spam4MeEmailResponse self) => JsonConvert.SerializeObject(self, Spam4MeEmailResponseConverter.Settings);
    }

    internal static class Spam4MeEmailResponseConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }


}
