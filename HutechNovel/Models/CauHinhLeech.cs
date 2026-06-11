using System;
using System.ComponentModel.DataAnnotations;

namespace HutechNovel.Models
{
    public class CauHinhLeech
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Domain { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string TitleSelector { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ContentSelector { get; set; } = string.Empty;

        [StringLength(255)]
        public string NextChapterSelector { get; set; } = string.Empty;
    }
}
