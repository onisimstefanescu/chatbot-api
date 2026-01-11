using Chatbot.Api.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Chatbot.Api.Services
{
    public class SupabaseService
    {
        private readonly HttpClient _http;
        private readonly string _url;

        public SupabaseService(IConfiguration config)
        {
            _url = config["SUPABASE_URL"]!;
            var key = config["SUPABASE_ANON_KEY"]!;

            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("apikey", key);
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", key);
        }

        public async Task CreateLead(string name, string email, string site)
        {
            var payload = new
            {
                name,
                email,
                site
            };

            var response = await _http.PostAsync(
                $"{_url}/rest/v1/leads",
                new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                )
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Supabase error: {error}");
            }
        }

        public async Task<List<LeadDto>> GetLeads()
        {
            var response = await _http.GetAsync(
                $"{_url}/rest/v1/leads?select=*"
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Supabase error: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var leads = JsonSerializer.Deserialize<List<LeadDto>>(json, options);

            if (leads == null)
            {
                throw new Exception("Deserialization failed");
            }

            return leads;
        }
    }
}




