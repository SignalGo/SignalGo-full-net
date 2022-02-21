using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Http
{
    /// <summary>
    /// reponse of http request
    /// </summary>
    public class HttpClientResponse
    {
        /// <summary>
        /// status
        /// </summary>
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        /// <summary>
        /// data of response
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// response headers
        /// </summary>
        public HttpResponseHeaders ResponseHeaders { get; set; }
    }

    /// <summary>
    /// a parameter data for method call
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// type of parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// value of parameter
        /// </summary>
        public string Value { get; set; }
    }

    public class SignalGoBlazorHttpClient
    {
        public HttpRequestHeaders RequestHeaders { get; set; } = new HttpRequestMessage().Headers;

        public async Task<HttpClientResponse> PostAsync(string url, ParameterInfo[] parameterInfoes)
        {
            using (HttpClient httpClient = new System.Net.Http.HttpClient())
            {
                foreach (KeyValuePair<string, IEnumerable<string>> item in RequestHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }

                MultipartFormDataContent form = new MultipartFormDataContent();
                foreach (ParameterInfo item in parameterInfoes)
                {
                    StringContent jsonPart = new StringContent(item.Value.ToString(), Encoding.UTF8, "application/json");
                    jsonPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                    jsonPart.Headers.ContentDisposition.Name = item.Name;
                    jsonPart.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    form.Add(jsonPart);
                }

                HttpResponseMessage httpresponse = await httpClient.PostAsync(url, form).ConfigureAwait(false);
                if (!httpresponse.IsSuccessStatusCode)
                {
                    // Unwrap the response and throw as an Api Exception:
                    throw new Exception(await httpresponse.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                else
                {
                    httpresponse.EnsureSuccessStatusCode();
                    return new HttpClientResponse() { Data = await httpresponse.Content.ReadAsStringAsync().ConfigureAwait(false), ResponseHeaders = httpresponse.Headers, Status = httpresponse.StatusCode };
                }
            }
        }
    }
}
