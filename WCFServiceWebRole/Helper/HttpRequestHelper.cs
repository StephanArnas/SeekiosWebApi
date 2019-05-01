using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WCFServiceWebRole.Helper
{
    public class HttpRequestHelper
    {
        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        /// <summary>
        /// Make a POST HTTP request
        /// </summary>
        /// <param name="url">target url</param>
        /// <param name="json">object to put in parameter in the request</param>
        /// <returns>the response of the server</returns>
        public static async Task<string> PostRequestAsync(string url, string json)
        {
            string result = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var param = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(new Uri(url), param);
                using (HttpContent content = response.Content)
                {
                    result = content.ReadAsStringAsync().Result;
                }
            }
            return result;
        }

        #endregion
    }
}