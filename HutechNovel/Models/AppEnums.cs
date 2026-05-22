using System.ComponentModel.DataAnnotations;

namespace HutechNovel.Models
{
    public enum TrangThaiTruyen
    {
        [Display(Name = "Đang tiến hành")]
        DangTienHanh,

        [Display(Name = "Đã hoàn thành")]
        DaHoanThanh,

        [Display(Name = "Tạm ngưng")]
        TamNgung
    }

    public enum LoaiNoiDungChuong
    {
        BanGoc,
        BanDich
    }

    public static class VaiTroHeThong
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Uploader = "Uploader";
    }

    public enum TrangThaiChuong
    {
        BanNhap,    // Chỉ Uploader thấy
        DaXuatBan,  // Mọi người đều thấy
        HenGio      // Đợi đến NgayHenGio mới chuyển sang DaXuatBan
    }
}
