using HutechNovel.Data;
using HutechNovel.Models;
using HutechNovel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HutechNovel.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TuongTacController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INhiemVuService _nhiemVuService;

        public TuongTacController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INhiemVuService nhiemVuService)
        {
            _context = context;
            _userManager = userManager;
            _nhiemVuService = nhiemVuService;
        }

        [HttpPost("DanhGia")]
        public async Task<IActionResult> DanhGia([FromForm] int maTruyen, [FromForm] int diemSo)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();
            if (diemSo < 1 || diemSo > 5) return BadRequest(new { success = false, message = "Điểm đánh giá không hợp lệ." });

            var dg = await _context.DanhGias.FirstOrDefaultAsync(d => d.MaTruyen == maTruyen && d.MaNguoiDung == userId);

            if (dg == null)
            {
                _context.DanhGias.Add(new DanhGia { MaTruyen = maTruyen, MaNguoiDung = userId, DiemSo = diemSo });
            }
            else
            {
                dg.DiemSo = diemSo;
            }

            await _context.SaveChangesAsync();

            // Cập nhật lại điểm trung bình và tổng sao cho truyện
            var truyen = await _context.Truyens.Include(t => t.DanhGias).FirstAsync(t => t.MaTruyen == maTruyen);
            truyen.DiemDanhGiaTrungBinh = truyen.DanhGias.Average(d => d.DiemSo);
            truyen.TongSoSao = truyen.DanhGias.Sum(d => d.DiemSo);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                diemMoi = truyen.DiemDanhGiaTrungBinh,
                tongSao = truyen.TongSoSao,
                totalRatings = truyen.DanhGias.Count
            });
        }

        [HttpPost("YeuThich")]
        public async Task<IActionResult> YeuThich([FromForm] int maTruyen)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var existing = await _context.YeuThichs
                .FirstOrDefaultAsync(x => x.MaTruyen == maTruyen && x.MaNguoiDung == userId);

            var active = existing == null;
            if (existing == null)
            {
                _context.YeuThichs.Add(new YeuThich { MaTruyen = maTruyen, MaNguoiDung = userId });
            }
            else
            {
                _context.YeuThichs.Remove(existing);
            }

            await _context.SaveChangesAsync();
            var count = await _context.YeuThichs.CountAsync(x => x.MaTruyen == maTruyen);
            return Ok(new { success = true, active, count });
        }

        [HttpPost("DanhDau")]
        public async Task<IActionResult> DanhDau([FromForm] int maTruyen)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var existing = await _context.DanhDaus
                .FirstOrDefaultAsync(x => x.MaTruyen == maTruyen && x.MaNguoiDung == userId);

            var active = existing == null;
            if (existing == null)
            {
                _context.DanhDaus.Add(new DanhDau { MaTruyen = maTruyen, MaNguoiDung = userId });
            }
            else
            {
                _context.DanhDaus.Remove(existing);
            }

            await _context.SaveChangesAsync();
            var count = await _context.DanhDaus.CountAsync(x => x.MaTruyen == maTruyen);
            return Ok(new { success = true, active, count });
        }

        [HttpPost("TheoDoi")]
        public async Task<IActionResult> TheoDoi([FromForm] int maTruyen)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var existing = await _context.TheoDoiTruyens
                .FirstOrDefaultAsync(x => x.MaTruyen == maTruyen && x.MaNguoiDung == userId);

            var active = existing == null;
            if (existing == null)
            {
                _context.TheoDoiTruyens.Add(new TheoDoiTruyen { MaTruyen = maTruyen, MaNguoiDung = userId });
            }
            else
            {
                _context.TheoDoiTruyens.Remove(existing);
            }

            await _context.SaveChangesAsync();
            var count = await _context.TheoDoiTruyens.CountAsync(x => x.MaTruyen == maTruyen);
            return Ok(new { success = true, active, count });
        }
        [HttpPost("BinhLuan")]
        public async Task<IActionResult> BinhLuan([FromForm] int maTruyen, [FromForm] string noiDung, [FromForm] int? maBinhLuanCha, [FromForm] bool laSpoiler = false)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(noiDung))
                return BadRequest(new { success = false, message = "Nội dung trống" });

            var comment = new BinhLuan
            {
                MaTruyen = maTruyen,
                MaNguoiDung = userId,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                LaSpoiler = laSpoiler,
                MaBinhLuanCha = maBinhLuanCha // <--- Lưu ID của bình luận cha
            };

            _context.BinhLuans.Add(comment);

            // Cập nhật thống kê số bình luận cho user
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.SoBinhLuan += 1;
                await _nhiemVuService.CapNhatTienDoAsync(user.Id, "BinhLuan");
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("CamXucBinhLuan")]
        public async Task<IActionResult> CamXucBinhLuan([FromForm] int maBinhLuan)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var comment = await _context.BinhLuans.FirstOrDefaultAsync(c => c.MaBinhLuan == maBinhLuan);
            if (comment == null) return NotFound(new { success = false });

            var existed = await _context.BinhLuanCamXucs
                .AnyAsync(x => x.MaBinhLuan == maBinhLuan && x.MaNguoiDung == userId);
            if (existed)
            {
                return Ok(new
                {
                    success = true,
                    alreadyReacted = true,
                    count = comment.SoCamXuc,
                    message = "Bạn đã thả tim bình luận này rồi."
                });
            }

            _context.BinhLuanCamXucs.Add(new BinhLuanCamXuc
            {
                MaBinhLuan = maBinhLuan,
                MaNguoiDung = userId
            });
            comment.SoCamXuc += 1;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, alreadyReacted = false, count = comment.SoCamXuc });
        }

        [HttpPost("BaoCaoBinhLuan")]
        public async Task<IActionResult> BaoCaoBinhLuan([FromForm] int maBinhLuan)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var comment = await _context.BinhLuans.FirstOrDefaultAsync(c => c.MaBinhLuan == maBinhLuan);
            if (comment == null) return NotFound(new { success = false });

            comment.SoBaoCao += 1;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, count = comment.SoBaoCao });
        }

        [HttpPost("GhimBinhLuan")]
        public async Task<IActionResult> GhimBinhLuan([FromForm] int maBinhLuan)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var comment = await _context.BinhLuans
                .Include(c => c.Truyen)
                .FirstOrDefaultAsync(c => c.MaBinhLuan == maBinhLuan);

            if (comment == null) return NotFound(new { success = false });

            var canPin = User.IsInRole("Admin") || comment.Truyen.NguoiDangId == userId;
            if (!canPin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Không có quyền ghim bình luận." });
            }

            comment.DaGhim = !comment.DaGhim;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, pinned = comment.DaGhim });
        }

        [HttpPost("XoaBinhLuan")]
        public async Task<IActionResult> XoaBinhLuan([FromForm] int maBinhLuan)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            // Tìm bình luận cần xóa, kèm theo các bình luận trả lời (con) của nó
            var comment = await _context.BinhLuans
                .Include(c => c.BinhLuanCons)
                .FirstOrDefaultAsync(c => c.MaBinhLuan == maBinhLuan);

            if (comment == null)
                return NotFound(new { success = false, message = "Không tìm thấy bình luận" });

            var isAdmin = User.IsInRole("Admin");

            // Kiểm tra quyền: Chỉ cho phép xóa nếu là người tạo ra bình luận đó HOẶC là Admin
            if (comment.MaNguoiDung != userId && !isAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Không có quyền xóa" });
            }

            // Xóa thủ công các bình luận trả lời (con) trước để tránh lỗi FK Constraint
            if (comment.BinhLuanCons != null && comment.BinhLuanCons.Any())
            {
                _context.BinhLuans.RemoveRange(comment.BinhLuanCons);
            }

            // Cuối cùng xóa bình luận cha
            _context.BinhLuans.Remove(comment);

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        [HttpPost("XoaLichSu")]
        public async Task<IActionResult> XoaLichSu([FromForm] int maLichSu)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var ls = await _context.LichSuDocs.FirstOrDefaultAsync(x => x.MaLichSu == maLichSu && x.MaNguoiDung == userId);
            if (ls != null)
            {
                _context.LichSuDocs.Remove(ls);
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            return NotFound();
        }

        [HttpPost("LuuViTriDoc")]
        public async Task<IActionResult> LuuViTriDoc([FromForm] int maChuong, [FromForm] string viTriDoc)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();
            if (string.IsNullOrWhiteSpace(viTriDoc)) return BadRequest(new { success = false });

            viTriDoc = viTriDoc.Length > 200 ? viTriDoc[..200] : viTriDoc;

            var lichSu = await _context.LichSuDocs
                .FirstOrDefaultAsync(x => x.MaNguoiDung == userId && x.MaChuong == maChuong);

            if (lichSu == null)
            {
                return NotFound(new { success = false });
            }

            lichSu.ViTriDoc = viTriDoc;
            lichSu.ThoiGianDoc = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("LuuCaiDatDoc")]
        public async Task<IActionResult> LuuCaiDatDoc(
            [FromForm] string bg,
            [FromForm] string text,
            [FromForm] string font,
            [FromForm] int fontSize,
            [FromForm] double lineHeight,
            [FromForm] int paragraphGap)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            fontSize = Math.Clamp(fontSize, 16, 36);
            lineHeight = Math.Clamp(lineHeight, 1.2, 2.6);
            paragraphGap = Math.Clamp(paragraphGap, 8, 40);

            user.CaiDatMauNen = JsonSerializer.Serialize(new
            {
                bg,
                text,
                lineHeight,
                paragraphGap
            });
            user.CaiDatFontChu = string.IsNullOrWhiteSpace(font)
                ? "'Palatino Linotype', 'Book Antiqua', Palatino, serif"
                : font;
            user.CaiDatCoChu = fontSize;

            await _userManager.UpdateAsync(user);
            return Ok(new { success = true });
        }
    }
}
