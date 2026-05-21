using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuanTriTheController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public QuanTriTheController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Thes.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Them(string tenThe)
        {
            if (!string.IsNullOrEmpty(tenThe))
            {
                _context.Thes.Add(new The { TenThe = tenThe });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            var the = await _context.Thes.FindAsync(id);
            if (the != null)
            {
                _context.Thes.Remove(the);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}