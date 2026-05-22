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
1. Bạn có thể sử dụng các CÔNG CỤ (Tools) được cung cấp để tra cứu cơ sở dữ liệu (Tìm truyện, Lấy tóm tắt).
2. Luôn chủ động GỌI HÀM (Function Call) nếu người dùng yêu cầu tìm truyện theo tên, thể loại, trạng thái, lượt xem, hoặc yêu cầu xem tóm tắt truyện.
3. KHI NHẮC ĐẾN TÊN TRUYỆN, LUÔN LUÔN tạo link Markdown có dạng `[Tên Truyện](/Truyen/ChiTiet/{MaTruyen})` để người dùng có thể bấm vào.
4. KHÔNG ĐƯỢC tự bịa đặt thông tin truyện. Nếu hàm trả về không có kết quả, hãy nói không tìm thấy.
5. Từ chối trả lời các câu hỏi ngoài lề (toán, code, thời tiết...).

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
