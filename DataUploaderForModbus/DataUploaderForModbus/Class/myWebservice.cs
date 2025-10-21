using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LIB_Common
{
    public class myWebservice
    {
        private readonly string _url;
        private readonly HttpClient _client;

        public myWebservice(string url)
        {
            _url = url;
            _client = new HttpClient();
        }

        public async Task<string> PostAsync<T>(Dictionary<string, T> parameters)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(parameters);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(_url, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"HTTP 오류: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // 예외 로깅 혹은 다시 throw 가능
                throw new Exception($"요청 실패: {ex.Message}", ex);
            }
        }
    }
}
