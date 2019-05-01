using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WCFServiceWebRole.Data.DB;
using WCFServiceWebRole.Data.ERROR;

namespace ServiceSeekiosUnitTest.Helper
{
    public class HttpRequestHelper
    {
        #region ----- PUBLIC VARIABLES ------------------------------------------------------------------------

        public static DBToken Token { get; set; }

        #endregion

        #region ----- PRIVATE VARIABLES -----------------------------------------------------------------------

        private static HttpClient _httpClient = new HttpClient();
        private const string _TOKEN_HEADER = "token";
        
        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        public static async Task<string> GetRequestAsync(string url, bool authenticationNeeded = true)
        {
            bool authenticationSuccess = true;
            string result = string.Empty;

            if (authenticationNeeded) authenticationSuccess = await AddAuthenticationHeaders();
            HttpResponseMessage response = null;
            try
            {
                if (authenticationSuccess)
                {
                    response = await _httpClient.GetAsync(url);
                    result = response.Content.ReadAsStringAsync().Result;
                }
                else return null;
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

        public static async Task<string> PostRequestAsync(string url, string json, bool authenticationNeeded = true)
        {
            string result = string.Empty;

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authenticationNeeded) await AddAuthenticationHeaders();
            var param = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync(new Uri(url), param);
            }
            catch (Exception ex)
            {
                throw new TimeoutException(ex.Message);
            }
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                if (result.Contains("Unauthorized access"))
                {
                    throw new Exception("Unauthorized access");
                }
            }
            using (HttpContent content = response.Content)
            {
                result = content.ReadAsStringAsync().Result;
            }
           return result;
        }

        public static async Task<string> PutRequestAsync(string url, string json, bool authenticationNeeded = true)
        {
            string result = string.Empty;

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authenticationNeeded) await AddAuthenticationHeaders();
            var param = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PutAsync(new Uri(url), param);
            }
            catch (Exception ex)
            {
                throw new TimeoutException(ex.Message);
            }
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                if (result.Contains("Unauthorized access"))
                {
                    throw new Exception("Unauthorized access");
                }
            }
            using (HttpContent content = response.Content)
            {
                result = content.ReadAsStringAsync().Result;
            }
            return result;
        }

        public static async Task<string> DeleteRequestAsync(string url, bool authenticationNeeded = true)
        {
            string result = string.Empty;
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authenticationNeeded) await AddAuthenticationHeaders();
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.DeleteAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                throw new TimeoutException(ex.Message);
            }
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                if (result.Contains("Unauthorized access"))
                {
                    throw new Exception("Unauthorized access");
                }
            }
            using (HttpContent content = response.Content)
            {
                result = content.ReadAsStringAsync().Result;
            }
            return result;
        }

        #endregion

        #region ----- PRIVATE METHODS -------------------------------------------------------------------------

        private async static Task<bool> AddAuthenticationHeaders()
        {
            bool success = true;

            _httpClient.DefaultRequestHeaders.Clear();
            if (Token == null || (DateTime.UtcNow > Token.DateExpires))
            {
                success = await ConnectUser(ServiceSeekiosUnitTest.EMAIL, ServiceSeekiosUnitTest.PASSWORD);
            }
            if (success)
            {
                _httpClient.DefaultRequestHeaders.Add(_TOKEN_HEADER, Token.AuthToken);
                return true;
            }
            return false;
        }

        private static async Task<bool> ConnectUser(string email, string pwd)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd)) return false;
            try
            {
                var url = string.Format("{0}/{1}/{2}", ServiceSeekiosUnitTest.LOGIN_URL, email, pwd);
                var json = await GetRequestAsync(url, false);
                Token = JsonConvert.DeserializeObject<DBToken>(json);
                if (Token?.AuthToken == null)
                {
                    Token = null;
                    var error = JsonConvert.DeserializeObject<DefaultCustomError>(json);
                    return false;
                }
            }
            catch (Exception e)
            {
                var error = e.InnerException + "\n" + e.Message;
            }
            return true;
        }

        #endregion
    }
}
