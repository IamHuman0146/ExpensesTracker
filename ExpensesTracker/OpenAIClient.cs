using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;  

namespace ExpensesTracker
{
    public static class OpenAIClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        public static async Task<string> GetSavingTipsAsync(string prompt)
        {
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var requestBody = new
            {
                model = "gpt-4o-mini", 
                messages = new[]
                {
                    new { role = "system", content = "You are an AI financial advisor." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 150,
                temperature = 0.7
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseContent);
                return jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }

            return "Failed to generate tips. Please try again later.";
        }
    }
}
