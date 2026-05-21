using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HutechNovel.Models
{
    public class BinhLuan
    {
        [Key]
        public int MaBinhLuan { get; set; }

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool LaSpoiler { get; set; }
        public bool DaGhim { get; set; }
        public int SoCamXuc { get; set; }
        public int SoBaoCao { get; set; }

        public string MaNguoiDung { get; set; } = null!;
        public virtual ApplicationUser NguoiDung { get; set; } = null!;

        public int MaTruyen { get; set; }
        public virtual Truyen Truyen { get; set; } = null!;

        public int? MaChuong { get; set; }
        public virtual Chuong? Chuong { get; set; }

        // [MỚI] Quan hệ Cha-Con (Reply)
        public int? MaBinhLuanCha { get; set; }
        public virtual BinhLuan? BinhLuanCha { get; set; }
        public virtual ICollection<BinhLuan> BinhLuanCons { get; set; } = new HashSet<BinhLuan>();
    }
}
