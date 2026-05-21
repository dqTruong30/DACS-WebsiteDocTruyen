using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class DayTruyenController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DayTruyenController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> ThucHienDay(int maTruyen, int soLuongVe)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        if (soLuongVe <= 0) return BadRequest("Số vé phải lớn hơn 0.");
        if (user.VeDaySach < soLuongVe) return BadRequest("Bạn không đủ vé!");

        var truyen = await _context.Truyens.FindAsync(maTruyen);
        if (truyen == null) return NotFound();

        // 1. Trừ vé user
        user.VeDaySach -= soLuongVe;
        // Cần cập nhật user
        await _userManager.UpdateAsync(user);

        // 2. Tạo bản ghi lịch sử đẩy
        for (int i = 0; i < soLuongVe; i++)
        {
            _context.DayTruyens.Add(new DayTruyen { MaTruyen = maTruyen, MaNguoiDung = user.Id });
        }

        // 3. LƯU THAY ĐỔI VÀO DATABASE (Dòng này bạn đang thiếu)
        await _context.SaveChangesAsync();

        return RedirectToAction("ChiTiet", "Truyen", new { id = maTruyen });
    }
}
