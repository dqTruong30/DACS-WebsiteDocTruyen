using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminCauHinhController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminCauHinhController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var config = await _context.CauHinhHeThongs.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new CauHinhHeThong();
                _context.CauHinhHeThongs.Add(config);
                await _context.SaveChangesAsync();
            }
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuCauHinh(CauHinhHeThong model, IFormFile? fileBanner, string? linkBanner, bool removeBanner = false)
        {
            ModelState.Remove("BannerUrl");

            if (ModelState.IsValid)
            {
                var config = await _context.CauHinhHeThongs.FirstOrDefaultAsync();
                if (config != null)
                {
                    config.TenWebsite = model.TenWebsite;
                    config.ThongBaoToanCuc = model.ThongBaoToanCuc;
                    config.CheDoBaoTri = model.CheDoBaoTri;
                    config.TieuDeSEO = model.TieuDeSEO;
                    config.MoTaSEO = model.MoTaSEO;
                    config.EmailLienHe = model.EmailLienHe;

                    void DeletePhysicalFile(string? url)
                    {
                        if (!string.IsNullOrEmpty(url) && url.StartsWith("/uploads/banners/"))
                        {
                            var filePath = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                    }

                    if (removeBanner)
                    {
                        DeletePhysicalFile(config.BannerUrl);
                        config.BannerUrl = null;
                    }
                    else if (fileBanner != null && fileBanner.Length > 0)
                    {
                        DeletePhysicalFile(config.BannerUrl);

                        string fileName = "home-banner-" + Guid.NewGuid() + Path.GetExtension(fileBanner.FileName);
                        string uploadPath = Path.Combine(_env.WebRootPath, "uploads/banners");
                        Directory.CreateDirectory(uploadPath);

                        using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                        {
                            await fileBanner.CopyToAsync(stream);
                        }
                        config.BannerUrl = "/uploads/banners/" + fileName;
                    }
                    else if (!string.IsNullOrWhiteSpace(linkBanner))
                    {
                        if (config.BannerUrl != linkBanner)
                        {
                            DeletePhysicalFile(config.BannerUrl);
                            config.BannerUrl = linkBanner;
                        }
                    }

                    await _context.SaveChangesAsync();
                    SetSuccessMessage("Đã cập nhật cấu hình hệ thống thành công!");
                }
                return RedirectToAction(nameof(Index));
            }

            SetErrorMessage("Dữ liệu không hợp lệ.");
            return View("Index", model);
        }
    }
}