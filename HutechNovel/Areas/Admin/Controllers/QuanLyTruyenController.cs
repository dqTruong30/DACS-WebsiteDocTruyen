using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    public class QuanLyTruyenController : BaseUploaderController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public QuanLyTruyenController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            int pageSize = 10; // Cố định hiển thị 10 truyện mỗi trang

            // 1. Tính toán các con số Thống kê tổng (Không bị ảnh hưởng bởi ô tìm kiếm)
            var userStoriesBase = _context.Truyens.Where(t => t.NguoiDangId == userId);

            int totalStories = await userStoriesBase.CountAsync();
            int totalChapters = await userStoriesBase.SumAsync(t => t.TongSoChuong);
            int totalViews = await userStoriesBase.SumAsync(t => t.TongLuotXem);
            var userStoryIds = await userStoriesBase.Select(t => t.MaTruyen).ToListAsync();
            var today = DateTime.Today;

            int totalFollowers = await _context.TheoDoiTruyens.CountAsync(t => userStoryIds.Contains(t.MaTruyen));
            int totalBoosts = await _context.DayTruyens.CountAsync(t => userStoryIds.Contains(t.MaTruyen));
            int draftChapters = await _context.Chuongs.CountAsync(c => userStoryIds.Contains(c.MaTruyen) && c.TrangThai == TrangThaiChuong.BanNhap);
            int scheduledChapters = await _context.Chuongs.CountAsync(c => userStoryIds.Contains(c.MaTruyen) && c.TrangThai == TrangThaiChuong.HenGio);
            int todayViews = await _context.LuotXems.CountAsync(v => userStoryIds.Contains(v.MaTruyen) && v.ThoiGianXem >= today);

            // 2. Query dành riêng cho Danh sách truyện có Lọc và Phân trang
            var query = _context.Truyens
                .Include(t => t.TacGia)
                .Where(t => t.NguoiDangId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => t.TieuDe.Contains(keyword));
            }

            int totalFilteredItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalFilteredItems / (double)pageSize);

            // Lấy ra 10 truyện của trang hiện tại
            var storiesToDisplay = await query
                .OrderByDescending(t => t.NgayCapNhat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var topStories = await _context.Truyens
                .Where(t => userStoryIds.Contains(t.MaTruyen))
                .Select(t => new UploaderStoryStatViewModel
                {
                    Story = t,
                    TotalViews = t.TongLuotXem,
                    TodayViews = _context.LuotXems.Count(v => v.MaTruyen == t.MaTruyen && v.ThoiGianXem >= today)
                })
                .OrderByDescending(t => t.TodayViews)
                .ThenByDescending(t => t.TotalViews)
                .Take(5)
                .ToListAsync();

            var topChapters = await _context.LichSuDocs
                .Include(ls => ls.Chuong)
                    .ThenInclude(c => c.Truyen)
                .Where(ls => userStoryIds.Contains(ls.Chuong.MaTruyen))
                .GroupBy(ls => ls.MaChuong)
                .Select(group => new
                {
                    MaChuong = group.Key,
                    ReadCount = group.Count()
                })
                .OrderByDescending(x => x.ReadCount)
                .Take(5)
                .ToListAsync();

            var topChapterIds = topChapters.Select(x => x.MaChuong).ToList();
            var chapterRows = await _context.Chuongs
                .Include(c => c.Truyen)
                .Where(c => topChapterIds.Contains(c.MaChuong))
                .ToListAsync();

            return View(new UploaderDashboardViewModel
            {
                TotalStories = totalStories,
                TotalChapters = totalChapters,
                TotalViews = totalViews,
                TotalFollowers = totalFollowers,
                TotalBoosts = totalBoosts,
                DraftChapters = draftChapters,
                ScheduledChapters = scheduledChapters,
                TodayViews = todayViews,
                RecentStories = storiesToDisplay,
                TopStories = topStories,
                TopChapters = topChapters
                    .Select(item => new UploaderChapterStatViewModel
                    {
                        Chapter = chapterRows.First(c => c.MaChuong == item.MaChuong),
                        ReadCount = item.ReadCount
                    })
                    .ToList(),
                SearchKeyword = keyword,
                CurrentPage = page,
                TotalPages = totalPages
            });
        }
        [HttpGet]
        public IActionResult ThemMoi()
        {
            ViewBag.DanhSachThe = _context.Thes.AsNoTracking().OrderBy(t => t.TenThe).ToList();
            return View(new Truyen());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemMoi(
            Truyen truyen,
            IFormFile? fileAnhBia,
            string? linkAnhBia,
            List<string> selectedTags,
            string? tenTacGiaMoi)
        {
            // Xóa tất cả các lỗi Validation ảo do EF Core tạo ra với Navigation Properties
            CleanModelState();

            // SỬA LỖI: Chủ động đọc dữ liệu Tags trực tiếp từ Form nếu Parameter Binding bị hụt
            if (selectedTags == null || !selectedTags.Any())
            {
                selectedTags = Request.Form["selectedTags"].Select(s => s!).ToList();
            }

            if (ModelState.IsValid)
            {
                await ProcessNovelData(truyen, fileAnhBia, linkAnhBia, selectedTags, tenTacGiaMoi);
                truyen.NguoiDangId = _userManager.GetUserId(User)!;

                _context.Truyens.Add(truyen);
                await _context.SaveChangesAsync();

                SetSuccessMessage("Thêm truyện thành công!");
                return RedirectToAction(nameof(Index));
            }

            SetErrorMessage("Dữ liệu không hợp lệ, vui lòng kiểm tra lại.");
            ViewBag.DanhSachThe = _context.Thes.AsNoTracking().OrderBy(t => t.TenThe).ToList();
            return View(truyen);
        }

        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var userId = _userManager.GetUserId(User);

            var truyen = await _context.Truyens
                .Include(t => t.Thes)
                .Include(t => t.TacGia)
                .FirstOrDefaultAsync(t => t.MaTruyen == id && (t.NguoiDangId == userId || User.IsInRole("Admin")));

            if (truyen == null) return NotFound();

            ViewBag.TenTacGiaCu = truyen.TacGia?.TenTacGia;
            ViewBag.DanhSachThe = _context.Thes.AsNoTracking().OrderBy(t => t.TenThe).ToList();

            // Lấy trực tiếp danh sách ID của các Thể loại đang có
            ViewBag.SelectedTags = truyen.Thes.Select(t => t.MaThe).ToList();

            return View(truyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(
             Truyen truyenUpdate,
             IFormFile? fileAnhBia,
             string? linkAnhBia,
             List<string> selectedTags,
             string? tenTacGiaMoi)
        {
            CleanModelState();

            // SỬA LỖI: Chủ động đọc dữ liệu Tags trực tiếp từ Form
            if (selectedTags == null || !selectedTags.Any())
            {
                selectedTags = Request.Form["selectedTags"].Select(s => s!).ToList();
            }

            var truyen = await _context.Truyens
                .Include(t => t.Thes)
                .FirstOrDefaultAsync(t => t.MaTruyen == truyenUpdate.MaTruyen);

            if (truyen == null) return NotFound();

            if (ModelState.IsValid)
            {
                truyen.Thes.Clear();

                await ProcessNovelData(truyen, fileAnhBia, linkAnhBia, selectedTags, tenTacGiaMoi);

                truyen.TieuDe = truyenUpdate.TieuDe;
                truyen.MoTa = truyenUpdate.MoTa;
                truyen.TrangThai = truyenUpdate.TrangThai;
                truyen.NgayCapNhat = DateTime.Now;

                await _context.SaveChangesAsync();
                SetSuccessMessage("Cập nhật truyện thành công!");
                return RedirectToAction(nameof(Index));
            }

            SetErrorMessage("Dữ liệu không hợp lệ.");
            ViewBag.DanhSachThe = _context.Thes.AsNoTracking().OrderBy(t => t.TenThe).ToList();

            // Ép kiểu các Tag hiện tại đang chọn để hiển thị lại nếu Form lỗi
            ViewBag.SelectedTags = selectedTags.Where(t => int.TryParse(t, out _)).Select(int.Parse).ToList();
            ViewBag.TenTacGiaCu = tenTacGiaMoi;
            return View(truyenUpdate);
        }

        [HttpGet]
        public async Task<IActionResult> SearchTacGia(string keyword)
        {
            var data = await _context.TacGias
                .Where(t => t.TenTacGia.Contains(keyword))
                .Select(t => new { id = t.MaTacGia, text = t.TenTacGia })
                .Take(10)
                .ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> SearchThe(string keyword)
        {
            var data = await _context.Thes
                .Where(t => t.TenThe.Contains(keyword))
                .Select(t => new { id = t.MaThe, text = t.TenThe })
                .Take(10)
                .ToListAsync();
            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Tìm truyện kèm theo thông tin Tác giả và danh sách truyện của tác giả đó
            var truyen = await _context.Truyens
                .Include(t => t.TacGia)
                    .ThenInclude(tg => tg.Truyens)
                .FirstOrDefaultAsync(t => t.MaTruyen == id && (t.NguoiDangId == userId || User.IsInRole("Admin")));

            if (truyen != null)
            {
                var tacGiaCanKiemTra = truyen.TacGia;

                var commentIds = await _context.BinhLuans
                    .Where(b => b.MaTruyen == truyen.MaTruyen)
                    .Select(b => b.MaBinhLuan)
                    .ToListAsync();

                if (commentIds.Any())
                {
                    await _context.BinhLuans
                        .Where(b => b.MaTruyen == truyen.MaTruyen && b.MaBinhLuanCha != null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.MaBinhLuanCha, (int?)null));

                    await _context.BinhLuanCamXucs
                        .Where(x => commentIds.Contains(x.MaBinhLuan))
                        .ExecuteDeleteAsync();

                    await _context.BinhLuans
                        .Where(b => b.MaTruyen == truyen.MaTruyen)
                        .ExecuteDeleteAsync();
                }

                // 1. Xóa truyện
                _context.Truyens.Remove(truyen);
                await _context.SaveChangesAsync();

                // 2. Dọn dẹp rác: Nếu tác giả đó không còn truyện nào thì xóa luôn tác giả
                if (tacGiaCanKiemTra != null)
                {
                    var soTruyenConLai = await _context.Truyens.CountAsync(t => t.MaTacGia == tacGiaCanKiemTra.MaTacGia);
                    if (soTruyenConLai == 0)
                    {
                        _context.TacGias.Remove(tacGiaCanKiemTra);
                        await _context.SaveChangesAsync();
                    }
                }

                SetSuccessMessage("Đã xóa truyện thành công!");
            }
            else
            {
                SetErrorMessage("Không tìm thấy truyện hoặc bạn không có quyền xóa!");
            }

            return RedirectToAction(nameof(Index));
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        private async Task ProcessNovelData(
            Truyen truyen,
            IFormFile? file,
            string? link,
            List<string> tags,
            string? authorName)
        {
            // 1. XỬ LÝ ẢNH BÌA
            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string path = Path.Combine(_env.WebRootPath, "uploads/covers", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
                truyen.AnhBia = "/uploads/covers/" + fileName;
            }
            else if (!string.IsNullOrEmpty(link))
            {
                truyen.AnhBia = link;
            }
            else if (string.IsNullOrEmpty(truyen.AnhBia))
            {
                truyen.AnhBia = "/images/no-cover.jpg";
            }

            // 2. XỬ LÝ TÁC GIẢ BẰNG EF CORE NAVIGATION
            if (!string.IsNullOrWhiteSpace(authorName))
            {
                authorName = authorName.Trim();
                var author = await _context.TacGias
                    .FirstOrDefaultAsync(a => a.TenTacGia.ToLower() == authorName.ToLower());

                if (author != null)
                {
                    truyen.TacGia = author;
                }
                else
                {
                    truyen.TacGia = new TacGia { TenTacGia = authorName };
                }
            }

            // 3. XỬ LÝ THỂ LOẠI / TAGS
            if (tags != null && tags.Any())
            {
                foreach (var tagItem in tags.Distinct())
                {
                    if (int.TryParse(tagItem, out int tagId))
                    {
                        var existingTag = await _context.Thes.FindAsync(tagId);
                        if (existingTag != null)
                        {
                            truyen.Thes.Add(existingTag);
                        }
                    }
                    else
                    {
                        string tagName = tagItem.StartsWith("new_", StringComparison.OrdinalIgnoreCase)
                            ? tagItem.Substring(4).Trim()
                            : tagItem.Trim();

                        if (string.IsNullOrWhiteSpace(tagName)) continue;

                        var existingTextTag = await _context.Thes
                            .FirstOrDefaultAsync(t => t.TenThe.ToLower() == tagName.ToLower());

                        if (existingTextTag != null)
                        {
                            truyen.Thes.Add(existingTextTag);
                        }
                        else
                        {
                            var newTag = new The { TenThe = tagName };
                            _context.Thes.Add(newTag);

                            // Phải gọi SaveChanges để bảng Thes tự sinh ra ID mới trước khi nối với Truyện
                            await _context.SaveChangesAsync();

                            truyen.Thes.Add(newTag);
                        }
                    }
                }
            }
        }

        private void CleanModelState()
        {
            // SỬA LỖI: Tránh việc EF Core tự động bẫy lỗi các Navigation Properties khiến ModelState bị False ngầm.
            // Chỉ giữ lại việc xác thực những dữ liệu nhập tay bắt buộc cơ bản nhất (TieuDe, TrangThai).
            var keysToKeep = new[] { "TieuDe", "TrangThai" };
            var errorKeys = ModelState.Keys.Where(k => !keysToKeep.Contains(k)).ToList();

            foreach (var key in errorKeys)
            {
                ModelState.Remove(key);
            }
        }
    }
}
