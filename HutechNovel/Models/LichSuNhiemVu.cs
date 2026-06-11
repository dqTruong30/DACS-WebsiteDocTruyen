using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HutechNovel.Models
{
    public class LichSuNhiemVu
    {
        [Key]
        public int MaLichSu { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int MaNhiemVu { get; set; }

        [ForeignKey("MaNhiemVu")]
        public virtual NhiemVu NhiemVu { get; set; }

        public int TienDo { get; set; } = 0; // Số lượng đã làm được

        public bool DaHoanThanh { get; set; } = false;
        
        public bool DaNhanThuong { get; set; } = false;

        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
    }
}
