using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HutechNovel.Models
{
    public class Truyen
    {
        [Key]
        public int MaTruyen { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(255)]
        public string TieuDe { get; set; } = null!;

        public string MoTa { get; set; } = string.Empty;
        public string AnhBia { get; set; } = string.Empty;

        [Required]
        public TrangThaiTruyen TrangThai { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // [MỚI] Các trường Denormalization (Tối ưu hiệu năng lọc/sắp xếp)
        public int TongSoChuong { get; set; } = 0;
        public int TongLuotXem { get; set; } = 0;
        public double DiemDanhGiaTrungBinh { get; set; } = 0.0;
        public int TongSoSao { get; set; } = 0; // Thêm trường này

        // [MỚI] Điểm xu hướng (Dùng cho thuật toán Time Decay)
        public double DiemTrending { get; set; } = 0.0;

        // Khóa ngoại - Tác giả
        public int MaTacGia { get; set; }
        public virtual TacGia TacGia { get; set; } = null!;

        // [MỚI] Khóa ngoại - Uploader (Người đăng truyện)
        public string NguoiDangId { get; set; } = null!;
        public virtual ApplicationUser NguoiDang { get; set; } = null!;

        // Navigation properties cũ giữ nguyên...
        public virtual ICollection<Chuong> Chuongs { get; set; } = new HashSet<Chuong>();
        public virtual ICollection<The> Thes { get; set; } = new HashSet<The>();
        public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new HashSet<BinhLuan>();
        public virtual ICollection<DanhDau> DanhDaus { get; set; } = new HashSet<DanhDau>();
        public virtual ICollection<YeuThich> YeuThichs { get; set; } = new HashSet<YeuThich>();
        public virtual ICollection<TheoDoiTruyen> TheoDoiTruyens { get; set; } = new HashSet<TheoDoiTruyen>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new HashSet<DanhGia>();
        public virtual ICollection<LuotXem> LuotXems { get; set; } = new HashSet<LuotXem>();
        public virtual ICollection<DayTruyen> DayTruyens { get; set; } = new HashSet<DayTruyen>();
    }
}
