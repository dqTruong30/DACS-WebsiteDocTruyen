using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HutechNovel.Controllers
{
    public class TruyenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TruyenController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? maThe, string? timKiem, int page = 1)
        {
            const int pageSize = 20;

            var query = _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.Thes)
                .AsQueryable();

            if (maThe.HasValue)
            {
                query = query.Where(t => t.Thes.Any(tag => tag.MaThe == maThe.Value));
            }

            if (!string.IsNullOrWhiteSpace(timKiem))
            {
                query = query.Where(t => t.TieuDe.Contains(timKiem));
            }

            var stories = await query
                .OrderByDescending(t => t.NgayCapNhat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(stories);
        }

        public async Task<IActionResult> ChiTiet(int id, string commentSort = "highlight")
        {
            var truyen = await _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.Thes)
                .Include(t => t.Chuongs.Where(c => c.TrangThai == TrangThaiChuong.DaXuatBan).OrderBy(c => c.SoChuong))
                .Include(t => t.BinhLuans.OrderByDescending(b => b.NgayTao))
                    .ThenInclude(b => b.NguoiDung)
                .Include(t => t.DanhGias)
                .Include(t => t.YeuThichs)
                .Include(t => t.DanhDaus)
                .Include(t => t.TheoDoiTruyens)
                .Include(t => t.DayTruyens)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.MaTruyen == id);

            if (truyen == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var readChapterIds = new HashSet<int>();
            if (userId != null)
            {
                // 1. Dùng ToListAsync() để kéo dữ liệu từ DB về
                var listIds = await _context.LichSuDocs
                    .Where(ls => ls.MaNguoiDung == userId && ls.Chuong.MaTruyen == id)
                    .Select(ls => ls.MaChuong)
                    .ToListAsync();

                // 2. Ép kiểu sang HashSet trên RAM
                readChapterIds = listIds.ToHashSet();
            }
            commentSort = commentSort == "latest" ? "latest" : "highlight";
            var sortedComments = SortComments(truyen.BinhLuans.ToList(), commentSort);
            var reactedCommentIds = new HashSet<int>();
            if (userId != null && sortedComments.Any())
            {
                var commentIds = sortedComments.Select(c => c.MaBinhLuan).ToList();
                reactedCommentIds = (await _context.BinhLuanCamXucs
                    .Where(x => x.MaNguoiDung == userId && commentIds.Contains(x.MaBinhLuan))
                    .Select(x => x.MaBinhLuan)
                    .ToListAsync())
                    .ToHashSet();
            }

            var viewModel = new StoryDetailViewModel
            {
                Truyen = truyen,
                Chapters = truyen.Chuongs.OrderBy(c => c.SoChuong).ToList(),
                Comments = sortedComments,
                CommentSort = commentSort,
                AverageRating = truyen.DanhGias.Any() ? truyen.DanhGias.Average(d => d.DiemSo) : truyen.DiemDanhGiaTrungBinh,
                TotalRatings = truyen.DanhGias.Count,
                TotalLikes = truyen.YeuThichs.Count,
                TotalFollowers = truyen.TheoDoiTruyens.Count,
                TotalBookmarks = truyen.DanhDaus.Count,
                TotalBoosts = truyen.DayTruyens.Count,
                CurrentUserBoostTickets = userId != null
                    ? (await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.VeDaySach)
                        .FirstOrDefaultAsync())
                    : 0,
                IsLiked = userId != null && truyen.YeuThichs.Any(x => x.MaNguoiDung == userId),
                IsBookmarked = userId != null && truyen.DanhDaus.Any(x => x.MaNguoiDung == userId),
                IsFollowing = userId != null && truyen.TheoDoiTruyens.Any(x => x.MaNguoiDung == userId),
                UserRating = userId != null ? (truyen.DanhGias.FirstOrDefault(d => d.MaNguoiDung == userId)?.DiemSo ?? 0) : 0,
                ReadChapterIds = readChapterIds,
                ReactedCommentIds = reactedCommentIds,
                SimilarStories = await BuildSimilarStories(truyen, 6)
            };

            return View(viewModel);
        }

        private static List<BinhLuan> SortComments(List<BinhLuan> comments, string sort)
        {
            if (sort == "latest")
            {
                return comments
                    .OrderByDescending(b => b.NgayTao)
                    .ToList();
            }

            var rootOrder = comments
                .Where(b => b.MaBinhLuanCha == null)
                .OrderByDescending(b => b.DaGhim)
                .ThenByDescending(b => b.SoCamXuc + comments.Count(reply => reply.MaBinhLuanCha == b.MaBinhLuan))
                .ThenByDescending(b => b.NgayTao)
                .Select((comment, index) => new { comment.MaBinhLuan, index })
                .ToDictionary(x => x.MaBinhLuan, x => x.index);

            return comments
                .OrderBy(b => b.MaBinhLuanCha.HasValue
                    ? rootOrder.GetValueOrDefault(b.MaBinhLuanCha.Value, int.MaxValue)
                    : rootOrder.GetValueOrDefault(b.MaBinhLuan, int.MaxValue))
                .ThenBy(b => b.MaBinhLuanCha.HasValue ? 1 : 0)
                .ThenByDescending(b => !b.MaBinhLuanCha.HasValue && b.DaGhim)
                .ThenBy(b => b.MaBinhLuanCha.HasValue ? b.NgayTao : DateTime.MinValue)
                .ToList();
        }

        private async Task<List<Truyen>> BuildSimilarStories(Truyen currentStory, int take)
        {
            var tagIds = currentStory.Thes.Select(tag => tag.MaThe).Distinct().ToList();
            if (!tagIds.Any())
            {
                return new List<Truyen>();
            }

            var rankedIds = await _context.Truyens
                .Where(t => t.MaTruyen != currentStory.MaTruyen && t.Thes.Any(tag => tagIds.Contains(tag.MaThe)))
                .Select(t => new
                {
                    t.MaTruyen,
                    Score = t.Thes.Count(tag => tagIds.Contains(tag.MaThe)),
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

        public IActionResult DocTruyen(int maTruyen, int soChuong)
        {
            return RedirectToAction("Index", "DocTruyen", new { maTruyen, soChuong });
        }

        public IActionResult LocTruyen(
            string? keyword,
            string? tomTat,
            int? viewMin,
            int? chuongMin,
            string sortBy = "updated",
            int page = 1)
        {
            return RedirectToAction("Index", "TimKiem", new
            {
                keyword,
                summary = tomTat,
                minViews = viewMin,
                minChapters = chuongMin,
                sortBy,
                page
            });
        }
    }
}
