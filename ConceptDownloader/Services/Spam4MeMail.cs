using System;
using System.Net;
using System.Net.Http;

namespace ConceptDownloader.Services
{
    public class Spam4MeMail
    {
       
        public CookieContainer Cookies { get; set; }
        public HttpClient client;
        private const string CREATE_EMAIL_URL = "https://www.guerrillamail.com/ajax.php?f=set_email_user";

        public Spam4MeMail()
        {
            this.Cookies = new CookieContainer();
            this.client = new HttpClient();
        }
        //public async Task<GuerrillaCreateEmailResponse> RegisterMailbox(string email, string site = "spam4.me")
        //{
        //    var requestMessage = new HttpRequestMessage(HttpMethod.Post, CREATE_EMAIL_URL);
        //    requestMessage.Content = new StringContent($"email_user={email}&lang=en&site={site}", Encoding.UTF8, "application/x-www-form-urlencoded");

        //    var response = await this.client.SendAsync(requestMessage);
        //    var json = await response.Content.ReadAsStringAsync();
        //    return GuerrillaCreateEmailResponse.FromJson(json);

        //}

        //public async Task<GuerrillaCheckEmailResponse> WaitForEmail(string site = "guerrillamail.com", int timeout = 30000)
        //{
        //    string url = $"https://www.guerrillamail.com/ajax.php?f=check_email&seq=0&site={site}&_={DateTime.Now.Ticks}";
        //    bool exist = false;
        //    while (!exist)
        //    {
        //        var json = await this.client.GetStringAsync(url);
        //        var resposne = GuerrillaCheckEmailResponse.FromJson(json);
        //        resposne.List = resposne.List.Where(x => x.MailFrom != "no-reply@guerrillamail.com").ToList();
        //        if (resposne.List.Count > 0) return resposne;
        //        await Task.Delay(1000 * 2);
        //    }
        //    return null;
        //}

        //public async Task<GuerrillaFetchEmailResponse> FetchMail(string emailId)
        //{
        //    string url = $"https://www.guerrillamail.com/ajax.php?f=fetch_email&email_id=mr_{emailId}&site=guerrillamail.com&_={DateTime.Now.Ticks}";
        //    var json = await this.client.GetStringAsync(url);
        //    var resposne = GuerrillaFetchEmailResponse.FromJson(json);
        //    return resposne;
        //}


    }
}
