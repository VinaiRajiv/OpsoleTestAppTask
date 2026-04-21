using Newtonsoft.Json;
using OpsoleTestApp.Helpers;
using System.Net.Http;
using System.Text;

namespace OpsoleTestApp.Services
{
    public class APIService
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly HttpClient client = new HttpClient();
        private readonly AppConfig config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public APIService(AppConfig appConfig)
        {
            config = appConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="msg"></param>
        /// <param name="isFailure"></param>
        /// <returns></returns>
        public async Task Send(string serial, string msg, bool isFailure)
        {
            try
            {
                var body = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    serialNumber = serial,
                    isError = isFailure,
                    hostName = Environment.MachineName,
                    log = msg,
                    isWarning = false
                };

                string json = JsonConvert.SerializeObject(body);

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Post, config.ApiEndPoint + "/client/logs");
                        req.Headers.Add("Authorization", config.BearerToken);
                        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        var res = await client.SendAsync(req);
                        if (res.IsSuccessStatusCode) return;
                    }
                    catch { }

                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
            }
        }
    }
}
