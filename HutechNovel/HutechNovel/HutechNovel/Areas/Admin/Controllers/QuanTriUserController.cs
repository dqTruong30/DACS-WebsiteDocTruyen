using HutechNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuanTriUserController : BaseAdminController
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public QuanTriUserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderByDescending(u => u.SoChuongDaDoc).ToListAsync();
            var viewModel = new UserAdminViewModel();

            foreach (var user in users)
            {
                await EnsureDefaultUserRoleAsync(user);

                viewModel.Users.Add(new UserAdminItemViewModel
                {
                    User = user,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapQuyenUploader(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                SetErrorMessage("Tài khoản Admin đã có quyền cao nhất.");
                return RedirectToAction(nameof(Index));
            }

            if (user != null && !await _userManager.IsInRoleAsync(user, "Uploader"))
            {
                await EnsureDefaultUserRoleAsync(user);
                await _userManager.AddToRoleAsync(user, "Uploader");
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThuHoiQuyenUploader(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                SetErrorMessage("Không thể thu hồi quyền Uploader của tài khoản Admin.");
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra nếu user tồn tại và thực sự đang có quyền Uploader
            if (user != null && await _userManager.IsInRoleAsync(user, "Uploader"))
            {
                // Gỡ bỏ Role "Uploader"
                var result = await _userManager.RemoveFromRoleAsync(user, "Uploader");

                if (result.Succeeded)
                {
                    SetSuccessMessage($"Đã thu hồi quyền đăng truyện của: {user.UserName}");
                }
                else
                {
                    SetErrorMessage("Có lỗi xảy ra khi thu hồi quyền.");
                }
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                SetErrorMessage("Không thể ban tài khoản Admin.");
                return RedirectToAction(nameof(Index));
            }

            // Không cho phép Admin tự khóa chính mình
            if (user.Id == _userManager.GetUserId(User))
            {
                SetErrorMessage("Lỗi hệ thống: Không thể tự khóa tài khoản của chính mình.");
                return RedirectToAction(nameof(Index));
            }

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
            {
                // Mở khóa: Đặt thời gian kết thúc Lockout về null
                await _userManager.SetLockoutEndDateAsync(user, null);
                SetSuccessMessage($"Đã gỡ lệnh cấm cho User: {user.UserName}");
            }
            else
            {
                // Khóa: Đặt thời gian kết thúc Lockout đến năm 9999 (Khóa vĩnh viễn)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                // Bắt buộc User đăng xuất ngay lập tức (Xóa Security Stamp)
                await _userManager.UpdateSecurityStampAsync(user);
                SetWarningMessage($"Đã kích hoạt giao thức BAN đối với: {user.UserName}");
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task EnsureDefaultUserRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
        }
    }
}
