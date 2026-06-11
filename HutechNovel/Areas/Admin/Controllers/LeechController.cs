using HutechNovel.Data;
using HutechNovel.Models;
using HutechNovel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Uploader")]
    public class LeechController : Controller
    {
        private readonly ILeechService _leechService;
        private readonly ILeechTaskQueue _taskQueue;
        private readonly ApplicationDbContext _context;
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;

        public LeechController(ILeechService leechService, ILeechTaskQueue taskQueue, ApplicationDbContext context, System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _leechService = leechService;
            _taskQueue = taskQueue;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int maTruyen)
        {
            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen == null) return NotFound("Không tìm thấy truyện.");
            
            ViewBag.MaTruyen = maTruyen;
            ViewBag.TenTruyen = truyen.TieuDe;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Proxy(string url)
        {
            if (string.IsNullOrEmpty(url)) return BadRequest("URL rỗng");

            try
            {
                var puppeteer = HttpContext.RequestServices.GetService(typeof(IPuppeteerProvider)) as IPuppeteerProvider;
                if (puppeteer == null) return Content("Puppeteer Provider is missing.");

                var browser = await puppeteer.GetBrowserAsync();
                await using var page = await browser.NewPageAsync();
                
                await page.GoToAsync(url, new PuppeteerSharp.NavigationOptions
                {
                    WaitUntil = new[] { PuppeteerSharp.WaitUntilNavigation.Networkidle2 },
                    Timeout = 30000
                });

                // Xóa tất cả các thẻ script của web gốc để tránh lỗi CORS khi chạy trong iframe Proxy
                await page.EvaluateExpressionAsync(@"
                    document.querySelectorAll('script, iframe').forEach(el => el.remove());
                ");

                var htmlContent = await page.GetContentAsync();

                var uri = new System.Uri(url);
                var baseHref = $"{uri.Scheme}://{uri.Host}";

                // Sử dụng Regex để chèn <base href> và file script vào <head>
                var baseTag = $"<base href=\"{baseHref}/\" />";
                
                string scriptPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "js", "leech-picker.js");
                string scriptContent = "";
                if (System.IO.File.Exists(scriptPath)) {
                    scriptContent = await System.IO.File.ReadAllTextAsync(scriptPath);
                }
                var scriptTag = $"<script>{scriptContent}</script>";
                
                if (htmlContent.Contains("<head>", System.StringComparison.OrdinalIgnoreCase))
                {
                    htmlContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<head>", $"<head>{baseTag}{scriptTag}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                else
                {
                    htmlContent = baseTag + scriptTag + htmlContent;
                }

                // Vô hiệu hóa Content-Security-Policy chặn frame
                Response.Headers.Remove("X-Frame-Options");
                Response.Headers.Remove("Content-Security-Policy");

                return Content(htmlContent, "text/html", System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                return Content($"<html><body>Lỗi tải trang: {ex.Message}</body></html>", "text/html", System.Text.Encoding.UTF8);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Preview([FromBody] MassLeechRequest request)
        {
            var result = await _leechService.PreviewChapterAsync(request.StartUrl, request.TitleSelector, request.ContentSelector, request.NextSelector);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> StartMassLeech([FromBody] MassLeechRequest request)
        {
            var truyen = await _context.Truyens.FindAsync(request.TruyenId);
            if (truyen == null) return BadRequest("Truyện không tồn tại.");

            await _taskQueue.QueueLeechRequestAsync(request);

            try
            {
                var uri = new System.Uri(request.StartUrl);
                var domain = uri.Host;

                var existingConfig = await _context.CauHinhLeeches.FirstOrDefaultAsync(c => c.Domain == domain);
                if (existingConfig == null)
                {
                    _context.CauHinhLeeches.Add(new CauHinhLeech
                    {
                        Domain = domain,
                        TitleSelector = request.TitleSelector,
                        ContentSelector = request.ContentSelector,
                        NextChapterSelector = request.NextSelector
                    });
                }
                else
                {
                    existingConfig.TitleSelector = request.TitleSelector;
                    existingConfig.ContentSelector = request.ContentSelector;
                    existingConfig.NextChapterSelector = request.NextSelector;
                    _context.CauHinhLeeches.Update(existingConfig);
                }
                await _context.SaveChangesAsync();
            }
            catch { }

            return Ok(new { success = true, message = "Đã đưa vào tiến trình chạy ngầm thành công." });
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus(int maTruyen)
        {
            var tasks = await _context.TienTrinhLeeches
                .Where(t => t.MaTruyen == maTruyen)
                .OrderByDescending(t => t.NgayBatDau)
                .Take(5)
                .ToListAsync();

            return Json(tasks);
        }

        [HttpGet]
        public async Task<IActionResult> GetConfig(string domain)
        {
            var config = await _context.CauHinhLeeches.FirstOrDefaultAsync(c => c.Domain == domain);
            return Json(config);
        }
    }
}
