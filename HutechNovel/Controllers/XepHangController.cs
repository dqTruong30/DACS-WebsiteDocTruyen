using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Controllers
{
    public class XepHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public XepHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string type = "view")
        {
            var query = _context.Truyens
                .Include(t => t.TacGia)
                .Include(t => t.DayTruyens)
                .AsQueryable();

            query = type switch
            {
                "rating" => query.OrderByDescending(t => t.DiemDanhGiaTrungBinh),
                "trending" => query.OrderByDescending(t => t.DiemTrending),
                "boost" => query.OrderByDescending(t => t.DayTruyens.Count),
                _ => query.OrderByDescending(t => t.TongLuotXem)
            };

            var viewModel = new RankingViewModel
            {
                CurrentType = type,
                Stories = await query.Take(20).ToListAsync()
            };

            return View(viewModel);
        }
    }
}