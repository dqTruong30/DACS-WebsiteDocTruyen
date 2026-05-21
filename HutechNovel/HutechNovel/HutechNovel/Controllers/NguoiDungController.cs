using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Controllers
{
    [Authorize]
    public class NguoiDungController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public NguoiDungController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> HoSo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var viewModel = new UserProfileViewModel
            {
                User = user,
                FollowingCount = await _context.TheoDoiTruyens.CountAsync(y => y.MaNguoiDung == user.Id),
                BookmarkCount = await _context.DanhDaus.CountAsync(d => d.MaNguoiDung == user.Id),
                CommentCount = await _context.BinhLuans.CountAsync(b => b.MaNguoiDung == user.Id),
                ReadHistoryCount = await _context.LichSuDocs.CountAsync(ls => ls.MaNguoiDung == user.Id),
                ReadStoryCount = await _context.LichSuDocs
                    .Where(ls => ls.MaNguoiDung == user.Id)
                    .Select(ls => ls.Chuong.MaTruyen)
                    .Distinct()
                    .CountAsync(),
                RecentReads = (await _context.LichSuDocs.Include(ls => ls.Chuong).ThenInclude(c => c.Truyen).Where(ls => ls.MaNguoiDung == user.Id).OrderByDescending(ls => ls.ThoiGianDoc).ToListAsync()).GroupBy(ls => ls.Chuong.MaTruyen).Select(g => g.First()).Take(6).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TuTruyen()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var viewModel = await BuildUserLibrary(user.Id);
            return View(viewModel);
        }

        public async Task<IActionResult> ThongBao()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var viewModel = await BuildUserLibrary(user.Id);
            return View(viewModel);
        }

        public async Task<IActionResult> DanhSachTheoDoi()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var viewModel = await BuildUserLibrary(user.Id);
            return View(viewModel);
        }

        private async Task<UserLibraryViewModel> BuildUserLibrary(string userId)
        {
            var history = await _context.LichSuDocs
                .Include(ls => ls.Chuong)
                    .ThenInclude(c => c.Truyen)
                .Where(ls => ls.MaNguoiDung == userId)
                .OrderByDescending(ls => ls.ThoiGianDoc)
                .ToListAsync();

            var following = await _context.TheoDoiTruyens
                .Include(td => td.Truyen)
                    .ThenInclude(t => t.Chuongs.Where(c => c.TrangThai == TrangThaiChuong.DaXuatBan))
                .Where(td => td.MaNguoiDung == userId)
                .OrderByDescending(td => td.NgayTao)
                .ToListAsync();

            var bookmarks = await _context.DanhDaus
                .Include(d => d.Truyen)
                    .ThenInclude(t => t.Chuongs)
                .Where(d => d.MaNguoiDung == userId)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            var favorites = await _context.YeuThichs
                .Include(y => y.Truyen)
                    .ThenInclude(t => t.Chuongs.Where(c => c.TrangThai == TrangThaiChuong.DaXuatBan))
                .Where(y => y.MaNguoiDung == userId)
                .OrderByDescending(y => y.NgayTao)
                .ToListAsync();

            var lastReadByStory = history
                .GroupBy(ls => ls.Chuong.MaTruyen)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        ChapterNumber = group.Max(ls => ls.Chuong.SoChuong),
                        LastActivity = group.Max(ls => ls.ThoiGianDoc)
                    });

            var followingItems = following
                .Select(td =>
                {
                    var latestChapter = td.Truyen.Chuongs
                        .OrderByDescending(c => c.SoChuong)
                        .FirstOrDefault();
                    lastReadByStory.TryGetValue(td.MaTruyen, out var lastRead);

                    return new UserLibraryItemViewModel
                    {
                        Story = td.Truyen,
                        CurrentChapter = latestChapter,
                        LastActivity = lastRead?.LastActivity ?? td.NgayTao,
                        LastReadChapterNumber = lastRead?.ChapterNumber,
                        HasNewChapter = latestChapter != null && (
                            lastRead?.ChapterNumber > 0
                                ? latestChapter.SoChuong > lastRead.ChapterNumber
                                : latestChapter.NgayTao > td.NgayTao)
                    };
                })
                .OrderByDescending(item => item.HasNewChapter)
                .ThenByDescending(item => item.CurrentChapter?.NgayTao ?? item.Story.NgayCapNhat)
                .ToList();

            var viewModel = new UserLibraryViewModel
            {
                ReadingHistory = history.GroupBy(ls => ls.Chuong.MaTruyen).Select(g => g.First()).Select(ls => new UserLibraryItemViewModel { Story = ls.Chuong.Truyen, CurrentChapter = ls.Chuong, LastActivity = ls.ThoiGianDoc }).ToList(),
                FollowingStories = followingItems,
                BookmarkedStories = bookmarks.Select(d => new UserLibraryItemViewModel
                {
                    Story = d.Truyen,
                    CurrentChapter = d.Truyen.Chuongs
                        .Where(c => c.TrangThai == TrangThaiChuong.DaXuatBan)
                        .OrderByDescending(c => c.SoChuong)
                        .FirstOrDefault(),
                    LastActivity = d.NgayTao
                }).ToList(),
                FavoriteStories = favorites.Select(y => new UserLibraryItemViewModel
                {
                    Story = y.Truyen,
                    CurrentChapter = y.Truyen.Chuongs.OrderByDescending(c => c.SoChuong).FirstOrDefault(),
                    LastActivity = y.NgayTao
                }).ToList()
            };

            viewModel.NewChapterStories = followingItems
                .Where(item => item.HasNewChapter)
                .ToList();

            var pausedCutoff = DateTime.Now.AddDays(-14);
            viewModel.ReadingNowStories = viewModel.ReadingHistory
                .Where(item => item.Story.TrangThai != TrangThaiTruyen.DaHoanThanh && item.LastActivity >= pausedCutoff)
                .GroupBy(item => item.Story.MaTruyen)
                .Select(group => group.OrderByDescending(item => item.LastActivity).First())
                .OrderByDescending(item => item.LastActivity)
                .ToList();

            viewModel.PausedStories = viewModel.ReadingHistory
                .Where(item => item.Story.TrangThai != TrangThaiTruyen.DaHoanThanh && item.LastActivity < pausedCutoff)
                .GroupBy(item => item.Story.MaTruyen)
                .Select(group => group.OrderByDescending(item => item.LastActivity).First())
                .ToList();

            viewModel.CompletedStories = viewModel.ReadingHistory
                .Where(item => item.Story.TrangThai == TrangThaiTruyen.DaHoanThanh)
                .GroupBy(item => item.Story.MaTruyen)
                .Select(group => group.OrderByDescending(item => item.LastActivity).First())
                .ToList();

            return viewModel;
        }

        [HttpPost]
        public async Task<IActionResult> DiemDanh()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (user.NgayDiemDanhCuoi == null || user.NgayDiemDanhCuoi.Value.Date < DateTime.Now.Date)
            {
                user.VeDaySach += 1;
                user.NgayDiemDanhCuoi = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Điểm danh thành công! Bạn nhận được 1 vé đẩy sách.";
            }
            else
            {
                TempData["Error"] = "Hôm nay bạn đã điểm danh rồi.";
            }

            return RedirectToAction(nameof(HoSo));
        }

        [HttpPost]
        public async Task<IActionResult> LuuCaiDat(string mauNen, string fontChu, int coChu)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            user.CaiDatMauNen = mauNen;
            user.CaiDatFontChu = fontChu;
            user.CaiDatCoChu = coChu;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
