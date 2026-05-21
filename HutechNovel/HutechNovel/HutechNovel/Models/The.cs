using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HutechNovel.Models
{
    public class The
    {
        [Key]
        public int MaThe { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenThe { get; set; } = null!;

        public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
    }
}