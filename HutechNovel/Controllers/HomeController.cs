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

        [HttpPost]
        public IActionResult SaveFavoriteKeywords([FromForm] string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords))
            {
                Response.Cookies.Delete("UserFavoriteKeywords");
            }
            else
            {
                var cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(30), Path = "/" };
                Response.Cookies.Append("UserFavoriteKeywords", keywords, cookieOptions);
            }
            return Json(new { success = true });
        }

        private async Task<List<Truyen>> BuildSmartRecommendations(string userId, int take)
        {
            // 1. Lấy từ khóa yêu thích từ Cookie
            var favKeywords = Request.Cookies["UserFavoriteKeywords"];
            var favList = string.IsNullOrEmpty(favKeywords) 
                ? new List<string>() 
                : favKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim().ToLower()).ToList();

            // 2. Lấy thói quen tìm kiếm từ Cookie
            var searchHabits = Request.Cookies["UserSearchHabits"];
            var searchList = string.IsNullOrEmpty(searchHabits) 
                ? new List<string>() 
                : searchHabits.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim().ToLower()).ToList();

            var combinedKeywords = favList.Concat(searchList).Distinct().ToList();

            // 3. Lấy dữ liệu cũ (Lịch sử, Yêu thích, Theo dõi...) để lấy Tags
            List<int> preferredTagIds = new List<int>();
            if (!string.IsNullOrEmpty(userId))
            {
                var readStoryIds = await _context.LichSuDocs.Where(x => x.MaNguoiDung == userId).Select(x => x.Chuong.MaTruyen).ToListAsync();
                var likedStoryIds = await _context.YeuThichs.Where(x => x.MaNguoiDung == userId).Select(x => x.MaTruyen).ToListAsync();
                var followedStoryIds = await _context.TheoDoiTruyens.Where(x => x.MaNguoiDung == userId).Select(x => x.MaTruyen).ToListAsync();
                var bookmarkedStoryIds = await _context.DanhDaus.Where(x => x.MaNguoiDung == userId).Select(x => x.MaTruyen).ToListAsync();

                var knownStoryIds = readStoryIds.Concat(likedStoryIds).Concat(followedStoryIds).Concat(bookmarkedStoryIds).Distinct().ToList();

                if (knownStoryIds.Any())
                {
                    preferredTagIds = await _context.Truyens
                        .Where(t => knownStoryIds.Contains(t.MaTruyen))
                        .SelectMany(t => t.Thes.Select(tag => tag.MaThe))
                        .Distinct()
                        .ToListAsync();
                }
            }

            // Nếu không có thói quen, không có tag, trả về truyện ngẫu nhiên hoặc top trending
            if (!combinedKeywords.Any() && !preferredTagIds.Any())
            {
                var defaultStories = await _context.Truyens
                    .Include(t => t.TacGia)
                    .Include(t => t.Thes)
                    .Where(t => t.TrangThai != TrangThaiTruyen.TamNgung)
                    .OrderByDescending(t => t.DiemTrending)
                    .Take(50)
                    .ToListAsync();
                return ShuffleAndTake(defaultStories, take, userId, combinedKeywords);
            }

            // Tìm truyện phù hợp dựa trên từ khóa (Tên truyện, tác giả, tag) HOẶC preferred tags
            var query = _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.Thes)
                .AsNoTracking();

            var matchingStories = await query.ToListAsync();

            var scoredStories = matchingStories.Select(t => new
            {
                Story = t,
                Score = CalculateScore(t, combinedKeywords, preferredTagIds)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Story.DiemTrending)
            .Take(50)
            .Select(x => x.Story)
            .ToList();

            // Nếu không đủ, bù thêm truyện top
            if (scoredStories.Count < take)
            {
                var excludeIds = scoredStories.Select(s => s.MaTruyen).ToList();
                var fillStories = await _context.Truyens
                    .Include(t => t.TacGia)
                    .Include(t => t.Thes)
                    .Where(t => !excludeIds.Contains(t.MaTruyen))
                    .OrderByDescending(t => t.DiemTrending)
                    .Take(take - scoredStories.Count)
                    .ToListAsync();
                scoredStories.AddRange(fillStories);
            }

            return ShuffleAndTake(scoredStories, take, userId, combinedKeywords);
        }

        private int CalculateScore(Truyen t, List<string> keywords, List<int> preferredTagIds)
        {
            int score = 0;
            // Chấm điểm theo Tag DB (1 điểm mỗi tag)
            score += t.Thes.Count(tag => preferredTagIds.Contains(tag.MaThe));

            var lowerTitle = t.TieuDe?.ToLower() ?? "";
            var lowerAuthor = t.TacGia?.TenTacGia?.ToLower() ?? "";
            
            foreach(var kw in keywords)
            {
                if (kw.StartsWith("author:")) { 
                    if (lowerAuthor.Contains(kw.Substring(7))) score += 50; 
                }
                else if (kw.StartsWith("summary:")) { 
                    if (t.MoTa?.ToLower().Contains(kw.Substring(8)) == true) score += 10; 
                }
                else if (kw.StartsWith("status:")) { 
                    if (int.TryParse(kw.Substring(7), out int st) && (int)t.TrangThai == st) score += 10; 
                }
                else if (kw.StartsWith("views:")) { 
                    if (int.TryParse(kw.Substring(6), out int v) && t.TongLuotXem >= v) score += 10; 
                }
                else if (kw.StartsWith("chapters:")) { 
                    if (int.TryParse(kw.Substring(9), out int c) && t.TongSoChuong >= c) score += 10; 
                }
                else if (kw.StartsWith("tag:")) { 
                    var tagName = kw.Substring(4).ToLower();
                    if (t.Thes.Any(tag => tag.TenThe.ToLower() == tagName)) score += 50; 
                }
                else if (kw.StartsWith("kw:")) { 
                     var term = kw.Substring(3);
                     if (lowerTitle.Contains(term) || lowerAuthor.Contains(term)) score += 50;
                     if (t.Thes.Any(tag => tag.TenThe.ToLower().Contains(term))) score += 30;
                }
                else {
                     // Tương thích ngược với các keyword tự do user nhập tay
                     if (lowerTitle.Contains(kw) || lowerAuthor.Contains(kw)) score += 50;
                     if (t.Thes.Any(tag => tag.TenThe.ToLower().Contains(kw))) score += 30;
                }
            }
            return score;
        }

        private List<Truyen> ShuffleAndTake(List<Truyen> source, int take, string userId, List<string> keywords = null)
        {
            // Seed cố định cho mỗi ngày kết hợp User ID (hoặc IP), CỘNG THÊM TỪ KHÓA
            // Để khi người dùng đổi từ khóa thì list truyện cũng thay đổi ngay lập tức
            int kwHash = keywords != null && keywords.Any() ? string.Join(",", keywords).GetHashCode() : 0;
            int seed = DateTime.Today.DayOfYear ^ (userId?.GetHashCode() ?? 0) ^ kwHash;
            var rng = new Random(seed);
            
            return source.OrderBy(x => rng.Next()).Take(take).ToList();
        }
    }
}
