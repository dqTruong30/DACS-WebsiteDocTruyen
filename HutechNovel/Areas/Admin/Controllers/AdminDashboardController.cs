using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6);
            var weekStart = today.AddDays(-6);
            var monthStart = today.AddDays(-29);

            // Lấy dữ liệu lượt xem theo ngày (7 ngày qua)
            var viewsThongKe = await _context.LuotXems
                .Where(l => l.ThoiGianXem >= sevenDaysAgo)
                .GroupBy(l => l.ThoiGianXem.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            var labels = new List<string>();
            var viewsData = new List<int>();
            var usersThongKe = await _context.Users
                .Where(u => u.KhaiSinh >= sevenDaysAgo)
                .GroupBy(u => u.KhaiSinh.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);
            var usersData = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                labels.Add(date.ToString("dd/MM"));
                viewsData.Add(viewsThongKe.ContainsKey(date) ? viewsThongKe[date] : 0);
                usersData.Add(usersThongKe.ContainsKey(date) ? usersThongKe[date] : 0);
            }

            async Task<List<AdminHotStoryViewModel>> HotStories(DateTime from, int take)
            {
                var rows = await _context.LuotXems
                    .Where(v => v.ThoiGianXem >= from)
                    .GroupBy(v => v.MaTruyen)
                    .Select(g => new { MaTruyen = g.Key, Views = g.Count() })
                    .OrderByDescending(x => x.Views)
                    .Take(take)
                    .ToListAsync();

                var ids = rows.Select(x => x.MaTruyen).ToList();
                var stories = await _context.Truyens
                    .Include(t => t.TacGia)
                    .Where(t => ids.Contains(t.MaTruyen))
                    .ToListAsync();

                return rows
                    .Select(row => new AdminHotStoryViewModel
                    {
                        Story = stories.First(t => t.MaTruyen == row.MaTruyen),
                        Views = row.Views
                    })
                    .ToList();
            }

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalStories = await _context.Truyens.CountAsync(),
                TotalViews = await _context.LuotXems.CountAsync(),
                PendingChapters = await _context.Chuongs.CountAsync(c => c.TrangThai == TrangThaiChuong.HenGio || c.TrangThai == TrangThaiChuong.BanNhap),
                ReportedCommentsCount = await _context.BinhLuans.CountAsync(b => b.SoBaoCao > 0),
                NewUsersToday = await _context.Users.CountAsync(u => u.KhaiSinh >= today),
                HotStoriesTodayCount = await _context.LuotXems.Where(v => v.ThoiGianXem >= today).Select(v => v.MaTruyen).Distinct().CountAsync(),
                ChartLabels = labels,
                ChartViewsData = viewsData,
                ChartNewUsersData = usersData,
                NewComments = await _context.BinhLuans
                    .Include(b => b.NguoiDung)
                    .Include(b => b.Truyen)
                    .OrderByDescending(b => b.NgayTao)
                    .Take(6)
                    .ToListAsync(),
                ReportedComments = await _context.BinhLuans
                    .Include(b => b.NguoiDung)
                    .Include(b => b.Truyen)
                    .Where(b => b.SoBaoCao > 0)
                    .OrderByDescending(b => b.SoBaoCao)
                    .ThenByDescending(b => b.NgayTao)
                    .Take(8)
                    .ToListAsync(),
                LatestStories = await _context.Truyens
                    .Include(t => t.TacGia)
                    .OrderByDescending(t => t.NgayTao)
                    .Take(6)
                    .ToListAsync(),
                HotStoriesToday = await HotStories(today, 5),
                HotStoriesWeek = await HotStories(weekStart, 5),
                HotStoriesMonth = await HotStories(monthStart, 5),
                ActionLogs = await _context.NhatKyQuanTris
                    .OrderByDescending(x => x.NgayTao)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}
