using System.Threading.Tasks;

namespace HutechNovel.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt, string systemInstruction = null, object tools = null, System.Func<string, System.Text.Json.JsonElement, Task<object>> toolHandler = null);
    }
}
