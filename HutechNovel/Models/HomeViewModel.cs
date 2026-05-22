namespace HutechNovel.Models
{
    // Class phụ để gom thêm dữ liệu thống kê cho truyện Hot
    public class TruyenHotViewModel
    {
        public Truyen Truyen { get; set; } = null!;
        public int LuotXemNgay { get; set; }
        public int LuotXemTuan { get; set; }
    }

    public class HomeViewModel
    {
        public string? BannerUrl { get; set; }
        public List<TruyenHotViewModel> TruyenHot { get; set; } = new();
        public List<Truyen> TruyenMoiCapNhat { get; set; } = new();
        public List<Truyen> TrendingStories { get; set; } = new();
        public Truyen? TruyenMoiDang { get; set; }
        public List<LichSuDoc> LichSuDocs { get; set; } = new(); // Chứa lịch sử đọc thật\
        public List<TruyenHotViewModel> TopTruyenNgay { get; set; } = new();
        public List<TruyenHotViewModel> TopTruyenTuan { get; set; } = new();
        public List<Truyen> SmartRecommendations { get; set; } = new();
    }
}
