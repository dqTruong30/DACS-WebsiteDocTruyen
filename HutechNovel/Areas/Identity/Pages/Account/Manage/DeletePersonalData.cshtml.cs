using System.ComponentModel.DataAnnotations;
using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool RequirePassword { get; set; }

        public class InputModel
        {
            [DataType(DataType.Password)]
            public string? Password { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword && !await _userManager.CheckPasswordAsync(user, Input.Password ?? string.Empty))
            {
                ModelState.AddModelError("Input.Password", "Mat khau khong dung.");
                return Page();
            }

            var userId = user.Id;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await RemoveApplicationDataAsync(userId);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return Page();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                _logger.LogError(ex, "Unable to delete user with ID '{UserId}'.", userId);
                ModelState.AddModelError(string.Empty, "Khong the xoa tai khoan luc nay. Vui long thu lai sau.");
                return Page();
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }

        private async Task RemoveApplicationDataAsync(string userId)
        {
            var hasUploadedStories = await _context.Truyens.AnyAsync(t => t.NguoiDangId == userId);
            if (hasUploadedStories)
            {
                var replacementUploaderId = await GetReplacementUploaderIdAsync(userId);
                if (string.IsNullOrWhiteSpace(replacementUploaderId))
                {
                    throw new InvalidOperationException("Khong the xoa tai khoan nay vi chua co tai khoan khac de tiep quan truyen da dang.");
                }

                await _context.Truyens
                    .Where(t => t.NguoiDangId == userId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.NguoiDangId, replacementUploaderId));
            }

            await _context.LuotXems
                .Where(x => x.MaNguoiDung == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.MaNguoiDung, (string?)null));

            var userCommentIds = await _context.BinhLuans
                .Where(x => x.MaNguoiDung == userId)
                .Select(x => x.MaBinhLuan)
                .ToListAsync();

            if (userCommentIds.Count > 0)
            {
                await _context.BinhLuans
                    .Where(x => x.MaBinhLuanCha.HasValue && userCommentIds.Contains(x.MaBinhLuanCha.Value))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.MaBinhLuanCha, (int?)null));

                await _context.BinhLuans
                    .Where(x => x.MaNguoiDung == userId)
                    .ExecuteDeleteAsync();
            }

            await _context.DanhDaus.Where(x => x.MaNguoiDung == userId).ExecuteDeleteAsync();
            await _context.YeuThichs.Where(x => x.MaNguoiDung == userId).ExecuteDeleteAsync();
            await _context.LichSuDocs.Where(x => x.MaNguoiDung == userId).ExecuteDeleteAsync();
            await _context.DanhGias.Where(x => x.MaNguoiDung == userId).ExecuteDeleteAsync();
            await _context.DayTruyens.Where(x => x.MaNguoiDung == userId).ExecuteDeleteAsync();
            await _context.TheoDoiTruyens.Where(x => x.MaNguoiDung == userId).ExecuteDeleteAsync();
        }

        private async Task<string?> GetReplacementUploaderIdAsync(string deletingUserId)
        {
            var admin = await _userManager.FindByEmailAsync("admin@gmail.com");
            if (admin != null && admin.Id != deletingUserId)
            {
                return admin.Id;
            }

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id != deletingUserId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }
    }
}
