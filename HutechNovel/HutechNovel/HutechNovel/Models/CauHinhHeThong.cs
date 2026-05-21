using System.ComponentModel.DataAnnotations;

namespace HutechNovel.Models
{
    public class CauHinhHeThong
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string TenWebsite { get; set; } = "HutechNovel";

        public string? ThongBaoToanCuc { get; set; }

        public bool CheDoBaoTri { get; set; } = false;

        [MaxLength(500)]
        public string? TieuDeSEO { get; set; } = "HutechNovel - Nền tảng đọc truyện trực tuyến";

        public string? MoTaSEO { get; set; } = "Đọc truyện chữ, truyện convert cập nhật nhanh nhất.";

        [MaxLength(255)]
        public string? EmailLienHe { get; set; } = "admin@gmail.com";

        // Thêm trường để lưu Banner
        public string? BannerUrl { get; set; }
    }
}