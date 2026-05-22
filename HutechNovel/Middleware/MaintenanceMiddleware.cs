using HutechNovel.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HutechNovel.Middleware
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Hàm này sẽ chạy mỗi khi có BẤT KỲ request nào gửi đến Server
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // 1. Kiểm tra cấu hình trong DB
            var config = await dbContext.CauHinhHeThongs.FirstOrDefaultAsync();
            bool isMaintenance = config != null && config.CheDoBaoTri;

            if (isMaintenance)
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";

                // 2. CHỪA ĐƯỜNG LÙI CHO ADMIN (Cực kỳ quan trọng)
                // Cho phép đi qua nếu là khu vực Admin, trang Login Admin, hoặc các file CSS/JS để load giao diện
                if (path.StartsWith("/admin") ||
                    path.StartsWith("/hutech-gate") ||
                    path.StartsWith("/css") ||
                    path.StartsWith("/js") ||
                    path.StartsWith("/lib") ||
                    path.StartsWith("/images"))
                {
                    await _next(context); // Cho phép đi qua trạm gác
                    return;
                }

                // 3. Nếu là User bình thường (Public), trả về trang giao diện Bảo trì Cyberpunk
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/html; charset=utf-8";

                var html = @"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Hệ thống đang bảo trì — HutechNovel</title>
                    <style>
                        body { background: #111; color: #35ff61; font-family: 'Inter', sans-serif; display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; margin: 0; text-align: center; overflow: hidden; }
                        h1 { font-size: 3.5rem; text-shadow: 0 0 20px rgba(53, 255, 97, 0.6); text-transform: uppercase; margin-bottom: 10px; letter-spacing: 2px; font-weight: 800; }
                        p { color: #a4a4a4; font-size: 1.2rem; max-width: 600px; line-height: 1.6; font-weight: 500; }
                        .loader { border: 4px solid rgba(53, 255, 97, 0.1); border-top: 4px solid #35ff61; border-radius: 50%; width: 50px; height: 50px; animation: spin 1s linear infinite; margin-top: 40px; box-shadow: 0 0 15px rgba(53, 255, 97, 0.3); }
                        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
                    </style>
                </head>
                <body>
                    <h1>HỆ THỐNG ĐANG BẢO TRÌ</h1>
                    <p>HutechNovel hiện đang trong quá trình nâng cấp định kỳ để mang lại trải nghiệm đọc truyện tốt hơn. Vui lòng quay lại sau ít phút.</p>
                    <div class='loader'></div>
                </body>
                </html>";

                await context.Response.WriteAsync(html);
                return; // Cắt đứt request tại đây, không cho vào Controller Public
            }

            // Nếu không bảo trì, cho phép hệ thống hoạt động bình thường
            await _next(context);
        }
    }
}