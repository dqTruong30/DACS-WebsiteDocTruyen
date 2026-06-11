using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HutechNovel.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<string> GenerateContentAsync(string prompt, string? systemInstruction = null, object? tools = null, System.Func<string?, System.Text.Json.JsonElement, Task<object>>? toolHandler = null)
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY")
            {
                return "Hệ thống AI chưa được cấu hình API Key. Vui lòng liên hệ quản trị viên.";
            }

            // Chuẩn bị dữ liệu gửi đi (sử dụng Gemini Flash cho nhanh và rẻ)
            var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={_apiKey}";

            var contents = new System.Collections.Generic.List<object>
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            };

            while (true)
            {
                object payload;
                if (!string.IsNullOrEmpty(systemInstruction))
                {
                    payload = new
                    {
                        system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                        contents = contents,
                        tools = tools != null ? new[] { tools } : null
                    };
                }
                else
                {
                    payload = new
                    {
                        contents = contents,
                        tools = tools != null ? new[] { tools } : null
                    };
                }

                var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
                var jsonContent = new StringContent(JsonSerializer.Serialize(payload, jsonOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUri, jsonContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var candidates = jsonDoc.RootElement.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var firstCandidate = candidates[0];
                        var content = firstCandidate.GetProperty("content");
                        var parts = content.GetProperty("parts");

                        if (parts.GetArrayLength() > 0)
                        {
                            var firstPart = parts[0];
                            if (firstPart.TryGetProperty("functionCall", out var functionCall))
                            {
                                var functionName = functionCall.GetProperty("name").GetString();
                                var args = functionCall.GetProperty("args");

                                // Ghi lại lịch sử AI đã gọi hàm nguyên bản (tránh lỗi thiếu thought_signature)
                                contents.Add(content);

                                if (toolHandler != null)
                                {
                                    var toolResult = await toolHandler(functionName, args);
                                    
                                    // Gửi lại kết quả hàm cho AI
                                    contents.Add(new
                                    {
                                        role = "function",
                                        parts = new[] { new { functionResponse = new { name = functionName, response = new { name = functionName, content = toolResult } } } }
                                    });
                                    continue; // Lặp lại để gửi request
                                }
                                else
                                {
                                    return "AI yêu cầu gọi hàm nhưng toolHandler chưa được cấu hình.";
                                }
                            }
                            else if (firstPart.TryGetProperty("text", out var textElem))
                            {
                                return textElem.GetString() ?? "Xin lỗi, mình chưa nhận được nội dung trả lời từ AI.";
                            }
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Lỗi từ Google API: {response.StatusCode} - {errorContent}";
                }
                
                return "Xin lỗi, mình đang gặp sự cố khi phân tích phản hồi từ AI.";
            }
        }
    }
}
