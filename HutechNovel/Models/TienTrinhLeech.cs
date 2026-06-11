using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HutechNovel.Models
{
    public class TienTrinhLeech
    {
        [Key]
        public int Id { get; set; }

        public int MaTruyen { get; set; }

        [ForeignKey("MaTruyen")]
        public virtual Truyen Truyen { get; set; } = null!;

        [Required]
        public string UrlHienTai { get; set; } = string.Empty;

        public int SoChuongDaCao { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "DangChay"; // DangChay, HoanThanh, Loi

        public DateTime NgayBatDau { get; set; } = DateTime.Now;
        public DateTime? NgayKetThuc { get; set; }

        public string? ThongBaoLoi { get; set; }
    }
}
