// Areas/Admin/Controllers/QuanLyChuongController.cs
using HutechNovel.Data;
using HutechNovel.Models;
using HutechNovel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    public class QuanLyChuongController : BaseUploaderController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IGeminiService _geminiService;

        public QuanLyChuongController(ApplicationDbContext context, IWebHostEnvironment env, IGeminiService geminiService)
        {
            _context = context;
            _env = env;
            _geminiService = geminiService;
        }

        public async Task<IActionResult> DanhSach(int maTruyen)
        {
            var story = await _context.Truyens.FirstOrDefaultAsync(t => t.MaTruyen == maTruyen);
            if (story == null) return NotFound();

            var chapters = await _context.Chuongs
                .Where(c => c.MaTruyen == maTruyen)
                .OrderByDescending(c => c.SoChuong)
                .ToListAsync();

            return View(new ChapterManagementViewModel { Story = story, Chapters = chapters });
        }

        [HttpGet]
        public async Task<IActionResult> ThemChuong(int maTruyen)
        {
            ViewBag.MaTruyen = maTruyen;
            ViewBag.SoChuongTiepTheo = (await _context.Chuongs.Where(c => c.MaTruyen == maTruyen && !c.LaPhuChuong).Select(c => (int?)c.SoChuong).MaxAsync() ?? 0) + 1;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemChuong(
            int maTruyen,
            string tieuDe,
            int soChuong,
            bool laPhuChuong,
            string? noiDungRaw,
            string? noiDungText,
            TrangThaiChuong trangThai = TrangThaiChuong.DaXuatBan,
            DateTime? ngayHenGio = null)
        {
            if (string.IsNullOrWhiteSpace(tieuDe))
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề chương không được để trống.");
                ViewBag.MaTruyen = maTruyen;
                ViewBag.SoChuongTiepTheo = soChuong;
                return View(new Chuong { MaTruyen = maTruyen, TieuDe = tieuDe, SoChuong = soChuong, LaPhuChuong = laPhuChuong });
            }

            if (trangThai == TrangThaiChuong.HenGio && !ngayHenGio.HasValue)
            {
                ModelState.AddModelError("NgayHenGio", "Vui lòng chọn thời điểm hẹn giờ đăng.");
                ViewBag.MaTruyen = maTruyen;
                ViewBag.SoChuongTiepTheo = soChuong;
                return View(new Chuong { MaTruyen = maTruyen, TieuDe = tieuDe, TrangThai = trangThai, NgayHenGio = ngayHenGio, SoChuong = soChuong, LaPhuChuong = laPhuChuong });
            }

            if (!laPhuChuong)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Chuongs SET SoChuong = SoChuong + 1 WHERE MaTruyen = {0} AND SoChuong >= {1} AND LaPhuChuong = 0",
                    maTruyen, soChuong);
            }

            var chuong = new Chuong
            {
                MaTruyen = maTruyen,
                SoChuong = soChuong,
                LaPhuChuong = laPhuChuong,
                TieuDe = tieuDe,
                TrangThai = trangThai,
                NgayTao = DateTime.Now,
                NgayHenGio = trangThai == TrangThaiChuong.HenGio ? ngayHenGio : null
            };

            _context.Chuongs.Add(chuong);
            await _context.SaveChangesAsync();

            // 3. Xử lý lưu nội dung Raw và Convert (Bản dịch)
            if (!string.IsNullOrWhiteSpace(noiDungRaw))
            {
                _context.NoiDungChuongs.Add(new NoiDungChuong
                {
                    MaChuong = chuong.MaChuong,
                    NoiDung = noiDungRaw,
                    LoaiNoiDung = LoaiNoiDungChuong.BanGoc
                });
            }

            if (!string.IsNullOrWhiteSpace(noiDungText))
            {
                _context.NoiDungChuongs.Add(new NoiDungChuong
                {
                    MaChuong = chuong.MaChuong,
                    NoiDung = noiDungText,
                    LoaiNoiDung = LoaiNoiDungChuong.BanDich
                });
            }

            // 4. Cập nhật thống kê của truyện
            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen != null)
            {
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == maTruyen && c.TrangThai == TrangThaiChuong.DaXuatBan && !c.LaPhuChuong);
                truyen.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            SetSuccessMessage($"Đã thêm thành công!");
            return RedirectToAction(nameof(DanhSach), new { maTruyen = maTruyen });
        }// --- CÁC HÀM XEM, SỬA, XÓA THÊM MỚI ---

        [HttpGet]
        public async Task<IActionResult> XemChuong(int id)
        {
            // Thêm .AsNoTracking() để lấy dữ liệu mới nhất từ DB, tránh lấy cache cũ
            var chuong = await _context.Chuongs
                .AsNoTracking()
                .Include(c => c.Truyen)
                .Include(c => c.NoiDungChuongs)
                .FirstOrDefaultAsync(c => c.MaChuong == id);

            if (chuong == null) return NotFound();

            return View(chuong);
        }
        [HttpGet]
        public async Task<IActionResult> SuaChuong(int id)
        {
            var chuong = await _context.Chuongs
                .Include(c => c.NoiDungChuongs)
                .FirstOrDefaultAsync(c => c.MaChuong == id);

            if (chuong == null) return NotFound();

            // Trích xuất nội dung Raw và Convert truyền sang View để render lên Textarea
            ViewBag.NoiDungRaw = chuong.NoiDungChuongs.FirstOrDefault(n => n.LoaiNoiDung == LoaiNoiDungChuong.BanGoc)?.NoiDung;
            ViewBag.NoiDungText = chuong.NoiDungChuongs.FirstOrDefault(n => n.LoaiNoiDung == LoaiNoiDungChuong.BanDich)?.NoiDung;

            return View(chuong);
            // Bạn sẽ cần tạo thêm file SuaChuong.cshtml (Giao diện giống hệt ThemChuong.cshtml)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaChuong(
            int maChuong,
            string tieuDe,
            int soChuong,
            bool laPhuChuong,
            string? noiDungRaw,
            string? noiDungText,
            TrangThaiChuong trangThai = TrangThaiChuong.DaXuatBan,
            DateTime? ngayHenGio = null)
        {
            var chuong = await _context.Chuongs
                .Include(c => c.NoiDungChuongs)
                .FirstOrDefaultAsync(c => c.MaChuong == maChuong);

            if (chuong == null) return NotFound();

            if (string.IsNullOrWhiteSpace(tieuDe))
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề không được để trống.");
                ViewBag.NoiDungRaw = noiDungRaw;
                ViewBag.NoiDungText = noiDungText;
                return View(chuong);
            }

            if (trangThai == TrangThaiChuong.HenGio && !ngayHenGio.HasValue)
            {
                ModelState.AddModelError("NgayHenGio", "Vui lòng chọn thời điểm hẹn giờ đăng.");
                ViewBag.NoiDungRaw = noiDungRaw;
                ViewBag.NoiDungText = noiDungText;
                return View(chuong);
            }

            if (chuong.LaPhuChuong != laPhuChuong || chuong.SoChuong != soChuong)
            {
                int oldSoChuong = chuong.SoChuong;
                chuong.SoChuong = -chuong.MaChuong;
                await _context.SaveChangesAsync();

                if (chuong.LaPhuChuong && !laPhuChuong)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Chuongs SET SoChuong = SoChuong + 1 WHERE MaTruyen = {0} AND SoChuong >= {1} AND LaPhuChuong = 0",
                        chuong.MaTruyen, soChuong);
                }
                else if (!chuong.LaPhuChuong && laPhuChuong)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Chuongs SET SoChuong = SoChuong - 1 WHERE MaTruyen = {0} AND SoChuong > {1} AND LaPhuChuong = 0",
                        chuong.MaTruyen, oldSoChuong);
                }
                else if (!chuong.LaPhuChuong && !laPhuChuong)
                {
                    if (soChuong > oldSoChuong)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE Chuongs SET SoChuong = SoChuong - 1 WHERE MaTruyen = {0} AND SoChuong > {1} AND SoChuong <= {2} AND LaPhuChuong = 0",
                            chuong.MaTruyen, oldSoChuong, soChuong);
                    }
                    else if (soChuong < oldSoChuong)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE Chuongs SET SoChuong = SoChuong + 1 WHERE MaTruyen = {0} AND SoChuong >= {1} AND SoChuong < {2} AND LaPhuChuong = 0",
                            chuong.MaTruyen, soChuong, oldSoChuong);
                    }
                }
            }

            var wasPublished = chuong.TrangThai == TrangThaiChuong.DaXuatBan;
            chuong.TieuDe = tieuDe;
            chuong.TrangThai = trangThai;
            chuong.NgayHenGio = trangThai == TrangThaiChuong.HenGio ? ngayHenGio : null;
            chuong.SoChuong = soChuong;
            chuong.LaPhuChuong = laPhuChuong;

            // 2. Cập nhật hoặc Thêm mới Nội dung Raw
            var rawEntity = chuong.NoiDungChuongs.FirstOrDefault(n => n.LoaiNoiDung == LoaiNoiDungChuong.BanGoc);
            if (rawEntity != null)
                rawEntity.NoiDung = noiDungRaw;
            else if (!string.IsNullOrWhiteSpace(noiDungRaw))
                _context.NoiDungChuongs.Add(new NoiDungChuong { MaChuong = maChuong, NoiDung = noiDungRaw, LoaiNoiDung = LoaiNoiDungChuong.BanGoc });

            // 3. Cập nhật hoặc Thêm mới Nội dung Convert
            var textEntity = chuong.NoiDungChuongs.FirstOrDefault(n => n.LoaiNoiDung == LoaiNoiDungChuong.BanDich);
            if (textEntity != null)
                textEntity.NoiDung = noiDungText;
            else if (!string.IsNullOrWhiteSpace(noiDungText))
                _context.NoiDungChuongs.Add(new NoiDungChuong { MaChuong = maChuong, NoiDung = noiDungText, LoaiNoiDung = LoaiNoiDungChuong.BanDich });

            var truyen = await _context.Truyens.FindAsync(chuong.MaTruyen);
            if (truyen != null)
            {
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == chuong.MaTruyen && c.TrangThai == TrangThaiChuong.DaXuatBan && !c.LaPhuChuong);
                if (trangThai == TrangThaiChuong.DaXuatBan && !wasPublished)
                {
                    truyen.NgayCapNhat = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            SetSuccessMessage("Cập nhật chương thành công!");

            return RedirectToAction(nameof(DanhSach), new { maTruyen = chuong.MaTruyen });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatTieuDeNhanh(int maChuong, string tieuDe)
        {
            var chuong = await _context.Chuongs.FindAsync(maChuong);
            if (chuong == null) return Json(new { success = false, message = "Không tìm thấy chương." });

            chuong.TieuDe = tieuDe;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DoiTrangThaiNhanh(int maChuong)
        {
            var chuong = await _context.Chuongs.FindAsync(maChuong);
            if (chuong == null) return Json(new { success = false, message = "Không tìm thấy chương." });

            var wasPublished = chuong.TrangThai == TrangThaiChuong.DaXuatBan;
            chuong.TrangThai = wasPublished ? TrangThaiChuong.BanNhap : TrangThaiChuong.DaXuatBan;
            
            var truyen = await _context.Truyens.FindAsync(chuong.MaTruyen);
            if (truyen != null)
            {
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == chuong.MaTruyen && c.TrangThai == TrangThaiChuong.DaXuatBan && !c.LaPhuChuong);
            }
            
            await _context.SaveChangesAsync();
            return Json(new { success = true, isPublished = chuong.TrangThai == TrangThaiChuong.DaXuatBan });
        }

        [HttpPost]
        public async Task<IActionResult> DoiSoChuong(int maChuong, int soChuongMoi)
        {
            try
            {
                var chuong = await _context.Chuongs.FindAsync(maChuong);
                if (chuong == null) return Json(new { success = false, message = "Không tìm thấy chương." });

                if (chuong.SoChuong != soChuongMoi)
                {
                    int oldSoChuong = chuong.SoChuong;
                    chuong.SoChuong = -chuong.MaChuong;
                    await _context.SaveChangesAsync();

                    if (!chuong.LaPhuChuong)
                    {
                        if (soChuongMoi > oldSoChuong)
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                                "UPDATE Chuongs SET SoChuong = SoChuong - 1 WHERE MaTruyen = {0} AND SoChuong > {1} AND SoChuong <= {2} AND LaPhuChuong = 0",
                                chuong.MaTruyen, oldSoChuong, soChuongMoi);
                        }
                        else if (soChuongMoi < oldSoChuong)
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                                "UPDATE Chuongs SET SoChuong = SoChuong + 1 WHERE MaTruyen = {0} AND SoChuong >= {1} AND SoChuong < {2} AND LaPhuChuong = 0",
                                chuong.MaTruyen, soChuongMoi, oldSoChuong);
                        }
                    }
                    chuong.SoChuong = soChuongMoi;
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, "Lỗi Server Chi Tiết: " + inner);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaChuong(int id)
        {
            var chuong = await _context.Chuongs.FindAsync(id);
            if (chuong == null) return NotFound();

            int maTruyen = chuong.MaTruyen;
            int deletedSoChuong = chuong.SoChuong;
            bool wasPhuChuong = chuong.LaPhuChuong;

            // Xóa các file ảnh truyện tranh vật lý trên ổ cứng
            var noiDungChuongs = await _context.NoiDungChuongs.Where(n => n.MaChuong == id).Select(n => n.NoiDung).ToListAsync();
            foreach (var nd in noiDungChuongs)
            {
                if (!string.IsNullOrEmpty(nd))
                {
                    var matches = System.Text.RegularExpressions.Regex.Matches(nd, @"/uploads/comics/([^'\""\s>]+)");
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        var imgPath = Path.Combine(_env.WebRootPath, match.Value.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(imgPath)) System.IO.File.Delete(imgPath);
                    }
                }
            }

            _context.Chuongs.Remove(chuong);
            await _context.SaveChangesAsync();
            
            if (!wasPhuChuong)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Chuongs SET SoChuong = SoChuong - 1 WHERE MaTruyen = {0} AND SoChuong > {1} AND LaPhuChuong = 0",
                    maTruyen, deletedSoChuong);
            }

            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen != null)
            {
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == maTruyen && c.TrangThai == TrangThaiChuong.DaXuatBan && !c.LaPhuChuong);
                truyen.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            SetSuccessMessage($"Đã xóa thành công!");
            return RedirectToAction(nameof(DanhSach), new { maTruyen = maTruyen });
        }

        [HttpGet]
        public IActionResult ThemGallery(int maTruyen)
        {
            ViewBag.MaTruyen = maTruyen;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemGallery(int maTruyen, string tieuDe, List<IFormFile> images, TrangThaiChuong trangThai = TrangThaiChuong.DaXuatBan)
        {
            if (string.IsNullOrWhiteSpace(tieuDe) || images == null || !images.Any())
            {
                ModelState.AddModelError("", "Vui lòng nhập tiêu đề và chọn ít nhất 1 ảnh.");
                ViewBag.MaTruyen = maTruyen;
                return View();
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "comics");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var htmlContent = "";
            foreach (var img in images)
            {
                if (img.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }
                    htmlContent += $"<img src='/uploads/comics/{fileName}' alt='Comic Page' style='max-width:100%; display:block; margin: 0 auto;' />\n";
                }
            }

            int soChuongMoi = (await _context.Chuongs.Where(c => c.MaTruyen == maTruyen && !c.LaPhuChuong).Select(c => (int?)c.SoChuong).MaxAsync() ?? 0) + 1;

            var chuong = new Chuong
            {
                MaTruyen = maTruyen,
                SoChuong = soChuongMoi,
                TieuDe = tieuDe,
                TrangThai = trangThai,
                NgayTao = DateTime.Now
            };

            _context.Chuongs.Add(chuong);
            await _context.SaveChangesAsync();

            _context.NoiDungChuongs.Add(new NoiDungChuong
            {
                MaChuong = chuong.MaChuong,
                NoiDung = htmlContent,
                LoaiNoiDung = LoaiNoiDungChuong.BanDich
            });

            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen != null)
            {
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == maTruyen && c.TrangThai == TrangThaiChuong.DaXuatBan && !c.LaPhuChuong);
                truyen.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            SetSuccessMessage("Đăng truyện tranh thành công!");
            return RedirectToAction(nameof(DanhSach), new { maTruyen = maTruyen });
        }

        [HttpGet]
        public IActionResult ThemNhieuChuong(int maTruyen)
        {
            ViewBag.MaTruyen = maTruyen;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemNhieuChuong(int maTruyen, List<IFormFile> textFiles, TrangThaiChuong trangThai = TrangThaiChuong.DaXuatBan)
        {
            if (textFiles == null || !textFiles.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 file .txt");
                ViewBag.MaTruyen = maTruyen;
                return View();
            }

            int soChuongHienTai = (await _context.Chuongs.Where(c => c.MaTruyen == maTruyen && !c.LaPhuChuong).Select(c => (int?)c.SoChuong).MaxAsync() ?? 0);
            
            var sortedFiles = textFiles.OrderBy(f => f.FileName).ToList();

            foreach (var file in sortedFiles)
            {
                if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".txt")
                {
                    soChuongHienTai++;
                    using var reader = new StreamReader(file.OpenReadStream());
                    var content = await reader.ReadToEndAsync();
                    var tieuDe = Path.GetFileNameWithoutExtension(file.FileName);

                    var chuong = new Chuong
                    {
                        MaTruyen = maTruyen,
                        SoChuong = soChuongHienTai,
                        TieuDe = tieuDe,
                        TrangThai = trangThai,
                        NgayTao = DateTime.Now
                    };
                    _context.Chuongs.Add(chuong);
                    await _context.SaveChangesAsync();

                    _context.NoiDungChuongs.Add(new NoiDungChuong
                    {
                        MaChuong = chuong.MaChuong,
                        NoiDung = content,
                        LoaiNoiDung = LoaiNoiDungChuong.BanDich
                    });
                }
            }

            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen != null)
            {
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == maTruyen && c.TrangThai == TrangThaiChuong.DaXuatBan && !c.LaPhuChuong);
                truyen.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            SetSuccessMessage("Tải lên hàng loạt thành công!");
            return RedirectToAction(nameof(DanhSach), new { maTruyen = maTruyen });
        }

        [HttpPost]
        public async Task<IActionResult> TranslateText([FromBody] TranslateRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.RawText))
            {
                return Json(new { success = false, error = "Nội dung Raw không được để trống." });
            }

            try
            {
                string systemInstruction = "Bạn là một dịch giả chuyên nghiệp các tác phẩm tiểu thuyết mạng (Tiên Hiệp, Huyền Huyễn, Ngôn Tình...).";
                
                string translatedTitle = "";
                if (!string.IsNullOrWhiteSpace(req.RawTitle))
                {
                    string titlePrompt = "Hãy dịch tiêu đề chương tiểu thuyết sau sang tiếng Việt (giữ nguyên phong cách tiên hiệp/kỳ ảo nếu có), chỉ trả về đúng kết quả dịch, không giải thích hay thêm bớt gì:\n\n" + req.RawTitle;
                    translatedTitle = await _geminiService.GenerateContentAsync(titlePrompt, systemInstruction);
                }

                string prompt = "Hãy dịch đoạn văn bản tiểu thuyết (tiếng Trung/Nhật/Anh) sau sang tiếng Việt. Giữ nguyên định dạng ngắt dòng (xuống dòng bằng <br> hoặc <p>), hành văn mượt mà, thuần Việt, chuẩn ngữ pháp và đúng thể loại tiên hiệp/huyền huyễn.\n\nLưu ý quan trọng: Nếu văn bản gốc là tiếng Trung và có dấu hiệu bị thiếu chữ/thủng lỗ chỗ (do lỗi chống copy của các web như Fanqie), hãy tự động dùng tư duy ngữ cảnh để điền bù các chữ bị thiếu (như □, 〇, ｘ, ＊, 屏蔽...) trước khi dịch để câu văn hoàn chỉnh.\n\n" + req.RawText;
                
                string translatedContent = await _geminiService.GenerateContentAsync(prompt, systemInstruction);
                
                return Json(new { success = true, result = translatedContent, titleResult = translatedTitle });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    public class TranslateRequest
    {
        public string RawText { get; set; } = string.Empty;
        public string RawTitle { get; set; } = string.Empty;
    }
}
