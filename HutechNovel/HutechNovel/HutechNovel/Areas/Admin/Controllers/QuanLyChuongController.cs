// Areas/Admin/Controllers/QuanLyChuongController.cs
using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Areas.Admin.Controllers
{
    public class QuanLyChuongController : BaseUploaderController
    {
        private readonly ApplicationDbContext _context;

        public QuanLyChuongController(ApplicationDbContext context)
            => _context = context;

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
        public IActionResult ThemChuong(int maTruyen)
        {
            // Chỉ cần truyền mã truyện sang View, không cần khởi tạo các trường thừa
            ViewBag.MaTruyen = maTruyen;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemChuong(
            int maTruyen,
            string tieuDe,
            string? noiDungRaw,
            string? noiDungText,
            TrangThaiChuong trangThai = TrangThaiChuong.DaXuatBan,
            DateTime? ngayHenGio = null)
        {
            // Kiểm tra dữ liệu đầu vào cơ bản
            if (string.IsNullOrWhiteSpace(tieuDe))
            {
                ModelState.AddModelError("TieuDe", "Tiêu đề chương không được để trống.");
                ViewBag.MaTruyen = maTruyen;
                return View(new Chuong { MaTruyen = maTruyen, TieuDe = tieuDe });
            }

            // 1. Tự động tính số chương tiếp theo một cách an toàn
            if (trangThai == TrangThaiChuong.HenGio && !ngayHenGio.HasValue)
            {
                ModelState.AddModelError("NgayHenGio", "Vui lòng chọn thời điểm hẹn giờ đăng.");
                ViewBag.MaTruyen = maTruyen;
                return View(new Chuong { MaTruyen = maTruyen, TieuDe = tieuDe, TrangThai = trangThai, NgayHenGio = ngayHenGio });
            }

            int soChuongMoi = (await _context.Chuongs
     .Where(c => c.MaTruyen == maTruyen)
     .Select(c => (int?)c.SoChuong)
     .MaxAsync() ?? 0) + 1;

            // 2. Khởi tạo đối tượng với các thông số mặc định tự động
            var chuong = new Chuong
            {
                MaTruyen = maTruyen,
                SoChuong = soChuongMoi,
                TieuDe = tieuDe,
                TrangThai = trangThai,
                NgayTao = DateTime.Now,                // Thời điểm bấm lưu
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
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == maTruyen);
                truyen.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            SetSuccessMessage($"Đã thêm thành công Chương {soChuongMoi}: {tieuDe}!");
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

            // 1. Cập nhật thông tin cơ bản
            if (trangThai == TrangThaiChuong.HenGio && !ngayHenGio.HasValue)
            {
                ModelState.AddModelError("NgayHenGio", "Vui lòng chọn thời điểm hẹn giờ đăng.");
                ViewBag.NoiDungRaw = noiDungRaw;
                ViewBag.NoiDungText = noiDungText;
                return View(chuong);
            }

            var wasPublished = chuong.TrangThai == TrangThaiChuong.DaXuatBan;
            chuong.TieuDe = tieuDe;
            chuong.TrangThai = trangThai;
            chuong.NgayHenGio = trangThai == TrangThaiChuong.HenGio ? ngayHenGio : null;

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
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == chuong.MaTruyen);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaChuong(int id)
        {
            var chuong = await _context.Chuongs.FindAsync(id);
            if (chuong == null) return NotFound();

            int maTruyen = chuong.MaTruyen; // Lưu lại để tí Redirect về đúng danh sách truyện đó

            // Entity Framework sẽ tự động xóa các NoiDungChuong liên quan nếu bạn đã cấu hình Cascade Delete (Mặc định là có)
            _context.Chuongs.Remove(chuong);
            await _context.SaveChangesAsync();

            // Cập nhật lại tổng số chương của Truyện sau khi xóa
            var truyen = await _context.Truyens.FindAsync(maTruyen);
            if (truyen != null)
            {
                truyen.TongSoChuong = await _context.Chuongs.CountAsync(c => c.MaTruyen == maTruyen);
                truyen.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            SetSuccessMessage($"Đã xóa thành công chương {chuong.SoChuong}!");
            return RedirectToAction(nameof(DanhSach), new { maTruyen = maTruyen });
        }
    }
}
