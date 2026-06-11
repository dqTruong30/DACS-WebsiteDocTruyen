using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HutechNovel.Models
{
    public class NhiemVu
    {
        [Key]
        public int MaNhiemVu { get; set; }

        [Required]
        [StringLength(200)]
        public string TenNhiemVu { get; set; } = string.Empty;

        public string MoTa { get; set; } = string.Empty;

        // Loai: "HangNgay", "HangTuan", "ThanhTuu", "TanThu"
        [Required]
        [StringLength(50)]
        public string LoaiNhiemVu { get; set; } = "HangNgay";

        public int PhanThuongXu { get; set; } = 0;
        public int PhanThuongKinhNghiem { get; set; } = 0;

        // Yeu cau the loai nao do, vd: SoChuongDoc, SoBinhLuan...
        [StringLength(50)]
        public string LoaiDieuKien { get; set; } = string.Empty;
        
        public int GiaTriYeuCau { get; set; } = 0;

        public virtual ICollection<LichSuNhiemVu> LichSuNhiemVus { get; set; } = new HashSet<LichSuNhiemVu>();
    }
}
