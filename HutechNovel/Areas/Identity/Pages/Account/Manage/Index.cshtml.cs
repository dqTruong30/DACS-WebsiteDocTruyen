// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _context = context;
        }

        public string Username { get; set; }

        public string CurrentAvatar { get; set; }

        public int FollowingCount { get; set; }

        public int HistoryCount { get; set; }

        public int CommentCount { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IFormFile AvatarUpload { get; set; }

        public class InputModel
        {
            [Display(Name = "Họ tên")]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Username = await _userManager.GetUserNameAsync(user);
            CurrentAvatar = user.Avatar;
            FollowingCount = await _context.TheoDoiTruyens.CountAsync(x => x.MaNguoiDung == user.Id);
            HistoryCount = await _context.LichSuDocs.CountAsync(x => x.MaNguoiDung == user.Id);
            CommentCount = await _context.BinhLuans.CountAsync(x => x.MaNguoiDung == user.Id);

            Input = new InputModel
            {
                FullName = user.HoTen,
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user)
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Không thể cập nhật số điện thoại.";
                    return RedirectToPage();
                }
            }

            if (user.HoTen != Input.FullName)
            {
                user.HoTen = Input.FullName;
            }

            if (AvatarUpload != null && AvatarUpload.Length > 0)
            {
                var extension = Path.GetExtension(AvatarUpload.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, "Avatar chỉ hỗ trợ JPG, PNG hoặc WEBP.");
                    await LoadAsync(user);
                    return Page();
                }

                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await AvatarUpload.CopyToAsync(stream);

                user.Avatar = $"/uploads/avatars/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Hồ sơ của bạn đã được cập nhật.";
            return RedirectToPage();
        }
    }
}
