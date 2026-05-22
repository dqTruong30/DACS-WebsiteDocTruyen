using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 1. Bắt buộc THÊM thư viện này

namespace HutechNovel.Models
{
    public class TacGia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 2. THÊM dòng này để báo EF Core tự tăng
        public int MaTacGia { get; set; }

        [Required]
        [MaxLength(255)]
        public string TenTacGia { get; set; } = null!;

        public string? TieuSu { get; set; }

        public virtual ICollection<Truyen> Truyens { get; set; } = new HashSet<Truyen>();
    }
}