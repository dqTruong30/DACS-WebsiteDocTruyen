using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào
    public class KiemDuyetTruyenController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public KiemDuyetTruyenController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.Truyens
                .Include(t => t.NguoiDang)
                .Include(t => t.TacGia)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.TieuDe.Contains(keyword) || (t.NguoiDang != null && t.NguoiDang.UserName != null && t.NguoiDang.UserName.Contains(keyword)));
            }

            // Lấy TẤT CẢ truyện, sắp xếp mới nhất lên đầu
            var stories = await query.OrderByDescending(t => t.NgayTao).ToListAsync();
            return View(stories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id, TrangThaiTruyen trangThaiMoi)
        {
            var truyen = await _context.Truyens.FindAsync(id);
            if (truyen != null)
            {
                truyen.TrangThai = trangThaiMoi;
                _context.NhatKyQuanTris.Add(new NhatKyQuanTri
                {
                    MaNguoiDung = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    HanhDong = "Đổi trạng thái truyện",
                    DoiTuong = "Truyen",
                    MaDoiTuong = truyen.MaTruyen,
                    NoiDung = $"Chuyển '{truyen.TieuDe}' thành {trangThaiMoi}",
                    NgayTao = DateTime.Now
                });
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Đã chuyển trạng thái truyện '{truyen.TieuDe}' thành {trangThaiMoi}");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThaiChuong(int id, TrangThaiChuong trangThaiMoi)
        {
            var chuong = await _context.Chuongs
                .Include(c => c.Truyen)
                .FirstOrDefaultAsync(c => c.MaChuong == id);

            if (chuong != null)
            {
                chuong.TrangThai = trangThaiMoi;
                if (trangThaiMoi == TrangThaiChuong.DaXuatBan)
                {
                    chuong.NgayHenGio = null;
                    chuong.Truyen.NgayCapNhat = DateTime.Now;
                }

                chuong.Truyen.TongSoChuong = await _context.Chuongs
                    .CountAsync(c => c.MaTruyen == chuong.MaTruyen);

                _context.NhatKyQuanTris.Add(new NhatKyQuanTri
                {
                    MaNguoiDung = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    HanhDong = "Đổi trạng thái chương",
                    DoiTuong = "Chuong",
                    MaDoiTuong = chuong.MaChuong,
                    NoiDung = $"Chuyển chương {chuong.SoChuong} của '{chuong.Truyen.TieuDe}' thành {trangThaiMoi}",
                    NgayTao = DateTime.Now
                });

                await _context.SaveChangesAsync();
                SetSuccessMessage($"Đã cập nhật chương {chuong.SoChuong}: {chuong.TieuDe}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
