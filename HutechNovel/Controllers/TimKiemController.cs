using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Controllers
{
    public class TimKiemController : Controller
    {
        private const int PageSize = 20;
        private const string SearchCollation = "SQL_Latin1_General_CP1_CI_AI";
        private readonly ApplicationDbContext _context;

        // Danh sách thể loại mặc định của hệ thống
        private static readonly string[] DefaultTags =
        {
            "Huyền huyễn", "Đồng nhân", "Dị năng", "Đô thị", "Linh dị",
            "Ngôn tình", "Light Novel", "Võng du", "Khoa học viễn tưởng", "Lịch sử"
        };

        public TimKiemController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Suggestions(string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(Array.Empty<object>());
            }

            var text = term.Trim();
            var stories = await _context.Truyens
                .AsNoTracking()
                .Where(t =>
                    EF.Functions.Collate(t.TieuDe, SearchCollation).Contains(text) ||
                    EF.Functions.Collate(t.TacGia.TenTacGia, SearchCollation).Contains(text))
                .OrderByDescending(t => EF.Functions.Collate(t.TieuDe, SearchCollation).StartsWith(text))
                .ThenBy(t => t.TieuDe)
                .Take(6)
                .Select(t => new
                {
                    id = t.MaTruyen,
                    title = t.TieuDe,
                    author = t.TacGia.TenTacGia,
                    cover = t.AnhBia
                })
                .ToListAsync();

            var suggestions = stories.Select(t => new
            {
                t.id,
                t.title,
                t.author,
                t.cover,
                url = Url.Action("ChiTiet", "Truyen", new { id = t.id })
            });

            return Json(suggestions);
        }

        public async Task<IActionResult> Index(
    string? keyword,
    string? author,
    string? summary,
    string? chapterTitle,
    int? status,
    string sortBy = "updated",
    int? selectedTagId = null,
    [FromQuery] List<int>? selectedCustomTagIds = null, // THÊM tham số này
    int? minViews = null,
    int? minChapters = null,
    int page = 1,
    bool infinite = false,
    bool partial = false)
        {
            page = Math.Max(1, page);
            var startOfToday = DateTime.Today;
            var startOfWeek = startOfToday.AddDays(-6);

            var query = _context.Truyens.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var text = keyword.Trim();
                query = query.Where(t =>
                    EF.Functions.Collate(t.TieuDe, SearchCollation).Contains(text) ||
                    EF.Functions.Collate(t.TacGia.TenTacGia, SearchCollation).Contains(text));
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                var authorText = author.Trim();
                query = query.Where(t => EF.Functions.Collate(t.TacGia.TenTacGia, SearchCollation).Contains(authorText));
            }

            if (!string.IsNullOrWhiteSpace(summary))
            {
                var summaryText = summary.Trim();
                query = query.Where(t => EF.Functions.Collate(t.MoTa, SearchCollation).Contains(summaryText));
            }

            if (!string.IsNullOrWhiteSpace(chapterTitle))
            {
                var chapterText = chapterTitle.Trim();
                query = query.Where(t => t.Chuongs.Any(c => EF.Functions.Collate(c.TieuDe, SearchCollation).Contains(chapterText)));
            }

            if (status.HasValue)
            {
                query = query.Where(t => (int)t.TrangThai == status.Value);
            }

            // 1. Lọc theo Thể loại chính (Dropdown)
            if (selectedTagId.HasValue)
            {
                query = query.Where(t => t.Thes.Any(tag => tag.MaThe == selectedTagId.Value));
            }

            // 2. Lọc theo nhiều Nhãn dán (Nút bấm)
            // Dùng logic AND: Truyện phải chứa TẤT CẢ các nhãn dán đã chọn
            if (selectedCustomTagIds != null && selectedCustomTagIds.Any())
            {
                foreach (var tagId in selectedCustomTagIds)
                {
                    query = query.Where(t => t.Thes.Any(tag => tag.MaThe == tagId));
                }
            }

            if (minViews.HasValue)
            {
                query = query.Where(t => t.TongLuotXem >= minViews.Value);
            }

            if (minChapters.HasValue)
            {
                query = query.Where(t => t.TongSoChuong >= minChapters.Value);
            }

            query = sortBy switch
            {
                "views" => query.OrderByDescending(t => t.TongLuotXem),
                "views-day" => query.OrderByDescending(t => t.LuotXems.Count(v => v.ThoiGianXem >= startOfToday)),
                "views-week" => query.OrderByDescending(t => t.LuotXems.Count(v => v.ThoiGianXem >= startOfWeek)),
                "likes" => query.OrderByDescending(t => t.YeuThichs.Count),
                "follows" => query.OrderByDescending(t => t.TheoDoiTruyens.Count),
                "bookmarks" => query.OrderByDescending(t => t.DanhDaus.Count),
                _ => query.OrderByDescending(t => t.NgayCapNhat)
            };

            var totalItems = await query.CountAsync();
            var stories = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(t => new SearchStoryItemViewModel
                {
                    MaTruyen = t.MaTruyen,
                    TieuDe = t.TieuDe,
                    MoTa = t.MoTa,
                    AnhBia = t.AnhBia,
                    TrangThai = t.TrangThai,
                    NgayCapNhat = t.NgayCapNhat,
                    TenTacGia = t.TacGia.TenTacGia,
                    MaTacGia = t.TacGia.MaTacGia,
                    TongLuotXem = t.TongLuotXem,
                    LuotXemNgay = t.LuotXems.Count(v => v.ThoiGianXem >= startOfToday),
                    LuotXemTuan = t.LuotXems.Count(v => v.ThoiGianXem >= startOfWeek),
                    TongSoChuong = t.TongSoChuong,
                    LuotThich = t.YeuThichs.Count,
                    LuotTheoDoi = t.TheoDoiTruyens.Count,
                    LuotDanhDau = t.DanhDaus.Count,
                    Tags = t.Thes
                        .Where(tag => DefaultTags.Contains(tag.TenThe))
                        .OrderBy(tag => tag.TenThe)
                        .Select(tag => tag.TenThe)
                        .ToList()
                })
                .ToListAsync();

            var customTags = await _context.Thes
                .AsNoTracking()
                .Where(t => !DefaultTags.Contains(t.TenThe))
                .Select(t => new CustomTagCount
                {
                    MaThe = t.MaThe,
                    TenThe = t.TenThe,
                    SoTruyen = t.Truyens.Count()
                })
                .Where(t => t.SoTruyen > 0)
                .OrderByDescending(t => t.SoTruyen)
                .ToListAsync();

            var viewModel = new SearchViewModel
            {
                Keyword = keyword,
                Author = author,
                Summary = summary,
                ChapterTitle = chapterTitle,
                Status = status,
                SortBy = sortBy,
                SelectedTagId = selectedTagId,
                SelectedCustomTagIds = selectedCustomTagIds ?? new List<int>(), // Nhớ truyền lại mảng này cho View
                MinViews = minViews,
                MinChapters = minChapters,
                Page = page,
                PageSize = PageSize,
                TotalItems = totalItems,
                Infinite = infinite,
                Tags = await _context.Thes
                    .AsNoTracking()
                    .Where(t => DefaultTags.Contains(t.TenThe))
                    .OrderBy(t => t.TenThe)
                    .ToListAsync(),
                CustomTags = customTags,
                Results = stories
            };

            if (partial)
            {
                return PartialView("_StoryResults", viewModel);
            }

            return View(viewModel);
        }
    }
}
