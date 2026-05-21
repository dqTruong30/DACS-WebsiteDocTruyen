using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTacGiaController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;
        public AdminTacGiaController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            // Tự động dọn dẹp: Xóa tất cả tác giả đang có 0 cuốn truyện trong DB
            var tacGiaRong = await _context.TacGias.Where(tg => !tg.Truyens.Any()).ToListAsync();
            if (tacGiaRong.Any())
            {
                _context.TacGias.RemoveRange(tacGiaRong);
                await _context.SaveChangesAsync();
            }

            // Lấy danh sách tác giả (kèm theo danh sách truyện của họ để đếm)
            var dsTacGia = await _context.TacGias
                .Include(tg => tg.Truyens)
                .OrderByDescending(tg => tg.Truyens.Count) // Xếp ai nhiều truyện lên đầu
                .ToListAsync();

            return View(dsTacGia);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(TacGia tacGia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tacGia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tacGia);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tacGia = await _context.TacGias.FindAsync(id);
            return tacGia == null ? NotFound() : View(tacGia);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TacGia tacGia)
        {
            if (ModelState.IsValid)
            {
                _context.Update(tacGia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tacGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tacGia = await _context.TacGias.FindAsync(id);
            if (tacGia != null)
            {
                _context.TacGias.Remove(tacGia);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Đã xóa tác giả {tacGia.TenTacGia} thành công!");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}