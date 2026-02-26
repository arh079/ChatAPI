using ChatAPI.DTO;
using ChatAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ChatAPI.Services
{
    public class AIService : IAIService
    {
    
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        public AIService(HttpClient _httpClient, IConfiguration _configuration)
        {
            httpClient = _httpClient;
            configuration = _configuration;
        }

        public string CleanAiResponse(string message)
        {
            message.Replace("**", "")
                .Replace("*", "")
                .Replace("\\n", "")
                .Replace("#", "")
                .Replace("\\", "")                
                .Replace("\\n", "");
            return message;
        }

        public async Task<string> GetResponseAsync(List<AIMessageDto> messages)
        {
            var apiKey = configuration["OpenRouter:ApiKey"];
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:4200");
            httpClient.DefaultRequestHeaders.Add("X-Title", "ChatAPI");
            // httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey); 
            //var contents = 
            //    messages.Select(m => new { role = m.Role == "assistant" ? "model" : "user", 
            //        parts = new[] { new { text = m.Content } } }).ToList();
           
            var systemMessage = new AIMessageDto
            {
                Role = "system",
                Content =
                $@" You are an assistant specialized only in e-commerce and selling electronic devices Like Phones and Laptops.
                      Your job is to:
             - Do NOT use Markdown or any formatting symbols such as \n,\\ **, *, or #. 
             - Do NOT include Symbols in the response Please. 
             - Respond using plain text only.
             - Do not answer any question outside the scope of e-commerce and electronic products
             - Never answer questions about politics, religion, sports, space, or general knowledge. 
             - Keep answers short and persuasive.
             - Speak Arabic if the user writes Arabic.
             - Speak in friendly professional tone.
             - Stay focused only on electronics and store-related topics.
             -If a question is unrelated, reply with: 'I am specialized only in e-commerce and electronic products.'"
            };

            messages.Insert(0, systemMessage);

            var formattedMessages = messages.Select(m => new
            {
                role = m.Role,
                content = m.Content

            }).ToList();

            var body =
                new
                {
                    model = "openrouter/free",
                    messages = formattedMessages
                };
            var json =
                JsonSerializer.Serialize(body);

            var response = await httpClient.PostAsync(
                      "https://openrouter.ai/api/v1/chat/completions",
                     new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorBody}";
            }
            var responseText =
                await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseText);
            if (!doc.RootElement.TryGetProperty("choices", out var choices))
            {
                throw new ApplicationException($"Unexpected response: {responseText}");
            }
            return choices[0]
             .GetProperty("message")
             .GetProperty("content")
             .GetString();
            //return doc.RootElement
            //    .GetProperty("candidates")[0]
            //    .GetProperty("content")
            //    .GetProperty("parts")[0]
            //    .GetProperty("text")
            //    .GetString();
        }
    }
}
