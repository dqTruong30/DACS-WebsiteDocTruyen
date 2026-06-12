using Microsoft.AspNetCore.Mvc;

namespace HutechNovel.Controllers
{
    /// <summary>
    /// Backend proxy cho Google Translate TTS.
    /// Mục đích: Trình duyệt không thể gọi trực tiếp Google TTS (bị block CORS/Referrer).
    /// Server gọi thay, stream audio MP3 về cho frontend.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TTSController : ControllerBase
    {
        private static readonly HttpClient _http = new HttpClient(new HttpClientHandler
        {
            // Tự động theo redirect (Google TTS đôi khi redirect)
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 3
        })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        static TTSController()
        {
            // Giả lập browser để Google không block
            _http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            _http.DefaultRequestHeaders.Add("Accept", "audio/mpeg, audio/*;q=0.8, */*;q=0.5");
            _http.DefaultRequestHeaders.Add("Accept-Language", "vi-VN,vi;q=0.9,en;q=0.8");
            _http.DefaultRequestHeaders.Add("Referer", "https://translate.google.com/");
        }

        /// <summary>
        /// GET /api/TTS/Speak?text=...&lang=vi
        /// Trả về audio/mpeg (MP3) từ Google Translate TTS.
        /// </summary>
        [HttpGet("Speak")]
        public async Task<IActionResult> Speak([FromQuery] string text, [FromQuery] string lang = "vi")
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("text is required");

            // Giới hạn độ dài (Google TTS giới hạn ~200 ký tự)
            if (text.Length > 200) text = text[..200];

            // Lọc ký tự không hợp lệ
            lang = System.Text.RegularExpressions.Regex.Replace(lang, "[^a-zA-Z\\-]", "");
            if (string.IsNullOrEmpty(lang)) lang = "vi";

            var url = $"https://translate.google.com/translate_tts" +
                      $"?ie=UTF-8&q={Uri.EscapeDataString(text)}&tl={lang}&client=tw-ob&ttsspeed=1";

            try
            {
                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode,
                        $"Google TTS trả về {response.StatusCode}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "audio/mpeg";
                var bytes = await response.Content.ReadAsByteArrayAsync();

                // Cache phía client 1 giờ để giảm request lặp lại
                Response.Headers.Append("Cache-Control", "public, max-age=3600");

                return File(bytes, contentType);
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, "Google TTS timeout");
            }
            catch (Exception ex)
            {
                return StatusCode(502, $"Proxy lỗi: {ex.Message}");
            }
        }
    }
}
