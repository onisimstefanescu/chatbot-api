using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Chatbot.Api.Models;
using Microsoft.Extensions.Options;

namespace Chatbot.Api.Services
{
    public class ChatService
    {
        private readonly ChatbotConfig _config;
        private readonly HttpClient _http;

        public ChatService(
            IOptions<ChatbotConfig> chatbotOptions,
            IConfiguration configuration)
        {
            _config = chatbotOptions.Value;

            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", configuration["OPENAI_API_KEY"]);
        }

        public async Task<string> Ask(string siteId, string question)
        {
            if (!_config.Sites.TryGetValue(siteId, out var site))
                throw new Exception($"SiteId necunoscut: {siteId}");

            var prompt = _config.PromptTemplate
                .Replace("{SITE_ID}", siteId)
                .Replace("{CONTEXT}", site.Context);

            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "system", content = prompt },
                new { role = "user", content = question }
            }
            };



            var response = await _http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new Exception("AI indisponibil temporar. Te rugăm să încerci mai târziu.");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
    }

}

//public class ChatService
//{
//    private readonly IConfiguration _config;
//    private readonly HttpClient _http;

//    public ChatService(IConfiguration config)
//    {
//        _config = config;
//        _http = new HttpClient();
//        _http.DefaultRequestHeaders.Authorization =
//            new AuthenticationHeaderValue("Bearer", _config["OPENAI_API_KEY"]);
//    }

//    public async Task<string> Ask(string siteId, string question)
//    {
//        var context = File.ReadAllText($"Sites/{siteId}.txt");
//        var systemPrompt = File.ReadAllText("Prompts/system-prompt.txt")
//            .Replace("{SITE_ID}", siteId)
//            .Replace("{CONTEXT}", context);

//        var payload = new
//        {
//            model = "gpt-4o-mini",
//            messages = new[]
//            {
//                new { role = "system", content = systemPrompt },
//                new { role = "user", content = question }
//            }
//        };

//        var apiKey = _config["OPENAI_API_KEY"];

//        if (string.IsNullOrWhiteSpace(apiKey))
//        {
//            throw new Exception("OPENAI_API_KEY is missing or empty");
//        }

//        _http.DefaultRequestHeaders.Authorization =
//            new AuthenticationHeaderValue("Bearer", apiKey);

//        var response = await _http.PostAsync(
//            "https://api.openai.com/v1/chat/completions",
//            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
//        );

//        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
//        {
//            throw new Exception("AI indisponibil temporar. Te rugăm să încerci mai târziu.");
//        }

//        var json = await response.Content.ReadAsStringAsync();
//        using var doc = JsonDocument.Parse(json);
//        return doc.RootElement
//            .GetProperty("choices")[0]
//            .GetProperty("message")
//            .GetProperty("content")
//            .GetString()!;
//    }
//}

