using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HutechNovel.Models;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // TẠO ĐƯỜNG LINK BÍ MẬT: Đổi chữ "hutech-gate" thành bất cứ ký tự nào bạn muốn
        // Ví dụ: [Route("cua-sau-an-toan")] -> Link sẽ là localhost:port/cua-sau-an-toan
        [AllowAnonymous]
        [HttpGet]
        [Route("hutech-gate")]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập và là Admin thì cho vào thẳng Dashboard
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "AdminDashboard", new { area = "Admin" });
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("hutech-gate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Xử lý chống Spam: Cố tình Delay 1 giây để chống tool Brute-force dò mật khẩu
            await Task.Delay(1000);

            // 1. Tìm user trong DB theo Username (Key ID) hoặc Email
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);

            if (user != null)
            {
                // 2. KIỂM TRA QUYỀN LỰC: Chỉ cho phép tài khoản có Role "Admin" mới được qua cửa này
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (isAdmin)
                {
                    // 3. Kiểm tra mật khẩu
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: false, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        // Đăng nhập thành công -> Đẩy vào Dashboard
                        return RedirectToAction("Index", "AdminDashboard", new { area = "Admin" });
                    }
                    if (result.IsLockedOut)
                    {
                        TempData["ErrorMessage"] = "Hệ thống phòng thủ: Tài khoản bị khóa do sai quá nhiều lần.";
                        return View();
                    }
                }
                else
                {
                    // Cố tình ghi log chung chung để hacker không biết đây là cửa Admin
                    TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                    return View();
                }
            }

            // Báo lỗi chung chung
            TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
            return View();
        }

        // Action Đăng xuất dành riêng cho Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}