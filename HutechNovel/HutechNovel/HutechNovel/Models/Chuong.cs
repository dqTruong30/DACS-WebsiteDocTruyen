using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HutechNovel.Models
{
    public class Chuong
    {
        [Key]
        public int MaChuong { get; set; }

        [Required]
        [MaxLength(255)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        public int SoChuong { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayHenGio { get; set; }

        // [MỚI] Trạng thái xuất bản
        public TrangThaiChuong TrangThai { get; set; } = TrangThaiChuong.DaXuatBan;

        public int MaTruyen { get; set; }
        [ForeignKey("MaTruyen")]
        public virtual Truyen Truyen { get; set; } = null!;

        public virtual ICollection<NoiDungChuong> NoiDungChuongs { get; set; } = new HashSet<NoiDungChuong>();
        public virtual ICollection<LichSuDoc> LichSuDocs { get; set; } = new HashSet<LichSuDoc>();
        public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new HashSet<BinhLuan>();
    }
}