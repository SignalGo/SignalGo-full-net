using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.Engines.Models
{
    public class HealthCheckInfo
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string CheckUrl { get; set; }
        /// <summary>
        /// when its post
        /// </summary>
        public string DataToPostForGetResult { get; set; }
        /// <summary>
        /// when result value of getresultulrl has this text contains
        /// </summary>
        public string ConditionResultHasValue { get; set; }
        public string InvalidHealthCheckUrl { get; set; }
        /// <summary>
        /// data to post when health check is not valid
        /// </summary>
        public string DataToPostForInvalidHealthCheck { get; set; }
        /// <summary>
        /// health check every time
        /// </summary>
        public string CheckTime { get; set; }
        public string TimeToPostDataWhenIsNotHealthy { get; set; }
        public string RestartTimeAfterNotHealthy { get; set; }
        DateTime? LastWasHealthy { get; set; }
        DateTime LastPostHealthy { get; set; } = DateTime.MinValue;

        public async Task<bool> Check(ServerInfo serverInfo)
        {
            var result = await CheckIsHealthy(serverInfo);
            if (!result)
            {
                await PostWhenHealthyIsInvalid(serverInfo);
            }
            return result;
        }

        string CombineUrls(params string[] urls)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < urls.Length; i++)
            {
                stringBuilder.Append(urls[i].Trim('/'));
                if (i != urls.Length - 1)
                    stringBuilder.Append('/');
            }
            return stringBuilder.ToString();
        }

        public async Task<bool> CheckIsHealthy(ServerInfo serverInfo)
        {
            if (!IsEnabled || string.IsNullOrEmpty(serverInfo.HealthCheckUrl))
                return true;
            try
            {
                var split = serverInfo.HealthCheckUrl.Split('#');
                if (split.Length > 1)
                {
                    if (!split[1].Equals(Name, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                HttpResponseMessage responseMessage = null;
                if (string.IsNullOrEmpty(DataToPostForGetResult))
                {
                    using var client = new HttpClient();
                    responseMessage = await client.GetAsync(CombineUrls(split[0], CheckUrl));
                }
                else
                {
                    var postData = DataToPostForGetResult.Replace("$ProjectName", serverInfo.Name);
                    var content = new StringContent(postData, Encoding.UTF8, "application/json");
                    using var client = new HttpClient();
                    responseMessage = await client.PostAsync(CombineUrls(split[0], CheckUrl), content);
                }
                var responseText = await responseMessage.Content.ReadAsStringAsync();
                var result = responseText.Contains(ConditionResultHasValue);
                if (result)
                    LastWasHealthy = DateTime.Now;
                return result;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, $"CheckIsHealthy {Name} has error!");
                return false;
            }
        }

        async Task PostWhenHealthyIsInvalid(ServerInfo serverInfo)
        {
            if (!IsEnabled || string.IsNullOrEmpty(InvalidHealthCheckUrl))
                return;
            bool canPost = false;
            if (!string.IsNullOrEmpty(TimeToPostDataWhenIsNotHealthy))
            {
                if (LastWasHealthy.HasValue)
                {
                    if (TimeSpan.TryParse(TimeToPostDataWhenIsNotHealthy, out TimeSpan time) && time > DateTime.Now - LastPostHealthy)
                    {
                        canPost = true;
                    }
                }
            }
            if (!canPost)
                return;
            try
            {
                HttpResponseMessage responseMessage = null;
                if (string.IsNullOrEmpty(DataToPostForInvalidHealthCheck))
                {
                    using var client = new HttpClient();
                    responseMessage = await client.GetAsync(InvalidHealthCheckUrl);
                }
                else
                {
                    var postData = DataToPostForInvalidHealthCheck.Replace("$ProjectName", serverInfo.Name);
                    var content = new StringContent(postData, Encoding.UTF8, "application/json");
                    using var client = new HttpClient();
                    responseMessage = await client.PostAsync(InvalidHealthCheckUrl, content);
                }
                var responseText = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, $"PostWhenHealthyIsInvalid {Name} has error!");
            }
        }
    }
}
