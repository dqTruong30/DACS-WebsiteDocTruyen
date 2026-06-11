using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Services;

public sealed class NhiemVuService : INhiemVuService
{
    private static readonly NhiemVuSeed[] DefaultMissions =
    [
        new("Điểm danh hằng ngày", "Đăng nhập và điểm danh mỗi ngày", "HangNgay", 10, 50, "DiemDanh", 1),
        new("Mọt sách", "Đọc 5 chương truyện", "HangNgay", 20, 100, "DocChuong", 5),
        new("Nhà bình luận", "Bình luận 3 lần", "HangNgay", 15, 80, "BinhLuan", 3)
    ];

    private readonly ApplicationDbContext _context;

    public NhiemVuService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureDefaultNhiemVuAsync()
    {
        var changed = false;

        foreach (var seed in DefaultMissions)
        {
            var mission = await _context.NhiemVus
                .FirstOrDefaultAsync(x => x.LoaiDieuKien == seed.LoaiDieuKien);

            if (mission == null)
            {
                _context.NhiemVus.Add(seed.ToNhiemVu());
                changed = true;
                continue;
            }

            if (mission.TenNhiemVu != seed.TenNhiemVu
                || mission.MoTa != seed.MoTa
                || mission.LoaiNhiemVu != seed.LoaiNhiemVu
                || mission.PhanThuongXu != seed.PhanThuongXu
                || mission.PhanThuongKinhNghiem != seed.PhanThuongKinhNghiem
                || mission.GiaTriYeuCau != seed.GiaTriYeuCau)
            {
                mission.TenNhiemVu = seed.TenNhiemVu;
                mission.MoTa = seed.MoTa;
                mission.LoaiNhiemVu = seed.LoaiNhiemVu;
                mission.PhanThuongXu = seed.PhanThuongXu;
                mission.PhanThuongKinhNghiem = seed.PhanThuongKinhNghiem;
                mission.GiaTriYeuCau = seed.GiaTriYeuCau;
                changed = true;
            }
        }

        if (changed)
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task<NhiemVuProgressResult> CapNhatTienDoAsync(string userId, string loaiDieuKien, int soLuong = 1)
    {
        var result = new NhiemVuProgressResult();
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(loaiDieuKien) || soLuong <= 0)
        {
            return result;
        }

        await EnsureDefaultNhiemVuAsync();

        var mission = await _context.NhiemVus
            .FirstOrDefaultAsync(x => x.LoaiDieuKien == loaiDieuKien);

        if (mission == null)
        {
            return result;
        }

        var now = DateTime.Now;
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var history = await _context.LichSuNhiemVus
            .FirstOrDefaultAsync(x => x.UserId == userId
                && x.MaNhiemVu == mission.MaNhiemVu
                && x.NgayCapNhat >= today
                && x.NgayCapNhat < tomorrow);

        if (history == null)
        {
            history = new LichSuNhiemVu
            {
                UserId = userId,
                MaNhiemVu = mission.MaNhiemVu,
                TienDo = 0,
                NgayCapNhat = now
            };
            _context.LichSuNhiemVus.Add(history);
        }

        var required = Math.Max(1, mission.GiaTriYeuCau);
        if (!history.DaHoanThanh)
        {
            history.TienDo = Math.Min(required, history.TienDo + soLuong);
            history.NgayCapNhat = now;
            history.DaHoanThanh = history.TienDo >= required;
            result.DaCapNhat = true;
        }

        if (history.DaHoanThanh && !history.DaNhanThuong)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.HutechXu += mission.PhanThuongXu;
                user.DiemKinhNghiem += mission.PhanThuongKinhNghiem;
            }

            history.DaNhanThuong = true;
            result.VuaHoanThanh = true;
        }

        result.NhiemVu = mission;
        return result;
    }

    private sealed record NhiemVuSeed(
        string TenNhiemVu,
        string MoTa,
        string LoaiNhiemVu,
        int PhanThuongXu,
        int PhanThuongKinhNghiem,
        string LoaiDieuKien,
        int GiaTriYeuCau)
    {
        public NhiemVu ToNhiemVu()
        {
            return new NhiemVu
            {
                TenNhiemVu = TenNhiemVu,
                MoTa = MoTa,
                LoaiNhiemVu = LoaiNhiemVu,
                PhanThuongXu = PhanThuongXu,
                PhanThuongKinhNghiem = PhanThuongKinhNghiem,
                LoaiDieuKien = LoaiDieuKien,
                GiaTriYeuCau = GiaTriYeuCau
            };
        }
    }
}
