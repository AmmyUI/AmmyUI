using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AmmySEA.StackExchangeApi
{
    public class Api
    {
        public Task<SiteList> GetSites()
        {
            var url = string.Format("https://api.stackexchange.com/2.2/sites?pagesize=500&key=SHI*m2JJJZ0t4*08A9rVZQ((");
            return DownloadObject<SiteList>(url);
        }

        public Task<QuestionList> GetQuestions(string siteName, int questionCount)
        {
            var url = string.Format("https://api.stackexchange.com/2.2/questions?pagesize=" + questionCount + "&key=SHI*m2JJJZ0t4*08A9rVZQ((&order=desc&sort=activity&site=" + siteName);
            return DownloadObject<QuestionList>(url);
        }

        private static async Task<T> DownloadObject<T>(string url)
        {
            var responseText = await DownloadJson(url);
            return (T) new JavaScriptSerializer().Deserialize(responseText, typeof (T));
        }

        private static async Task<string> DownloadJson(string url)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);

            httpWebRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            httpWebRequest.Method = "GET";

            var httpResponse = await httpWebRequest.GetResponseAsync();

            string responseText;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                responseText = await streamReader.ReadToEndAsync();

            return responseText;
        }
    }
}