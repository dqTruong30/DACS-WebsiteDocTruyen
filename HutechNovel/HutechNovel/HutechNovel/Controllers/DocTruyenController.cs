using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Controllers
{
    public class DocTruyenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocTruyenController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("DocTruyen/{maTruyen:int}/{soChuong:int}")]
        public async Task<IActionResult> Index(int maTruyen, int soChuong)
        {
            var chuong = await _context.Chuongs
                .Include(c => c.Truyen)
                .Include(c => c.NoiDungChuongs)
                .FirstOrDefaultAsync(c => c.MaTruyen == maTruyen && c.SoChuong == soChuong && c.TrangThai == TrangThaiChuong.DaXuatBan);

            if (chuong == null)
            {
                return NotFound();
            }

            var chapters = await _context.Chuongs
                .Where(c => c.MaTruyen == maTruyen && c.TrangThai == TrangThaiChuong.DaXuatBan)
                .OrderBy(c => c.SoChuong)
                .ToListAsync();

            var user = await _userManager.GetUserAsync(User);
            var savedReadingPosition = "0";
            var readerThemeJson = string.Empty;
            var readerFontFamily = "'Palatino Linotype', 'Book Antiqua', Palatino, serif";
            var readerFontSize = 22;

            if (user != null)
            {
                readerThemeJson = user.CaiDatMauNen?.TrimStart().StartsWith("{") == true ? user.CaiDatMauNen : string.Empty;
                readerFontFamily = string.IsNullOrWhiteSpace(user.CaiDatFontChu) ? readerFontFamily : user.CaiDatFontChu;
                readerFontSize = user.CaiDatCoChu > 0 ? user.CaiDatCoChu : readerFontSize;
            }

            // =========================================================
            // LƯỢT XEM LOGIC: Mỗi tài khoản/IP chỉ tính 1 lượt xem duy nhất cho 1 truyện mãi mãi
            // =========================================================
            bool shouldCountView = false;
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (user != null)
            {
                // Nếu đã đăng nhập: Check xem đã từng có record trong bảng LuotXems chưa
                var hasViewed = await _context.LuotXems
                    .AnyAsync(l => l.MaTruyen == maTruyen
                                && l.MaNguoiDung == user.Id);

                if (!hasViewed)
                {
                    shouldCountView = true;
                }
            }
            else if (!string.IsNullOrEmpty(ipAddress))
            {
                // Khách vãng lai: Check xem IP này đã xem truyện này chưa
                var hasViewed = await _context.LuotXems
                    .AnyAsync(l => l.MaTruyen == maTruyen
                                && l.IpAddress == ipAddress);

                if (!hasViewed)
                {
                    shouldCountView = true;
                }
            }

            // Nếu thỏa mãn điều kiện (chưa từng xem) thì mới tăng View
            if (shouldCountView)
            {
                chuong.Truyen.TongLuotXem += 1;

                _context.LuotXems.Add(new LuotXem
                {
                    MaTruyen = maTruyen,
                    MaNguoiDung = user?.Id,
                    IpAddress = ipAddress,
                    ThoiGianXem = DateTime.Now
                });
            }
            // =========================================================

            if (user != null)
            {
                var lichSu = await _context.LichSuDocs
                    .FirstOrDefaultAsync(ls => ls.MaNguoiDung == user.Id && ls.MaChuong == chuong.MaChuong);

                if (lichSu != null)
                {
                    savedReadingPosition = lichSu.ViTriDoc;
                    lichSu.ThoiGianDoc = DateTime.Now;
                    lichSu.ViTriDoc = savedReadingPosition;
                }
                else
                {
                    _context.LichSuDocs.Add(new LichSuDoc
                    {
                        MaNguoiDung = user.Id,
                        NguoiDung = user,
                        MaChuong = chuong.MaChuong,
                        Chuong = chuong,
                        ThoiGianDoc = DateTime.Now,
                        ViTriDoc = "0"
                    });
                    user.SoChuongDaDoc += 1;
                }

                // Cập nhật số chương đã đọc (có thể bạn muốn thêm logic check để không cộng dồn mãi 1 chương)
            }

            await _context.SaveChangesAsync();

            var viewModel = new ReadingViewModel
            {
                Chapter = chuong,
                Chapters = chapters,
                RawContent = chuong.NoiDungChuongs.FirstOrDefault(nd => nd.LoaiNoiDung == LoaiNoiDungChuong.BanGoc),
                ConvertedContent = chuong.NoiDungChuongs.FirstOrDefault(nd => nd.LoaiNoiDung == LoaiNoiDungChuong.BanDich),
                PreviousChapterNumber = chapters.Where(c => c.SoChuong < soChuong).OrderByDescending(c => c.SoChuong).Select(c => (int?)c.SoChuong).FirstOrDefault(),
                NextChapterNumber = chapters.Where(c => c.SoChuong > soChuong).OrderBy(c => c.SoChuong).Select(c => (int?)c.SoChuong).FirstOrDefault(),
                SavedReadingPosition = savedReadingPosition,
                ReaderThemeJson = readerThemeJson,
                ReaderFontFamily = readerFontFamily,
                ReaderFontSize = readerFontSize
            };

            return View(viewModel);
        }
    }
}
