using System;
using System.Collections.Generic;
using System.Linq;

namespace HutechNovel.Models
{
    // Thêm class này để chứa Nhãn dán và Số lượng truyện
    public class CustomTagCount
    {
        public int MaThe { get; set; }
        public string TenThe { get; set; } = string.Empty;
        public int SoTruyen { get; set; }
    }

    public class SearchViewModel
    {
        public string? Keyword { get; set; }
        public string? Author { get; set; }
        public string? Summary { get; set; }
        public string? ChapterTitle { get; set; }
        public int? Status { get; set; }
        public string SortBy { get; set; } = "updated";

        public int? SelectedTagId { get; set; } // Dùng cho Thể loại chính (Dropdown)
        public List<int> SelectedCustomTagIds { get; set; } = new(); // THÊM MỚI: Dùng cho nhiều Nhãn dán (Click bật/tắt)

        public int? MinViews { get; set; }
        public int? MinChapters { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool Infinite { get; set; }

        public List<The> Tags { get; set; } = new();
        public List<CustomTagCount> CustomTags { get; set; } = new();
        public List<SearchStoryItemViewModel> Results { get; set; } = new();
    }

    public class SearchStoryItemViewModel
    {
        public int MaTruyen { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? AnhBia { get; set; }
        public TrangThaiTruyen TrangThai { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public string? TenTacGia { get; set; }
        public int? MaTacGia { get; set; }
        public int TongLuotXem { get; set; }
        public int LuotXemNgay { get; set; }
        public int LuotXemTuan { get; set; }
        public int TongSoChuong { get; set; }
        public int LuotThich { get; set; }
        public int LuotTheoDoi { get; set; }
        public int LuotDanhDau { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    // ... Các class khác giữ nguyên (RankingViewModel, StoryDetailViewModel, v.v...)
    public class RankingViewModel
    {
        public string CurrentType { get; set; } = "view";
        public List<Truyen> Stories { get; set; } = new();
    }

    public class StoryDetailViewModel
    {
        public Truyen Truyen { get; set; } = null!;
        public List<Chuong> Chapters { get; set; } = new();
        public List<BinhLuan> Comments { get; set; } = new();
        public string CommentSort { get; set; } = "highlight";
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalBookmarks { get; set; }
        public int TotalLikes { get; set; }
        public int TotalBoosts { get; set; }
        public int CurrentUserBoostTickets { get; set; }
        public bool IsLiked { get; set; }
        public bool IsBookmarked { get; set; }
        public bool IsFollowing { get; set; }
        public int UserRating { get; set; } // Thêm thuộc tính này
        public Chuong? FirstPublishedChapter => Chapters.FirstOrDefault();
        public HashSet<int> ReadChapterIds { get; set; } = new();
        public HashSet<int> ReactedCommentIds { get; set; } = new();
        public List<Truyen> SimilarStories { get; set; } = new();
    }

    public class ReadingViewModel
    {
        public Chuong Chapter { get; set; } = null!;
        public List<Chuong> Chapters { get; set; } = new();
        public NoiDungChuong? RawContent { get; set; }
        public NoiDungChuong? ConvertedContent { get; set; }
        public int? PreviousChapterNumber { get; set; }
        public int? NextChapterNumber { get; set; }
        public string SavedReadingPosition { get; set; } = "0";
        public string ReaderThemeJson { get; set; } = string.Empty;
        public string ReaderFontFamily { get; set; } = "'Palatino Linotype', 'Book Antiqua', Palatino, serif";
        public int ReaderFontSize { get; set; } = 22;
    }

    public class UserLibraryItemViewModel
    {
        public Truyen Story { get; set; } = null!;
        public Chuong? CurrentChapter { get; set; }
        public DateTime LastActivity { get; set; }
        public bool HasNewChapter { get; set; }
        public int? LastReadChapterNumber { get; set; }
    }

    public class UserLibraryViewModel
    {
        public List<UserLibraryItemViewModel> ReadingHistory { get; set; } = new();
        public List<UserLibraryItemViewModel> FollowingStories { get; set; } = new();
        public List<UserLibraryItemViewModel> BookmarkedStories { get; set; } = new();
        public List<UserLibraryItemViewModel> CompletedStories { get; set; } = new();
        public List<UserLibraryItemViewModel> ReadingNowStories { get; set; } = new();
        public List<UserLibraryItemViewModel> PausedStories { get; set; } = new();
        public List<UserLibraryItemViewModel> FavoriteStories { get; set; } = new();
        public List<UserLibraryItemViewModel> NewChapterStories { get; set; } = new();
    }

    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public int FollowingCount { get; set; }
        public int BookmarkCount { get; set; }
        public int CommentCount { get; set; }
        public int ReadHistoryCount { get; set; }
        public int ReadStoryCount { get; set; }
        public List<LichSuDoc> RecentReads { get; set; } = new();
    }

    public class UploaderDashboardViewModel
    {
        public int TotalStories { get; set; }
        public int TotalChapters { get; set; }
        public int TotalViews { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalBoosts { get; set; }
        public int DraftChapters { get; set; }
        public int ScheduledChapters { get; set; }
        public int TodayViews { get; set; }
        public List<Truyen> RecentStories { get; set; } = new();
        public List<UploaderStoryStatViewModel> TopStories { get; set; } = new();
        public List<UploaderChapterStatViewModel> TopChapters { get; set; } = new();

        // THÊM 3 DÒNG NÀY ĐỂ HỖ TRỢ TÌM KIẾM VÀ PHÂN TRANG
        public string? SearchKeyword { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
    }

    public class UploaderStoryStatViewModel
    {
        public Truyen Story { get; set; } = null!;
        public int TodayViews { get; set; }
        public int TotalViews { get; set; }
    }

    public class UploaderChapterStatViewModel
    {
        public Chuong Chapter { get; set; } = null!;
        public int ReadCount { get; set; }
    }

    public class StoryManagementViewModel
    {
        public List<Truyen> Stories { get; set; } = new();
        public List<TacGia> Authors { get; set; } = new();
    }

    public class ChapterManagementViewModel
    {
        public Truyen Story { get; set; } = null!;
        public List<Chuong> Chapters { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStories { get; set; }
        public int TotalViews { get; set; }
        public int PendingChapters { get; set; }
        public int ReportedCommentsCount { get; set; }
        public int NewUsersToday { get; set; }
        public int HotStoriesTodayCount { get; set; }

        public List<string> ChartLabels { get; set; } = new();
        public List<int> ChartViewsData { get; set; } = new();
        public List<int> ChartNewUsersData { get; set; } = new();

        public List<BinhLuan> NewComments { get; set; } = new();
        public List<BinhLuan> ReportedComments { get; set; } = new();
        public List<Truyen> LatestStories { get; set; } = new();
        public List<AdminHotStoryViewModel> HotStoriesToday { get; set; } = new();
        public List<AdminHotStoryViewModel> HotStoriesWeek { get; set; } = new();
        public List<AdminHotStoryViewModel> HotStoriesMonth { get; set; } = new();
        public List<NhatKyQuanTri> ActionLogs { get; set; } = new();
    }

    public class AdminHotStoryViewModel
    {
        public Truyen Story { get; set; } = null!;
        public int Views { get; set; }
    }

    public class UserAdminItemViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IList<string> Roles { get; set; } = Array.Empty<string>();
    }

    public class UserAdminViewModel
    {
        public List<UserAdminItemViewModel> Users { get; set; } = new();
    }
}
