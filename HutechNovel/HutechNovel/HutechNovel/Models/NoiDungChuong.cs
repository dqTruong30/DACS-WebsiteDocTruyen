using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HutechNovel.Models
{
    public class NoiDungChuong
    {
        [Key]
        public int MaNoiDung { get; set; }

        public int MaChuong { get; set; }
        [ForeignKey("MaChuong")]
        public virtual Chuong Chuong { get; set; } = null!;

        [Required]
        public string NoiDung { get; set; } = string.Empty; // Chứa text truyện

        [Required]
        public LoaiNoiDungChuong LoaiNoiDung { get; set; } // Raw hoặc Convert  

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}