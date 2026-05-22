using System.Globalization;
using System.Security.Claims;
using System.Text;
using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatBotController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] ChatBotRequest request)
        {
            var message = request.Message?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(message))
            {
                return Ok(new ChatBotResponse
                {
                    Reply = "Bạn muốn mình hỗ trợ phần nào? Mình có thể hướng dẫn dùng web, tìm truyện, gợi ý truyện, xem thống kê, hoặc chỉ bạn tới đúng trang chức năng."
                });
            }

            var normalized = NormalizeForMatch(message);

            var directAnswer = await BuildDirectAnswerResponse(normalized);
            if (directAnswer != null)
            {
                return Ok(directAnswer);
            }

            var stats = await BuildStatsResponse(normalized);
            if (stats != null)
            {
                return Ok(stats);
            }

            var smartAssistant = await BuildSmartAssistantResponse(message, normalized);
            if (smartAssistant != null)
            {
                return Ok(smartAssistant);
            }

            var filter = await BuildFilterResponse(message, normalized);
            if (filter != null)
            {
                return Ok(filter);
            }

            if (IsStorySearchIntent(normalized))
            {
                return Ok(await BuildStorySearchResponse(message, normalized));
            }

            var directStoryResult = await TryFindStoriesByText(message, normalized);
            if (directStoryResult.Stories.Any())
            {
                directStoryResult.Reply = $"Mình tìm thấy {directStoryResult.Stories.Count} truyện có thể liên quan đến \"{message}\".";
                return Ok(directStoryResult);
            }

            var guide = BuildGuideResponse(normalized);
            if (guide != null)
            {
                return Ok(guide);
            }

            return Ok(BuildFeatureOverviewResponse());
        }

        private async Task<ChatBotResponse?> BuildDirectAnswerResponse(string normalized)
        {
            if (IsCurrentReadingQuestion(normalized))
            {
                return await BuildCurrentReadingResponse(normalized);
            }

            var storyFact = await BuildStoryFactResponse(normalized);
            if (storyFact != null)
            {
                return storyFact;
            }

            if (ContainsAny(normalized, "cao nhat", "nhieu nhat", "top 1", "dung dau", "lon nhat"))
            {
                if (ContainsAny(normalized, "luot xem", "luot doc", "view"))
                {
                    var story = await GetTopStory("views");
                    return story == null
                        ? NoStoryResponse()
                        : SingleStoryAnswer($"Truyen co luot xem cao nhat hien tai la \"{story.Title}\" voi {story.Views:N0} luot xem.", story);
                }

                if (ContainsAny(normalized, "chuong", "so chuong", "dai nhat"))
                {
                    var story = await GetTopStory("chapters");
                    return story == null
                        ? NoStoryResponse()
                        : SingleStoryAnswer($"Truyen co so chuong nhieu nhat hien tai la \"{story.Title}\" voi {story.Chapters:N0} chuong.", story);
                }

                if (ContainsAny(normalized, "thich", "like"))
                {
                    var story = await GetTopStory("likes");
                    return story == null
                        ? NoStoryResponse()
                        : SingleStoryAnswer($"Truyen co luot thich cao nhat hien tai la \"{story.Title}\".", story);
                }

                if (ContainsAny(normalized, "theo doi", "follow"))
                {
                    var story = await GetTopStory("follows");
                    return story == null
                        ? NoStoryResponse()
                        : SingleStoryAnswer($"Truyen co luot theo doi cao nhat hien tai la \"{story.Title}\".", story);
                }

                if (ContainsAny(normalized, "danh dau", "bookmark", "doc sau"))
                {
                    var story = await GetTopStory("bookmarks");
                    return story == null
                        ? NoStoryResponse()
                        : SingleStoryAnswer($"Truyen duoc danh dau nhieu nhat hien tai la \"{story.Title}\".", story);
                }
            }

            if (ContainsAny(normalized, "moi cap nhat", "cap nhat moi", "moi nhat", "gan day"))
            {
                var story = await GetTopStory("updated");
                return story == null
                    ? NoStoryResponse()
                    : SingleStoryAnswer($"Truyen moi cap nhat gan day nhat la \"{story.Title}\".", story);
            }

            return null;
        }

        private async Task<ChatBotResponse?> BuildStoryFactResponse(string normalized)
        {
            if (!ContainsAny(normalized, "bao nhieu", "luot xem", "luot doc", "view", "chuong", "tac gia", "trang thai"))
            {
                return null;
            }

            var story = await FindMentionedStory(normalized);
            if (story == null)
            {
                return null;
            }

            var result = new ChatBotStoryResult
            {
                Title = story.Title,
                Author = story.Author,
                Chapters = story.Chapters,
                Views = story.Views,
                Url = story.Url
            };

            if (ContainsAny(normalized, "luot xem", "luot doc", "view"))
            {
                return SingleStoryAnswer($"\"{story.Title}\" hien co {story.Views:N0} luot xem.", result);
            }

            if (ContainsAny(normalized, "bao nhieu chuong", "so chuong", "chuong"))
            {
                return SingleStoryAnswer($"\"{story.Title}\" hien co {story.Chapters:N0} chuong.", result);
            }

            if (ContainsAny(normalized, "tac gia", "ai viet", "cua ai"))
            {
                return SingleStoryAnswer($"Tac gia cua \"{story.Title}\" la {story.Author ?? "dang cap nhat"}.", result);
            }

            if (ContainsAny(normalized, "trang thai", "hoan thanh", "dang tien hanh", "tam ngung"))
            {
                return SingleStoryAnswer($"\"{story.Title}\" dang o trang thai {story.Status}.", result);
            }

            return null;
        }

        private async Task<ChatBotResponse> BuildCurrentReadingResponse(string normalized)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ChatBotResponse
                {
                    Reply = "Ban can dang nhap de minh xem truyen ban dang doc.",
                    Links = new List<ChatBotLink> { new() { Label = "Dang nhap", Url = "/Identity/Account/Login" } }
                };
            }

            var story = await _context.LichSuDocs
                .AsNoTracking()
                .Where(x => x.MaNguoiDung == userId)
                .OrderByDescending(x => x.ThoiGianDoc)
                .Select(x => new ChatBotStoryResult
                {
                    Title = x.Chuong.Truyen.TieuDe,
                    Author = x.Chuong.Truyen.TacGia.TenTacGia,
                    Chapters = x.Chuong.Truyen.TongSoChuong,
                    Views = x.Chuong.Truyen.TongLuotXem,
                    Url = Url.Action("ChiTiet", "Truyen", new { id = x.Chuong.Truyen.MaTruyen }) ?? $"/Truyen/ChiTiet/{x.Chuong.Truyen.MaTruyen}"
                })
                .FirstOrDefaultAsync();

            if (story == null)
            {
                return new ChatBotResponse
                {
                    Reply = "Minh chua thay lich su doc cua ban nen chua biet ban dang doc truyen nao.",
                    Links = new List<ChatBotLink> { new() { Label = "Tim truyen de doc", Url = Url.Action("Index", "TimKiem") ?? "/TimKiem" } }
                };
            }

            if (ContainsAny(normalized, "luot xem", "luot doc", "view"))
            {
                return SingleStoryAnswer($"Truyen ban doc gan nhat la \"{story.Title}\", hien co {story.Views:N0} luot xem.", story);
            }

            if (ContainsAny(normalized, "bao nhieu chuong", "so chuong", "chuong"))
            {
                return SingleStoryAnswer($"Truyen ban doc gan nhat la \"{story.Title}\", hien co {story.Chapters:N0} chuong.", story);
            }

            return SingleStoryAnswer($"Truyen ban doc gan nhat la \"{story.Title}\".", story);
        }

        private async Task<ChatBotStoryResult?> GetTopStory(string metric)
        {
            var query = _context.Truyens.AsNoTracking();

            query = metric switch
            {
                "chapters" => query.OrderByDescending(t => t.TongSoChuong).ThenByDescending(t => t.TongLuotXem),
                "likes" => query.OrderByDescending(t => t.YeuThichs.Count).ThenByDescending(t => t.TongLuotXem),
                "follows" => query.OrderByDescending(t => t.TheoDoiTruyens.Count).ThenByDescending(t => t.TongLuotXem),
                "bookmarks" => query.OrderByDescending(t => t.DanhDaus.Count).ThenByDescending(t => t.TongLuotXem),
                "updated" => query.OrderByDescending(t => t.NgayCapNhat),
                _ => query.OrderByDescending(t => t.TongLuotXem).ThenByDescending(t => t.NgayCapNhat)
            };

            return await query
                .Select(t => new ChatBotStoryResult
                {
                    Title = t.TieuDe,
                    Author = t.TacGia.TenTacGia,
                    Chapters = t.TongSoChuong,
                    Views = t.TongLuotXem,
                    Url = Url.Action("ChiTiet", "Truyen", new { id = t.MaTruyen }) ?? $"/Truyen/ChiTiet/{t.MaTruyen}"
                })
                .FirstOrDefaultAsync();
        }

        private async Task<StoryFact?> FindMentionedStory(string normalized)
        {
            var stories = await _context.Truyens
                .AsNoTracking()
                .OrderByDescending(t => t.TieuDe.Length)
                .Take(500)
                .Select(t => new StoryFact
                {
                    Title = t.TieuDe,
                    NormalizedTitle = string.Empty,
                    Author = t.TacGia.TenTacGia,
                    Chapters = t.TongSoChuong,
                    Views = t.TongLuotXem,
                    Status = t.TrangThai == TrangThaiTruyen.DaHoanThanh
                        ? "da hoan thanh"
                        : t.TrangThai == TrangThaiTruyen.TamNgung
                            ? "tam ngung"
                            : "dang tien hanh",
                    Url = Url.Action("ChiTiet", "Truyen", new { id = t.MaTruyen }) ?? $"/Truyen/ChiTiet/{t.MaTruyen}"
                })
                .ToListAsync();

            foreach (var story in stories)
            {
                story.NormalizedTitle = NormalizeForMatch(story.Title);
            }

            var directMatch = stories.FirstOrDefault(story => normalized.Contains(story.NormalizedTitle));
            if (directMatch != null)
            {
                return directMatch;
            }

            var queryText = RemoveQuestionWords(normalized);
            if (queryText.Length < 2)
            {
                return null;
            }

            return stories.FirstOrDefault(story =>
                story.NormalizedTitle.Contains(queryText) ||
                queryText.Contains(story.NormalizedTitle));
        }

        private static string RemoveQuestionWords(string normalized)
        {
            var words = new[]
            {
                "truyen", "ten", "co", "bao nhieu", "may", "la", "la ai", "la gi",
                "luot xem", "luot doc", "view", "chuong", "so chuong", "tac gia",
                "ai viet", "cua ai", "cua", "trang thai", "hien tai", "dang", "khong",
                "toi", "minh", "ban", "hoi", "cho biet"
            };

            foreach (var word in words)
            {
                normalized = normalized.Replace(word, " ", StringComparison.OrdinalIgnoreCase);
            }

            return string.Join(" ", normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static bool IsCurrentReadingQuestion(string normalized)
        {
            return ContainsAny(normalized, "toi dang doc", "dang doc cua toi", "truyen toi doc", "truyen minh dang doc", "toi doc gan nhat", "dang doc");
        }

        private static ChatBotResponse SingleStoryAnswer(string reply, ChatBotStoryResult story)
        {
            return new ChatBotResponse
            {
                Reply = reply,
                Stories = new List<ChatBotStoryResult> { story },
                Links = new List<ChatBotLink> { new() { Label = "Mo truyen", Url = story.Url } }
            };
        }

        private static ChatBotResponse NoStoryResponse()
        {
            return new ChatBotResponse
            {
                Reply = "Hien chua co truyen nao trong he thong de minh tra loi cau nay."
            };
        }

        private class StoryFact
        {
            public string Title { get; set; } = string.Empty;
            public string NormalizedTitle { get; set; } = string.Empty;
            public string? Author { get; set; }
            public int Chapters { get; set; }
            public int Views { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
        }

        private async Task<ChatBotResponse?> BuildStatsResponse(string normalized)
        {
            if (!ContainsAny(normalized, "bao nhieu", "so luong", "co may", "thong ke", "tong"))
            {
                return null;
            }

            if (ContainsAny(normalized, "the loai", "tag", "nhan dan"))
            {
                var tagCount = await _context.Thes.CountAsync();
                var storyCount = await _context.Truyens.CountAsync();
                return new ChatBotResponse
                {
                    Reply = $"Hiện web có {tagCount} thể loại/nhãn và {storyCount} truyện trong hệ thống.",
                    Links = new List<ChatBotLink>
                    {
                        new() { Label = "Lọc truyện theo thể loại", Url = Url.Action("Index", "TimKiem") ?? "/TimKiem" }
                    }
                };
            }

            if (ContainsAny(normalized, "truyen", "sach"))
            {
                var totalStories = await _context.Truyens.CountAsync();
                var completedStories = await _context.Truyens.CountAsync(t => t.TrangThai == TrangThaiTruyen.DaHoanThanh);
                var updatingStories = await _context.Truyens.CountAsync(t => t.TrangThai == TrangThaiTruyen.DangTienHanh);
                return new ChatBotResponse
                {
                    Reply = $"Hiện web có {totalStories} truyện: {updatingStories} truyện đang tiến hành và {completedStories} truyện đã hoàn thành.",
                    Links = new List<ChatBotLink>
                    {
                        new() { Label = "Xem tất cả truyện", Url = Url.Action("Index", "TimKiem") ?? "/TimKiem" }
                    }
                };
            }

            if (ContainsAny(normalized, "chuong"))
            {
                var chapterCount = await _context.Chuongs.CountAsync(c => c.TrangThai == TrangThaiChuong.DaXuatBan);
                return new ChatBotResponse
                {
                    Reply = $"Hiện web có {chapterCount} chương đã xuất bản."
                };
            }

            if (ContainsAny(normalized, "tac gia"))
            {
                var authorCount = await _context.TacGias.CountAsync();
                return new ChatBotResponse
                {
                    Reply = $"Hiện web có {authorCount} tác giả."
                };
            }

            return null;
        }

        private ChatBotResponse? BuildGuideResponse(string normalized)
        {
            if (ContainsAny(normalized, "tat ca chuc nang", "co the lam gi", "ho tro gi", "huong dan web", "chuc nang web"))
            {
                return BuildFeatureOverviewResponse();
            }

            if (ContainsAny(normalized, "trang chu", "home"))
            {
                return new ChatBotResponse
                {
                    Reply = "Trang chủ giúp bạn xem truyện mới cập nhật, truyện nổi bật và các khu vực giới thiệu nhanh của HutechNovel.",
                    Links = new List<ChatBotLink> { new() { Label = "Trang chủ", Url = Url.Action("Index", "Home") ?? "/" } }
                };
            }

            if (ContainsAny(normalized, "the loai", "danh sach the", "nhan dan"))
            {
                return new ChatBotResponse
                {
                    Reply = "Bạn có thể mở menu Thể loại trên thanh điều hướng hoặc vào Lọc truyện để chọn thể loại, nhãn, trạng thái, lượt xem và số chương.",
                    Links = new List<ChatBotLink> { new() { Label = "Lọc truyện", Url = Url.Action("Index", "TimKiem") ?? "/TimKiem" } }
                };
            }

            if (ContainsAny(normalized, "tim kiem", "loc truyen", "tim truyen", "search"))
            {
                return new ChatBotResponse
                {
                    Reply = "Bạn có thể tìm truyện theo tên, tác giả, tóm tắt, thể loại, trạng thái, lượt xem và số chương. Bạn cũng có thể hỏi mình trực tiếp như: \"gợi ý truyện dị năng\".",
                    Links = new List<ChatBotLink> { new() { Label = "Lọc truyện", Url = Url.Action("Index", "TimKiem") ?? "/TimKiem" } }
                };
            }

            if (ContainsAny(normalized, "xep hang", "bang xep hang", "top", "noi bat"))
            {
                return new ChatBotResponse
                {
                    Reply = "Bảng xếp hạng giúp bạn xem truyện nổi bật theo lượt xem, xu hướng hoặc các tiêu chí phổ biến của web.",
                    Links = new List<ChatBotLink> { new() { Label = "Bảng xếp hạng", Url = Url.Action("Index", "XepHang") ?? "/XepHang" } }
                };
            }

            if (ContainsAny(normalized, "chi tiet truyen", "gioi thieu truyen", "mo ta truyen"))
            {
                return new ChatBotResponse
                {
                    Reply = "Trang chi tiết truyện có mô tả, tác giả, thể loại, danh sách chương, lượt xem, thích, đánh dấu, theo dõi, đánh giá sao và bình luận."
                };
            }

            if (ContainsAny(normalized, "doc truyen", "chuong truoc", "chuong sau", "muc luc", "tts", "doc thanh tieng", "giong doc"))
            {
                return new ChatBotResponse
                {
                    Reply = "Trang đọc truyện hỗ trợ đọc chương, chuyển chương trước/sau, về mục lục, đổi màu nền, cỡ chữ, font chữ và đọc truyện bằng giọng nói trong bảng Text to speech."
                };
            }

            if (ContainsAny(normalized, "theo doi", "follow", "chuong moi"))
            {
                return new ChatBotResponse
                {
                    Reply = "Để theo dõi truyện, mở chi tiết truyện rồi bấm Theo dõi. Truyện sẽ vào Danh sách theo dõi và được báo khi có chương mới.",
                    Links = new List<ChatBotLink> { new() { Label = "Danh sách theo dõi", Url = Url.Action("DanhSachTheoDoi", "NguoiDung") ?? "/NguoiDung/DanhSachTheoDoi" } }
                };
            }

            if (ContainsAny(normalized, "lich su", "tu truyen", "da doc", "dang doc", "doc sau"))
            {
                return new ChatBotResponse
                {
                    Reply = "Tủ truyện cá nhân lưu lịch sử đọc, truyện đang đọc, đọc sau và truyện đã hoàn thành. Bạn cần đăng nhập để dùng đầy đủ.",
                    Links = new List<ChatBotLink> { new() { Label = "Tủ truyện", Url = Url.Action("TuTruyen", "NguoiDung") ?? "/NguoiDung/TuTruyen" } }
                };
            }

            if (ContainsAny(normalized, "danh dau", "bookmark", "luu truyen"))
            {
                return new ChatBotResponse
                {
                    Reply = "Nút Đánh dấu ở trang chi tiết truyện giúp lưu truyện vào mục đọc sau trong Tủ truyện."
                };
            }

            if (ContainsAny(normalized, "thich", "like"))
            {
                return new ChatBotResponse
                {
                    Reply = "Nút Thích nằm ở trang chi tiết truyện. Khi bấm, hệ thống sẽ cập nhật lượt thích của truyện."
                };
            }

            if (ContainsAny(normalized, "ve day", "day sach", "nhan ve", "kiem ve", "diem danh"))
            {
                return new ChatBotResponse
                {
                    Reply = "Để nhận vé đẩy sách, bạn vào Hồ sơ và bấm Điểm danh nhận thưởng. Mỗi ngày điểm danh được 1 vé. Khi có vé, mở trang chi tiết truyện rồi dùng khu Đẩy sách để đẩy truyện bạn muốn.",
                    Links = new List<ChatBotLink>
                    {
                        new() { Label = "Hồ sơ", Url = Url.Action("HoSo", "NguoiDung") ?? "/NguoiDung/HoSo" },
                        new() { Label = "Bảng xếp hạng đẩy sách", Url = Url.Action("Index", "XepHang", new { type = "boost" }) ?? "/XepHang?type=boost" }
                    }
                };
            }

            if (ContainsAny(normalized, "danh gia", "sao", "rating"))
            {
                return new ChatBotResponse
                {
                    Reply = "Bạn có thể đánh giá truyện bằng sao trong trang chi tiết truyện. Điểm đánh giá sẽ được cập nhật cho truyện đó."
                };
            }

            if (ContainsAny(normalized, "binh luan", "comment", "tra loi"))
            {
                return new ChatBotResponse
                {
                    Reply = "Khu bình luận nằm dưới trang chi tiết truyện. Bạn có thể viết bình luận, trả lời người khác và xóa bình luận của mình."
                };
            }

            if (ContainsAny(normalized, "dang nhap", "dang ky", "tai khoan", "ho so", "avatar", "mat khau", "email"))
            {
                return new ChatBotResponse
                {
                    Reply = "Khu tài khoản hỗ trợ đăng nhập, đăng ký, quản lý hồ sơ, avatar, email, mật khẩu và các thống kê cá nhân.",
                    Links = new List<ChatBotLink>
                    {
                        new() { Label = "Đăng nhập", Url = "/Identity/Account/Login" },
                        new() { Label = "Quản lý tài khoản", Url = "/Identity/Account/Manage" }
                    }
                };
            }

            if (ContainsAny(normalized, "uploader", "dang truyen", "quan ly truyen", "them chuong", "kiem duyet"))
            {
                return new ChatBotResponse
                {
                    Reply = "Khu Uploader/Admin dùng để quản lý truyện, tác giả, chương, kiểm duyệt và cấu hình hệ thống. Chỉ tài khoản có quyền phù hợp mới vào được.",
                    Links = new List<ChatBotLink> { new() { Label = "Khu Uploader", Url = "/Admin/QuanLyTruyen" } }
                };
            }

            if (ContainsAny(normalized, "bao tri", "cau hinh", "thong bao web", "seo"))
            {
                return new ChatBotResponse
                {
                    Reply = "Admin có thể cấu hình tên website, SEO, thông báo toàn cục và chế độ bảo trì trong khu cấu hình hệ thống.",
                    Links = new List<ChatBotLink> { new() { Label = "Cấu hình hệ thống", Url = "/Admin/AdminCauHinh" } }
                };
            }

            return null;
        }

        private async Task<ChatBotResponse?> BuildSmartAssistantResponse(string message, string normalized)
        {
            if (ContainsAny(normalized, "goi y tag", "de xuat tag", "tag phu hop", "nhan phu hop"))
            {
                var text = ExtractAfterAny(message, "gợi ý tag", "goi y tag", "đề xuất tag", "de xuat tag", "tag phù hợp", "tag phu hop")
                    ?? message;
                var tags = await SuggestTagsFromText(text);
                return new ChatBotResponse
                {
                    Reply = tags.Any()
                        ? $"Mình gợi ý các tag phù hợp: {string.Join(", ", tags)}."
                        : "Mình chưa bắt được tag rõ ràng. Bạn có thể thêm vài ý về bối cảnh, thể loại, năng lực nhân vật hoặc nhịp truyện.",
                    Links = new List<ChatBotLink> { new() { Label = "Đăng truyện mới", Url = "/Admin/QuanLyTruyen/ThemMoi" } }
                };
            }

            if (ContainsAny(normalized, "tom tat", "noi dung chinh", "ke tom", "review nhanh"))
            {
                var story = await FindMentionedStory(normalized);
                if (story == null)
                {
                    return null;
                }

                var storyRow = await _context.Truyens
                    .AsNoTracking()
                    .Include(t => t.Chuongs.Where(c => c.TrangThai == TrangThaiChuong.DaXuatBan).OrderBy(c => c.SoChuong))
                    .FirstOrDefaultAsync(t => t.TieuDe == story.Title);

                if (storyRow == null)
                {
                    return null;
                }

                if (ContainsAny(normalized, "chuong"))
                {
                    var chapter = storyRow.Chuongs
                        .OrderBy(c => c.SoChuong)
                        .FirstOrDefault(c => normalized.Contains(c.SoChuong.ToString()))
                        ?? storyRow.Chuongs.OrderBy(c => c.SoChuong).FirstOrDefault();

                    return new ChatBotResponse
                    {
                        Reply = chapter == null
                            ? $"\"{storyRow.TieuDe}\" hiện chưa có chương đã xuất bản để tóm tắt."
                            : $"Tóm tắt nhanh chương {chapter.SoChuong} của \"{storyRow.TieuDe}\": {chapter.TieuDe}. Mình chưa đọc sâu nội dung chương ở đây, nhưng có thể mở chương để bạn xem trực tiếp.",
                        Links = chapter == null
                            ? new List<ChatBotLink> { new() { Label = "Mở truyện", Url = story.Url } }
                            : new List<ChatBotLink> { new() { Label = "Mở chương", Url = Url.Action("Index", "DocTruyen", new { maTruyen = storyRow.MaTruyen, soChuong = chapter.SoChuong }) ?? story.Url } }
                    };
                }

                var summary = string.IsNullOrWhiteSpace(storyRow.MoTa)
                    ? "Truyện chưa có tóm tắt."
                    : storyRow.MoTa.Trim();
                if (summary.Length > 420)
                {
                    summary = summary[..420].TrimEnd() + "...";
                }

                return new ChatBotResponse
                {
                    Reply = $"Tóm tắt \"{storyRow.TieuDe}\": {summary}",
                    Stories = new List<ChatBotStoryResult>
                    {
                        new()
                        {
                            Title = story.Title,
                            Author = story.Author,
                            Chapters = story.Chapters,
                            Views = story.Views,
                            Url = story.Url
                        }
                    },
                    Links = new List<ChatBotLink> { new() { Label = "Mở truyện", Url = story.Url } }
                };
            }

            if (ContainsAny(normalized, "muon truyen", "tim truyen", "goi y", "theo gu", "main", "nhan vat chinh"))
            {
                var tags = await FindTagsInMessage(normalized);
                var wantsCompleted = ContainsAny(normalized, "da hoan thanh", "hoan thanh", "full");
                var preferenceWords = ExtractPreferenceWords(normalized);

                if (!tags.Any() && !wantsCompleted && !preferenceWords.Any())
                {
                    return null;
                }

                var candidates = await _context.Truyens
                    .AsNoTracking()
                    .Include(t => t.TacGia)
                    .Include(t => t.Thes)
                    .Where(t => !wantsCompleted || t.TrangThai == TrangThaiTruyen.DaHoanThanh)
                    .OrderByDescending(t => t.TongLuotXem)
                    .ThenByDescending(t => t.NgayCapNhat)
                    .Take(200)
                    .ToListAsync();

                var tagIds = tags.Select(t => t.MaThe).ToHashSet();
                var ranked = candidates
                    .Select(story => new
                    {
                        Story = story,
                        Score = story.Thes.Count(tag => tagIds.Contains(tag.MaThe)) * 5
                            + preferenceWords.Count(word => NormalizeForMatch(story.MoTa ?? string.Empty).Contains(word)) * 3
                            + preferenceWords.Count(word => NormalizeForMatch(story.TieuDe).Contains(word))
                            + (wantsCompleted && story.TrangThai == TrangThaiTruyen.DaHoanThanh ? 2 : 0)
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Story.TongLuotXem)
                    .Take(6)
                    .Select(x => new ChatBotStoryResult
                    {
                        Title = x.Story.TieuDe,
                        Author = x.Story.TacGia?.TenTacGia,
                        Chapters = x.Story.TongSoChuong,
                        Views = x.Story.TongLuotXem,
                        Url = Url.Action("ChiTiet", "Truyen", new { id = x.Story.MaTruyen }) ?? $"/Truyen/ChiTiet/{x.Story.MaTruyen}"
                    })
                    .ToList();

                if (!ranked.Any())
                {
                    ranked = await GetPopularStories();
                }

                var taste = new List<string>();
                if (tags.Any()) taste.AddRange(tags.Select(t => t.TenThe));
                if (wantsCompleted) taste.Add("đã hoàn thành");
                taste.AddRange(preferenceWords);

                return new ChatBotResponse
                {
                    Reply = $"Mình lọc theo gu: {string.Join(", ", taste.Distinct())}. Đây là vài truyện hợp nhất trong dữ liệu hiện có.",
                    Stories = ranked,
                    Links = new List<ChatBotLink> { new() { Label = "Mở bộ lọc", Url = Url.Action("Index", "TimKiem", new { status = wantsCompleted ? (int?)((int)TrangThaiTruyen.DaHoanThanh) : null, selectedTagId = tags.FirstOrDefault()?.MaThe, sortBy = "views" }) ?? "/TimKiem" } }
                };
            }

            return null;
        }

        private async Task<ChatBotResponse?> BuildFilterResponse(string message, string normalized)
        {
            var links = new List<ChatBotLink>();
            var filterParts = new List<string>();
            int? status = null;
            string? sortBy = null;
            int? selectedTagId = null;
            string? summary = null;
            string? chapterTitle = null;
            string? keyword = null;

            var tag = await FindTagInMessage(normalized);
            if (tag != null)
            {
                selectedTagId = tag.MaThe;
                filterParts.Add($"thể loại {tag.TenThe}");
            }

            if (ContainsAny(normalized, "dang tien hanh", "chua hoan thanh", "con ra chuong"))
            {
                status = (int)TrangThaiTruyen.DangTienHanh;
                filterParts.Add("đang tiến hành");
            }
            else if (ContainsAny(normalized, "da hoan thanh", "hoan thanh", "full", "truyen full"))
            {
                status = (int)TrangThaiTruyen.DaHoanThanh;
                filterParts.Add("đã hoàn thành");
            }
            else if (ContainsAny(normalized, "tam ngung", "ngung cap nhat"))
            {
                status = (int)TrangThaiTruyen.TamNgung;
                filterParts.Add("tạm ngưng");
            }

            if (ContainsAny(normalized, "luot thich", "thich cao", "like cao"))
            {
                sortBy = "likes";
                filterParts.Add("sắp xếp theo lượt thích");
            }
            else if (ContainsAny(normalized, "luot theo doi", "theo doi cao", "follow cao"))
            {
                sortBy = "follows";
                filterParts.Add("sắp xếp theo lượt theo dõi");
            }
            else if (ContainsAny(normalized, "luot danh dau", "danh dau cao", "bookmark cao"))
            {
                sortBy = "bookmarks";
                filterParts.Add("sắp xếp theo lượt đánh dấu");
            }
            else if (ContainsAny(normalized, "luot xem ngay", "doc ngay", "view ngay"))
            {
                sortBy = "views-day";
                filterParts.Add("sắp xếp theo lượt đọc ngày");
            }
            else if (ContainsAny(normalized, "luot xem tuan", "doc tuan", "view tuan"))
            {
                sortBy = "views-week";
                filterParts.Add("sắp xếp theo lượt đọc tuần");
            }
            else if (ContainsAny(normalized, "luot xem", "luot doc", "doc nhieu", "view cao", "cao nhat"))
            {
                sortBy = "views";
                filterParts.Add("sắp xếp theo lượt đọc");
            }

            if (ContainsAny(normalized, "tom tat", "mo ta", "noi dung"))
            {
                summary = ExtractAfterAny(message, "tóm tắt", "tom tat", "mô tả", "mo ta", "nội dung", "noi dung");
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    filterParts.Add($"tóm tắt có \"{summary}\"");
                }
            }

            if (ContainsAny(normalized, "ten chuong", "tên chương", "chuong ten", "chuong co", "chuong"))
            {
                chapterTitle = ExtractAfterAny(message, "tên chương", "ten chuong", "chương", "chuong");
                if (!string.IsNullOrWhiteSpace(chapterTitle))
                {
                    filterParts.Add($"tên chương có \"{chapterTitle}\"");
                }
            }

            var isExplicitFilter = ContainsAny(
                normalized,
                "loc", "tim theo", "sap xep", "cao nhat", "dang tien hanh", "hoan thanh",
                "tam ngung", "tom tat", "mo ta", "ten chuong", "the loai", "luot thich",
                "luot theo doi", "luot xem", "luot doc", "danh dau");

            if (!isExplicitFilter || (!status.HasValue && sortBy == null && selectedTagId == null && summary == null && chapterTitle == null))
            {
                return null;
            }

            if (summary == null && chapterTitle == null && selectedTagId == null && !status.HasValue && sortBy == null)
            {
                keyword = CleanSearchText(message);
            }

            var url = Url.Action("Index", "TimKiem", new
            {
                keyword,
                summary,
                chapterTitle,
                status,
                selectedTagId,
                sortBy = sortBy ?? "updated"
            }) ?? "/TimKiem";

            links.Add(new ChatBotLink { Label = "Mở trang lọc đã chọn", Url = url });

            if (ContainsAny(normalized, "luot thich") && ContainsAny(normalized, "theo doi"))
            {
                links.Add(new ChatBotLink
                {
                    Label = "Sắp xếp lượt thích",
                    Url = Url.Action("Index", "TimKiem", new { status, selectedTagId, sortBy = "likes" }) ?? "/TimKiem?sortBy=likes"
                });
                links.Add(new ChatBotLink
                {
                    Label = "Sắp xếp lượt theo dõi",
                    Url = Url.Action("Index", "TimKiem", new { status, selectedTagId, sortBy = "follows" }) ?? "/TimKiem?sortBy=follows"
                });
            }

            var stories = await GetFilteredPreview(keyword, summary, chapterTitle, status, selectedTagId, sortBy);
            return new ChatBotResponse
            {
                Reply = filterParts.Any()
                    ? $"Mình đã chuẩn bị bộ lọc: {string.Join(", ", filterParts)}. Bấm nút bên dưới để mở trang Lọc truyện với sẵn điều kiện."
                    : "Mình đã chuẩn bị trang Lọc truyện theo yêu cầu của bạn.",
                Stories = stories,
                Links = links
            };
        }

        private async Task<ChatBotResponse> BuildStorySearchResponse(string message, string normalized)
        {
            var tag = await FindTagInMessage(normalized);
            if (tag != null)
            {
                var taggedStories = await GetStoriesByTag(tag.MaThe);
                if (taggedStories.Any())
                {
                    return new ChatBotResponse
                    {
                        Reply = $"Mình gợi ý cho bạn {taggedStories.Count} truyện thuộc thể loại/nhãn \"{tag.TenThe}\".",
                        Stories = taggedStories,
                        Links = new List<ChatBotLink>
                        {
                            new() { Label = $"Xem thêm {tag.TenThe}", Url = (Url.Action("Index", "TimKiem", new { selectedTagId = tag.MaThe }) ?? "/TimKiem") }
                        }
                    };
                }
            }

            var queryText = CleanSearchText(message);
            var response = await TryFindStoriesByText(queryText, NormalizeForMatch(queryText));
            if (response.Stories.Any())
            {
                response.Reply = $"Mình tìm thấy {response.Stories.Count} truyện liên quan đến \"{queryText}\".";
                return response;
            }

            var suggestions = await GetPopularStories();
            return new ChatBotResponse
            {
                Reply = $"Mình chưa tìm thấy truyện khớp với \"{queryText}\". Bạn có thể thử tên khác, tên tác giả, hoặc vào trang Lọc truyện để lọc chi tiết hơn.",
                Stories = suggestions,
                Links = new List<ChatBotLink> { new() { Label = "Lọc truyện", Url = Url.Action("Index", "TimKiem") ?? "/TimKiem" } }
            };
        }

        private async Task<ChatBotResponse> TryFindStoriesByText(string queryText, string normalizedQuery)
        {
            if (string.IsNullOrWhiteSpace(queryText) || normalizedQuery.Length < 2)
            {
                return new ChatBotResponse();
            }

            var stories = await _context.Truyens
                .AsNoTracking()
                .Where(t =>
                    t.TieuDe.Contains(queryText) ||
                    t.MoTa.Contains(queryText) ||
                    t.TacGia.TenTacGia.Contains(queryText) ||
                    t.Chuongs.Any(c => c.TieuDe.Contains(queryText)) ||
                    t.Thes.Any(tag => tag.TenThe.Contains(queryText)))
                .OrderByDescending(t => t.TieuDe.Contains(queryText))
                .ThenByDescending(t => t.NgayCapNhat)
                .Take(8)
                .Select(t => new ChatBotStoryResult
                {
                    Title = t.TieuDe,
                    Author = t.TacGia.TenTacGia,
                    Chapters = t.TongSoChuong,
                    Views = t.TongLuotXem,
                    Url = Url.Action("ChiTiet", "Truyen", new { id = t.MaTruyen }) ?? $"/Truyen/ChiTiet/{t.MaTruyen}"
                })
                .ToListAsync();

            if (!stories.Any())
            {
                var allCandidates = await _context.Truyens
                    .AsNoTracking()
                    .OrderByDescending(t => t.NgayCapNhat)
                    .Take(500)
                    .Select(t => new
                    {
                        t.MaTruyen,
                        t.TieuDe,
                        t.MoTa,
                        Author = t.TacGia.TenTacGia,
                        t.TongSoChuong,
                        t.TongLuotXem,
                        ChapterTitles = t.Chuongs
                            .Where(c => c.TrangThai == TrangThaiChuong.DaXuatBan)
                            .Select(c => c.TieuDe)
                            .Take(12)
                            .ToList()
                    })
                    .ToListAsync();

                stories = allCandidates
                    .Where(t =>
                        NormalizeForMatch(t.TieuDe).Contains(normalizedQuery) ||
                        NormalizeForMatch(t.MoTa).Contains(normalizedQuery) ||
                        t.ChapterTitles.Any(title => NormalizeForMatch(title).Contains(normalizedQuery)) ||
                        normalizedQuery.Contains(NormalizeForMatch(t.TieuDe)))
                    .Take(5)
                    .Select(t => new ChatBotStoryResult
                    {
                        Title = t.TieuDe,
                        Author = t.Author,
                        Chapters = t.TongSoChuong,
                        Views = t.TongLuotXem,
                        Url = Url.Action("ChiTiet", "Truyen", new { id = t.MaTruyen }) ?? $"/Truyen/ChiTiet/{t.MaTruyen}"
                    })
                    .ToList();
            }

            return new ChatBotResponse { Stories = stories };
        }

        private async Task<List<ChatBotStoryResult>> GetFilteredPreview(
            string? keyword,
            string? summary,
            string? chapterTitle,
            int? status,
            int? selectedTagId,
            string? sortBy)
        {
            var query = _context.Truyens.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => t.TieuDe.Contains(keyword) || t.TacGia.TenTacGia.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(summary))
            {
                query = query.Where(t => t.MoTa.Contains(summary));
            }

            if (!string.IsNullOrWhiteSpace(chapterTitle))
            {
                query = query.Where(t => t.Chuongs.Any(c => c.TieuDe.Contains(chapterTitle)));
            }

            if (status.HasValue)
            {
                query = query.Where(t => (int)t.TrangThai == status.Value);
            }

            if (selectedTagId.HasValue)
            {
                query = query.Where(t => t.Thes.Any(tag => tag.MaThe == selectedTagId.Value));
            }

            query = sortBy switch
            {
                "views" => query.OrderByDescending(t => t.TongLuotXem),
                "views-day" => query.OrderByDescending(t => t.LuotXems.Count(v => v.ThoiGianXem >= DateTime.Today)),
                "views-week" => query.OrderByDescending(t => t.LuotXems.Count(v => v.ThoiGianXem >= DateTime.Today.AddDays(-6))),
                "likes" => query.OrderByDescending(t => t.YeuThichs.Count),
                "follows" => query.OrderByDescending(t => t.TheoDoiTruyens.Count),
                "bookmarks" => query.OrderByDescending(t => t.DanhDaus.Count),
                _ => query.OrderByDescending(t => t.NgayCapNhat)
            };

            return await query
                .Take(5)
                .Select(t => new ChatBotStoryResult
                {
                    Title = t.TieuDe,
                    Author = t.TacGia.TenTacGia,
                    Chapters = t.TongSoChuong,
                    Views = t.TongLuotXem,
                    Url = Url.Action("ChiTiet", "Truyen", new { id = t.MaTruyen }) ?? $"/Truyen/ChiTiet/{t.MaTruyen}"
                })
                .ToListAsync();
        }

        private async Task<The?> FindTagInMessage(string normalized)
        {
            var tags = await _context.Thes.AsNoTracking().ToListAsync();
            return tags
                .OrderByDescending(tag => tag.TenThe.Length)
                .FirstOrDefault(tag => normalized.Contains(NormalizeForMatch(tag.TenThe)));
        }

        private async Task<List<The>> FindTagsInMessage(string normalized)
        {
            var tags = await _context.Thes.AsNoTracking().ToListAsync();
            return tags
                .Where(tag => normalized.Contains(NormalizeForMatch(tag.TenThe)))
                .OrderByDescending(tag => tag.TenThe.Length)
                .Take(5)
                .ToList();
        }

        private async Task<List<string>> SuggestTagsFromText(string text)
        {
            var normalized = NormalizeForMatch(text);
            var tags = await FindTagsInMessage(normalized);
            var result = tags.Select(t => t.TenThe).ToList();

            var rules = new Dictionary<string, string[]>
            {
                ["Game"] = new[] { "game", "tro choi", "e sport", "he thong", "level" },
                ["Đô thị"] = new[] { "do thi", "thanh pho", "hoc duong", "cong ty" },
                ["Dị năng"] = new[] { "di nang", "sieu nang luc", "nang luc", "thuc tinh" },
                ["Linh dị"] = new[] { "linh di", "ma", "quy", "kinh di", "tam linh" },
                ["Khoa học viễn tưởng"] = new[] { "khoa hoc", "vien tuong", "tuong lai", "robot", "ai" },
                ["Ngôn tình"] = new[] { "ngon tinh", "tinh cam", "yeu", "lang man" },
                ["Light Novel"] = new[] { "light novel", "nhat ban", "isekai", "hoc vien" }
            };

            foreach (var rule in rules)
            {
                if (result.Contains(rule.Key)) continue;
                if (rule.Value.Any(normalized.Contains))
                {
                    result.Add(rule.Key);
                }
            }

            return result.Distinct().Take(6).ToList();
        }

        private static List<string> ExtractPreferenceWords(string normalized)
        {
            var candidates = new[]
            {
                "main thong minh", "thong minh", "game", "he thong", "hai huoc", "nghien tuc",
                "manh", "ba dao", "chien thuat", "doi tri", "hoc duong", "do thi", "phieu luu",
                "nhanh", "cham", "it drama", "khong nguoc", "tinh cam", "linh di"
            };

            return candidates
                .Where(normalized.Contains)
                .Distinct()
                .ToList();
        }

        private async Task<List<ChatBotStoryResult>> GetStoriesByTag(int tagId)
        {
            return await _context.Truyens
                .AsNoTracking()
                .Where(t => t.Thes.Any(tag => tag.MaThe == tagId))
                .OrderByDescending(t => t.NgayCapNhat)
                .Take(6)
                .Select(t => new ChatBotStoryResult
                {
                    Title = t.TieuDe,
                    Author = t.TacGia.TenTacGia,
                    Chapters = t.TongSoChuong,
                    Views = t.TongLuotXem,
                    Url = Url.Action("ChiTiet", "Truyen", new { id = t.MaTruyen }) ?? $"/Truyen/ChiTiet/{t.MaTruyen}"
                })
                .ToListAsync();
        }

        private async Task<List<ChatBotStoryResult>> GetPopularStories()
        {
            return await _context.Truyens
                .AsNoTracking()
                .OrderByDescending(t => t.TongLuotXem)
                .ThenByDescending(t => t.NgayCapNhat)
                .Take(4)
                .Select(t => new ChatBotStoryResult
                {
                    Title = t.TieuDe,
                    Author = t.TacGia.TenTacGia,
                    Chapters = t.TongSoChuong,
                    Views = t.TongLuotXem,
                    Url = Url.Action("ChiTiet", "Truyen", new { id = t.MaTruyen }) ?? $"/Truyen/ChiTiet/{t.MaTruyen}"
                })
                .ToListAsync();
        }

        private ChatBotResponse BuildFeatureOverviewResponse()
        {
            return new ChatBotResponse
            {
                Reply = "Mình hỗ trợ bạn dùng các tính năng của web: tìm/lọc truyện, xem thể loại, bảng xếp hạng, đọc truyện, nghe đọc TTS, theo dõi chương mới, đánh dấu đọc sau, thích, đánh giá sao, bình luận, lịch sử đọc, quản lý tài khoản và khu Uploader/Admin nếu bạn có quyền. Bạn hỏi tên chức năng hoặc tên truyện là được.",
                Links = new List<ChatBotLink>
                {
                    new() { Label = "Lọc truyện", Url = Url.Action("Index", "TimKiem") ?? "/TimKiem" },
                    new() { Label = "Bảng xếp hạng", Url = Url.Action("Index", "XepHang") ?? "/XepHang" },
                    new() { Label = "Tủ truyện", Url = Url.Action("TuTruyen", "NguoiDung") ?? "/NguoiDung/TuTruyen" }
                }
            };
        }

        private static bool IsStorySearchIntent(string normalized)
        {
            return ContainsAny(
                normalized,
                "tim", "kiem", "truyen", "goi y", "de xuat", "nen doc", "the loai", "tac gia", "tuong tu");
        }

        private static string CleanSearchText(string message)
        {
            var removableWords = new[]
            {
                "tim truyen", "tim", "kiem", "truyen", "goi y", "de xuat", "cho toi", "giup toi",
                "vai", "may", "bo", "cuon", "thuoc", "lien quan", "the loai", "the", "loai",
                "nen doc", "di", "voi", "nhe"
            };

            var normalized = NormalizeForMatch(message);
            foreach (var word in removableWords)
            {
                normalized = normalized.Replace(word, " ", StringComparison.OrdinalIgnoreCase);
            }

            normalized = string.Join(" ", normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(normalized) ? message.Trim() : normalized;
        }

        private static string? ExtractAfterAny(string message, params string[] markers)
        {
            var normalized = NormalizeForMatch(message);
            foreach (var marker in markers)
            {
                var normalizedMarker = NormalizeForMatch(marker);
                var index = normalized.IndexOf(normalizedMarker, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    continue;
                }

                var value = message.Length > index + marker.Length
                    ? message[(index + marker.Length)..]
                    : string.Empty;

                value = value.Trim(' ', '.', ',', ':', ';', '?', '!', '"', '\'');
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return null;
        }

        private static string NormalizeForMatch(string text)
        {
            var normalized = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(character == 'đ' ? 'd' : character);
            }

            return builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("?", " ")
                .Replace(".", " ")
                .Replace(",", " ")
                .Replace(":", " ")
                .Replace(";", " ")
                .Trim();
        }

        private static bool ContainsAny(string text, params string[] values)
        {
            return values.Any(text.Contains);
        }
    }

    public class ChatBotRequest
    {
        public string? Message { get; set; }
    }

    public class ChatBotResponse
    {
        public string Reply { get; set; } = string.Empty;
        public List<ChatBotStoryResult> Stories { get; set; } = new();
        public List<ChatBotLink> Links { get; set; } = new();
    }

    public class ChatBotStoryResult
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public int Chapters { get; set; }
        public int Views { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class ChatBotLink
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
