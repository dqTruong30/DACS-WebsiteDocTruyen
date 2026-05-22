using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace HutechNovel.Models
{
    public class DanhDau
    {
        [Key] public int MaDanhDau { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        public virtual ApplicationUser NguoiDung { get; set; } = null!;
        public int MaTruyen { get; set; }
        public virtual Truyen Truyen { get; set; } = null!;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }

    public class YeuThich
    {
        [Key] public int MaYeuThich { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        public virtual ApplicationUser NguoiDung { get; set; } = null!;
        public int MaTruyen { get; set; }
        public virtual Truyen Truyen { get; set; } = null!;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }

    public class LichSuDoc
    {
        [Key] public int MaLichSu { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        public virtual ApplicationUser NguoiDung { get; set; } = null!;
        public int MaChuong { get; set; }
        public virtual Chuong Chuong { get; set; } = null!;
        public DateTime ThoiGianDoc { get; set; } = DateTime.Now;
        public string ViTriDoc { get; set; } = string.Empty; // Lưu vị trí pixel hoặc dòng đang đọc
    }

    public class DanhGia
    {
        [Key] public int MaDanhGia { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        public virtual ApplicationUser NguoiDung { get; set; } = null!;
        public int MaTruyen { get; set; }
        public virtual Truyen Truyen { get; set; } = null!;
        [Range(1, 5)] public int DiemSo { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }

    public class LuotXem
    {
        [Key] public int MaLuotXem { get; set; }
        public int MaTruyen { get; set; }
        public virtual Truyen Truyen { get; set; } = null!;
        public string? MaNguoiDung { get; set; } // Nullable cho khách
        public virtual ApplicationUser? NguoiDung { get; set; }
        public string? IpAddress { get; set; } // Theo dõi IP để tính view cho khách
        public DateTime ThoiGianXem { get; set; } = DateTime.Now;
    }

    public class TheoDoiTruyen
    {
        [Key] public int MaTheoDoi { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        [ForeignKey(nameof(MaNguoiDung))]
        public virtual ApplicationUser NguoiDung { get; set; } = null!;
        public int MaTruyen { get; set; }
        [ForeignKey(nameof(MaTruyen))]
        public virtual Truyen Truyen { get; set; } = null!;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }

    public class DayTruyen
    {
        [Key] public int MaDay { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        public virtual ApplicationUser NguoiDung { get; set; } = null!;
        public int MaTruyen { get; set; }
        public virtual Truyen Truyen { get; set; } = null!;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }

    public class NhatKyQuanTri
    {
        [Key] public int MaNhatKy { get; set; }
        public string? MaNguoiDung { get; set; }
        public string HanhDong { get; set; } = string.Empty;
        public string DoiTuong { get; set; } = string.Empty;
        public int? MaDoiTuong { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }

    public class BinhLuanCamXuc
    {
        [Key] public int MaCamXuc { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        public virtual ApplicationUser NguoiDung { get; set; } = null!;
        public int MaBinhLuan { get; set; }
        public virtual BinhLuan BinhLuan { get; set; } = null!;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
