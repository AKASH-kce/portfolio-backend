using Microsoft.Extensions.Configuration;
using portfolioAPI.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace portfolioAPI.services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const string SystemMessage = ChatBotSystemData.SystemMessage;

        public ChatService(IConfiguration config)
        {
            _apiKey = config["OpenRouter:ApiKey"];
            _httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(_apiKey))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            _httpClient.DefaultRequestHeaders.Add("X-Title", "AkashPortfolioBot");
        }

        public async Task<string> AskAsync(string question)
        {
            var payload = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[]
                {
                    new { role = "system", content = SystemMessage },
                    new { role = "user", content = question }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);

            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }
    }
}
