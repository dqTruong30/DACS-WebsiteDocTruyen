using HutechNovel.Data;
using HutechNovel.Models;
using HutechNovel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Controllers
{
    [Authorize]
    public class NhiemVuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INhiemVuService _nhiemVuService;

        public NhiemVuController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INhiemVuService nhiemVuService)
        {
            _context = context;
            _userManager = userManager;
            _nhiemVuService = nhiemVuService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            await _nhiemVuService.EnsureDefaultNhiemVuAsync();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var nhiemVus = await _context.NhiemVus
                .OrderBy(x => x.MaNhiemVu)
                .ToListAsync();

            var lichSu = await _context.LichSuNhiemVus
                .Where(x => x.UserId == user.Id && x.NgayCapNhat >= today && x.NgayCapNhat < tomorrow)
                .ToListAsync();

            ViewBag.LichSu = lichSu;
            ViewBag.DaDiemDanhHomNay = user.NgayDiemDanhCuoi.HasValue && user.NgayDiemDanhCuoi.Value.Date == today;
            ViewBag.HutechXu = user.HutechXu;
            ViewBag.VeDaySach = user.VeDaySach;
            return View(nhiemVus);
        }

        [HttpPost]
        public async Task<IActionResult> DiemDanh()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.NgayDiemDanhCuoi.HasValue && user.NgayDiemDanhCuoi.Value.Date == DateTime.Today)
            {
                return Json(new { success = false, message = "Bạn đã điểm danh hôm nay rồi!" });
            }

            user.NgayDiemDanhCuoi = DateTime.Now;
            var result = await _nhiemVuService.CapNhatTienDoAsync(user.Id, "DiemDanh");

            await _context.SaveChangesAsync();

            var rewardXu = result.NhiemVu?.PhanThuongXu ?? 10;
            var rewardExp = result.NhiemVu?.PhanThuongKinhNghiem ?? 50;
            return Json(new { success = true, message = $"Điểm danh thành công! +{rewardXu} HutechXu, +{rewardExp} EXP" });
        }

        [HttpPost]
        public async Task<IActionResult> MuaVeDaySach()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.HutechXu < 50)
            {
                return Json(new { success = false, message = "Bạn không đủ 50 HutechXu để mua Vé Đẩy Sách!" });
            }

            user.HutechXu -= 50;
            user.VeDaySach += 1;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Mua thành công 1 Vé Đẩy Sách! Chúc bạn tu tiên vui vẻ." });
        }
    }
}
