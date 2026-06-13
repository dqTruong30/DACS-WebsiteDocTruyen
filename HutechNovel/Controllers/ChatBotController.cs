using System;
using System.Security.Claims;
using HutechNovel.Data;
using HutechNovel.Models;
using HutechNovel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace HutechNovel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGeminiService _geminiService;

        public ChatBotController(ApplicationDbContext context, IGeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] ChatBotRequest request)
        {
            var message = request.Message?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(message))
            {
                return Ok(new ChatBotResponse { Reply = "Bạn muốn mình hỗ trợ phần nào về HutechNovel?" });
            }

            // 1. Thông tin cơ bản (Context) luôn cung cấp
            var contextBuilder = new StringBuilder();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var lastRead = await _context.LichSuDocs
                    .Include(x => x.Chuong.Truyen)
                    .Where(x => x.MaNguoiDung == userId)
                    .OrderByDescending(x => x.ThoiGianDoc)
                    .FirstOrDefaultAsync();

                if (lastRead != null)
                {
                    contextBuilder.AppendLine($"- Người dùng đang hỏi bạn hiện đang đọc truyện: {lastRead.Chuong.Truyen.TieuDe}. Link: /Truyen/ChiTiet/{lastRead.Chuong.Truyen.MaTruyen}");
                }
            }

            var systemPrompt = @"Bạn là Trợ lý AI Độc Quyền của website đọc truyện chữ HutechNovel.
QUY TẮC BẮT BUỘC:
1. Bạn có thể sử dụng các CÔNG CỤ (Tools) được cung cấp để tra cứu cơ sở dữ liệu (Tìm truyện, Lấy tóm tắt, Tìm kiếm nội dung bên trong truyện).
2. Luôn chủ động GỌI HÀM (Function Call) nếu người dùng yêu cầu tìm truyện theo tên, thể loại, trạng thái, hoặc yêu cầu xem tóm tắt truyện.
3. Nếu người dùng yêu cầu tìm kiếm nội dung, đoạn trích, hoặc chi tiết BÊN TRONG truyện mà CHƯA cung cấp tên truyện, BẮT BUỘC phải hỏi lại người dùng là muốn tìm trong truyện nào. Tuyệt đối không tự tìm kiếm hay gọi hàm TimKiemNoiDungTruyen nếu chưa biết tên truyện.
4. KHI NHẮC ĐẾN TÊN TRUYỆN, LUÔN LUÔN tạo link Markdown có dạng `[Tên Truyện](/Truyen/ChiTiet/{MaTruyen})`.
5. KHI TRẢ VỀ KẾT QUẢ CHƯƠNG TRUYỆN, LUÔN LUÔN tạo link Markdown có dạng `[Tên Chương](/DocTruyen/{MaTruyen}/{SoChuong})`.
6. KHÔNG ĐƯỢC tự bịa đặt thông tin. Nếu hàm trả về không có kết quả, hãy nói không tìm thấy.
7. Từ chối trả lời các câu hỏi ngoài lề (toán, code, thời tiết...).

DỮ LIỆU BỔ SUNG:
" + contextBuilder.ToString();

            // 2. Định nghĩa các công cụ (Tools)
            var tools = new
            {
                function_declarations = new object[]
                {
                    new
                    {
                        name = "TimKiemTruyen",
                        description = "Tìm kiếm truyện trong hệ thống theo tên, thể loại hoặc trạng thái.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                tuKhoa = new { type = "STRING", description = "Từ khóa tên truyện cần tìm (tùy chọn)." },
                                theLoai = new { type = "STRING", description = "Thể loại truyện (ví dụ: Ngôn tình, Đô thị, Tiên hiệp) (tùy chọn)." },
                                trangThai = new { type = "STRING", description = "Trạng thái truyện: 'Đang tiến hành', 'Đã hoàn thành', 'Tạm ngừng' (tùy chọn)." }
                            }
                        }
                    },
                    new
                    {
                        name = "LayThongTinTruyen",
                        description = "Lấy thông tin chi tiết và tóm tắt (mô tả) của một truyện cụ thể.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                tenTruyen = new { type = "STRING", description = "Tên chính xác của bộ truyện cần xem." }
                            },
                            required = new[] { "tenTruyen" }
                        }
                    },
                    new
                    {
                        name = "TimKiemNoiDungTruyen",
                        description = "Tìm kiếm một đoạn văn bản, câu nói hoặc chi tiết cụ thể bên trong các chương của một bộ truyện.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                tuKhoa = new { type = "STRING", description = "Đoạn văn bản, chi tiết hoặc câu nói cần tìm." },
                                tenTruyen = new { type = "STRING", description = "Tên truyện cần tìm kiếm bên trong." }
                            },
                            required = new[] { "tuKhoa", "tenTruyen" }
                        }
                    }
                }
            };

            // 3. Hàm xử lý (Handler) khi AI gọi Tool
            Func<string?, JsonElement, Task<object>> toolHandler = async (functionName, args) =>
            {
                try
                {
                    if (functionName == "TimKiemTruyen")
                    {
                        string? tuKhoa = args.TryGetProperty("tuKhoa", out var tk) ? tk.GetString() : null;
                        string? theLoai = args.TryGetProperty("theLoai", out var tl) ? tl.GetString() : null;
                        string? trangThaiStr = args.TryGetProperty("trangThai", out var tt) ? tt.GetString() : null;

                        var query = _context.Truyens.AsQueryable();

                        if (!string.IsNullOrEmpty(tuKhoa))
                            query = query.Where(t => t.TieuDe.Contains(tuKhoa));

                        if (!string.IsNullOrEmpty(theLoai))
                            query = query.Where(t => t.Thes.Any(x => x.TenThe.Contains(theLoai)));

                        if (!string.IsNullOrEmpty(trangThaiStr))
                        {
                            if (trangThaiStr.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase))
                                query = query.Where(t => t.TrangThai == TrangThaiTruyen.DaHoanThanh);
                            else if (trangThaiStr.Contains("tạm ngừng", StringComparison.OrdinalIgnoreCase))
                                query = query.Where(t => t.TrangThai == TrangThaiTruyen.TamNgung);
                            else
                                query = query.Where(t => t.TrangThai == TrangThaiTruyen.DangTienHanh);
                        }

                        var results = await query.OrderByDescending(t => t.TongLuotXem)
                                                 .Take(5)
                                                 .Select(t => new { t.MaTruyen, t.TieuDe, t.TacGia.TenTacGia, t.TongLuotXem, t.TongSoChuong })
                                                 .ToListAsync();

                        if (!results.Any()) return new { message = "Không tìm thấy truyện nào phù hợp." };
                        return results;
                    }
                    else if (functionName == "LayThongTinTruyen")
                    {
                        string? tenTruyen = args.TryGetProperty("tenTruyen", out var t) ? t.GetString() : null;
                        if (string.IsNullOrEmpty(tenTruyen)) return new { message = "Vui lòng cung cấp tên truyện." };

                        var truyen = await _context.Truyens
                            .Include(x => x.TacGia)
                            .Include(x => x.Thes)
                            .FirstOrDefaultAsync(x => x.TieuDe.Contains(tenTruyen));

                        if (truyen == null) return new { message = "Không tìm thấy truyện có tên này." };

                        return new
                        {
                            MaTruyen = truyen.MaTruyen,
                            TieuDe = truyen.TieuDe,
                            TacGia = truyen.TacGia.TenTacGia,
                            TheLoai = string.Join(", ", truyen.Thes.Select(x => x.TenThe)),
                            MoTa = truyen.MoTa,
                            LuotXem = truyen.TongLuotXem,
                            SoChuong = truyen.TongSoChuong,
                            TrangThai = truyen.TrangThai.ToString()
                        };
                    }
                    else if (functionName == "TimKiemNoiDungTruyen")
                    {
                        string? tuKhoa = args.TryGetProperty("tuKhoa", out var tk) ? tk.GetString() : null;
                        string? tenTruyen = args.TryGetProperty("tenTruyen", out var t) ? t.GetString() : null;
                        
                        if (string.IsNullOrEmpty(tuKhoa) || string.IsNullOrEmpty(tenTruyen)) 
                            return new { message = "Vui lòng cung cấp đủ từ khóa và tên truyện." };

                        var truyen = await _context.Truyens
                            .FirstOrDefaultAsync(x => x.TieuDe.Contains(tenTruyen));

                        if (truyen == null) return new { message = $"Không tìm thấy truyện nào có tên chứa '{tenTruyen}' để tìm kiếm nội dung." };

                        var query = _context.NoiDungChuongs
                            .Include(x => x.Chuong)
                            .Where(x => x.Chuong.MaTruyen == truyen.MaTruyen && x.NoiDung.Contains(tuKhoa));

                        var results = await query.OrderBy(x => x.Chuong.SoChuong)
                                                 .Take(3)
                                                 .Select(x => new 
                                                 { 
                                                     MaTruyen = truyen.MaTruyen,
                                                     SoChuong = x.Chuong.SoChuong, 
                                                     TieuDeChuong = x.Chuong.TieuDe,
                                                     TrichDoan = x.NoiDung.Length > 200 && x.NoiDung.IndexOf(tuKhoa) != -1
                                                         ? x.NoiDung.Substring(Math.Max(0, x.NoiDung.IndexOf(tuKhoa) - 50), Math.Min(200, x.NoiDung.Length - Math.Max(0, x.NoiDung.IndexOf(tuKhoa) - 50))) + "..."
                                                         : x.NoiDung
                                                 })
                                                 .ToListAsync();

                        if (!results.Any()) return new { message = $"Không tìm thấy đoạn văn nào chứa '{tuKhoa}' trong truyện '{truyen.TieuDe}'." };
                        
                        return results;
                    }
                }
                catch (Exception ex)
                {
                    return new { error = ex.Message };
                }

                return new { error = "Function not implemented." };
            };

            // 4. Gọi AI
            var reply = await _geminiService.GenerateContentAsync(message, systemPrompt, tools, toolHandler);

            return Ok(new ChatBotResponse
            {
                Reply = reply
            });
        }
    }

    public class ChatBotRequest
    {
        public string? Message { get; set; }
    }

    public class ChatBotResponse
    {
        public string? Reply { get; set; }
    }
}
