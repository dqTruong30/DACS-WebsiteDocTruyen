using HutechNovel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TacGiaController : Controller
{
    private readonly ApplicationDbContext _context;
    public TacGiaController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> ChiTiet(int id)
    {
        var tacGia = await _context.TacGias
            .Include(tg => tg.Truyens)
            .FirstOrDefaultAsync(tg => tg.MaTacGia == id);

        if (tacGia == null) return NotFound();
        return View(tacGia);
    }

    public async Task<IActionResult> Index()
    {
        var dsTacGia = await _context.TacGias.ToListAsync();
        return View(dsTacGia);
    }
}