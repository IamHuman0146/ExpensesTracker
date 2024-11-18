using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExpensesTracker
{
    public static class HuggingFaceApiClient
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string apiUrl = "https://api-inference.huggingface.co/models/EleutherAI/gpt-neo-1.3B";

        public static async Task<string> GenerateSavingsTipsAsync(string prompt, string apiKey)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = JsonConvert.SerializeObject(new { inputs = prompt });
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return $"Failed to generate tips. Error: {errorDetails}";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseContent);

                if (result != null && result.Count > 0 && result[0].generated_text != null)
                {
                    return result[0].generated_text;
                }
                else
                {
                    return "Unexpected response format received from the API.";
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
