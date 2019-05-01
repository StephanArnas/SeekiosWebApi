using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServiceSeekiosUnitTest.Helper
{
    public class HttpRequestHelper
    {
        #region ----- PRIVATE VARIABLES -----------------------------------------------------------------------

        private static HttpClient _httpClient = new HttpClient();

        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        public static async Task<string> GetRequestAsync(string url)
        {
            string result = string.Empty;

            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.GetAsync(url);
                result = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                if (result.Contains("Unauthorized access"))
                {
                    throw new Exception("Unauthorized access");
                }
            }
            return result;
        }

        #endregion
    }
}
