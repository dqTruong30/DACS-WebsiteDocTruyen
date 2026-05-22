using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Identity;

namespace HutechNovel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Bổ sung thêm UserManager vào Constructor
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var config = await _context.CauHinhHeThongs.FirstOrDefaultAsync();
            var user = await _userManager.GetUserAsync(User);

            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-7);

            var lichSuDoc = new List<LichSuDoc>();
            if (user != null)
            {
                lichSuDoc = (await _context.LichSuDocs.Include(ls => ls.Chuong).ThenInclude(c => c.Truyen).Where(ls => ls.MaNguoiDung == user.Id).OrderByDescending(ls => ls.ThoiGianDoc).ToListAsync()).GroupBy(ls => ls.Chuong.MaTruyen).Select(g => g.First()).ToList();
            }
            var smartRecommendations = user != null
                ? await BuildSmartRecommendations(user.Id, 8)
                : new List<Truyen>();

            var truyenHotData = await _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.Thes)
                .Include(t => t.YeuThichs)
                .Include(t => t.TheoDoiTruyens)
                .OrderByDescending(t => t.DiemTrending)
                .Take(8)
                .Select(t => new TruyenHotViewModel
                {
                    Truyen = t,
                    LuotXemNgay = _context.LuotXems.Count(l => l.MaTruyen == t.MaTruyen && l.ThoiGianXem >= today),
                    LuotXemTuan = _context.LuotXems.Count(l => l.MaTruyen == t.MaTruyen && l.ThoiGianXem >= sevenDaysAgo)
                }).ToListAsync();

            // THÊM: Truy vấn Top 4 Truyện xem nhiều nhất trong ngày (Có ít nhất 1 lượt xem)
            var topTruyenNgay = await _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.Thes)
                .Include(t => t.YeuThichs)
                .Include(t => t.TheoDoiTruyens)
                .Select(t => new TruyenHotViewModel
                {
                    Truyen = t,
                    LuotXemNgay = _context.LuotXems.Count(l => l.MaTruyen == t.MaTruyen && l.ThoiGianXem >= today)
                })
                .Where(t => t.LuotXemNgay > 0)
                .OrderByDescending(t => t.LuotXemNgay)
                .Take(4).ToListAsync();

            // THÊM: Truy vấn Top 4 Truyện xem nhiều nhất trong tuần (Có ít nhất 1 lượt xem)
            var topTruyenTuan = await _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.Thes)
                .Include(t => t.YeuThichs)
                .Include(t => t.TheoDoiTruyens)
                .Select(t => new TruyenHotViewModel
                {
                    Truyen = t,
                    LuotXemTuan = _context.LuotXems.Count(l => l.MaTruyen == t.MaTruyen && l.ThoiGianXem >= sevenDaysAgo)
                })
                .Where(t => t.LuotXemTuan > 0)
                .OrderByDescending(t => t.LuotXemTuan)
                .Take(4).ToListAsync();

            var vm = new HomeViewModel
            {
                BannerUrl = config?.BannerUrl,
                LichSuDocs = lichSuDoc,
                TruyenHot = truyenHotData,
                TopTruyenNgay = topTruyenNgay, // TRUYỀN DỮ LIỆU RA VIEW
                TopTruyenTuan = topTruyenTuan, // TRUYỀN DỮ LIỆU RA VIEW
                SmartRecommendations = smartRecommendations,

                TruyenMoiCapNhat = await _context.Truyens
                    .Include(t => t.TacGia)
                    .OrderByDescending(t => t.NgayCapNhat)
                    .Take(10).ToListAsync(),

                TrendingStories = await _context.Truyens
                    .Include(t => t.TacGia)
                    .OrderByDescending(t => t.DiemTrending)
                    .ThenByDescending(t => t.NgayCapNhat)
                    .Take(5).ToListAsync()
            };

            return View(vm);
        }

        private async Task<List<Truyen>> BuildSmartRecommendations(string userId, int take)
        {
            var readStoryIds = await _context.LichSuDocs
                .Where(x => x.MaNguoiDung == userId)
                .Select(x => x.Chuong.MaTruyen)
                .ToListAsync();

            var likedStoryIds = await _context.YeuThichs
                .Where(x => x.MaNguoiDung == userId)
                .Select(x => x.MaTruyen)
                .ToListAsync();

            var followedStoryIds = await _context.TheoDoiTruyens
                .Where(x => x.MaNguoiDung == userId)
                .Select(x => x.MaTruyen)
                .ToListAsync();

            var bookmarkedStoryIds = await _context.DanhDaus
                .Where(x => x.MaNguoiDung == userId)
                .Select(x => x.MaTruyen)
                .ToListAsync();

            var knownStoryIds = readStoryIds
                .Concat(likedStoryIds)
                .Concat(followedStoryIds)
                .Concat(bookmarkedStoryIds)
                .Distinct()
                .ToList();

            var preferredTagIds = knownStoryIds.Any()
                ? await _context.Truyens
                    .Where(t => knownStoryIds.Contains(t.MaTruyen))
                    .SelectMany(t => t.Thes.Select(tag => tag.MaThe))
                    .Distinct()
                    .ToListAsync()
                : new List<int>();

            if (!preferredTagIds.Any())
            {
                return await _context.Truyens
                    .Include(t => t.TacGia)
                    .Include(t => t.Thes)
                    .OrderByDescending(t => t.DiemTrending)
                    .ThenByDescending(t => t.NgayCapNhat)
                    .Take(take)
                    .ToListAsync();
            }

            var rankedIds = await _context.Truyens
                .Where(t => !knownStoryIds.Contains(t.MaTruyen) && t.Thes.Any(tag => preferredTagIds.Contains(tag.MaThe)))
                .Select(t => new
                {
                    t.MaTruyen,
                    Score = t.Thes.Count(tag => preferredTagIds.Contains(tag.MaThe)),
                    t.TongLuotXem,
                    t.NgayCapNhat
                })
                .OrderByDescending(t => t.Score)
                .ThenByDescending(t => t.TongLuotXem)
                .ThenByDescending(t => t.NgayCapNhat)
                .Take(take)
                .ToListAsync();

            var ids = rankedIds.Select(x => x.MaTruyen).ToList();
            var stories = await _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.Thes)
                .Where(t => ids.Contains(t.MaTruyen))
                .ToListAsync();

            return stories
                .OrderBy(t => ids.IndexOf(t.MaTruyen))
                .ToList();
        }
    }
}
