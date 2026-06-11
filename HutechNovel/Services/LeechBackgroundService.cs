using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HutechNovel.Services
{
    public class LeechBackgroundService : BackgroundService
    {
        private readonly ILeechTaskQueue _taskQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LeechBackgroundService> _logger;

        public LeechBackgroundService(ILeechTaskQueue taskQueue, IServiceProvider serviceProvider, ILogger<LeechBackgroundService> logger)
        {
            _taskQueue = taskQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var request = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var leechService = scope.ServiceProvider.GetRequiredService<ILeechService>();

                        // Khởi tạo Tiến trình
                        var tienTrinh = new TienTrinhLeech
                        {
                            MaTruyen = request.TruyenId,
                            UrlHienTai = request.StartUrl,
                            SoChuongDaCao = 0,
                            TrangThai = "DangChay",
                            NgayBatDau = DateTime.Now
                        };
                        context.TienTrinhLeeches.Add(tienTrinh);
                        await context.SaveChangesAsync(stoppingToken);

                        string currentUrl = request.StartUrl;
                        int failedAttempts = 0;

                        while (!string.IsNullOrEmpty(currentUrl) && failedAttempts < 3 && !stoppingToken.IsCancellationRequested && (request.MaxChapters == null || tienTrinh.SoChuongDaCao < request.MaxChapters))
                        {
                            var result = await leechService.PreviewChapterAsync(currentUrl, request.TitleSelector, request.ContentSelector, request.NextSelector);
                            
                            if (result.Success && !string.IsNullOrEmpty(result.Title))
                            {
                                int nextChapterNumber = 1;
                                if (context.Chuongs.Any(c => c.MaTruyen == request.TruyenId))
                                {
                                    nextChapterNumber = context.Chuongs.Where(c => c.MaTruyen == request.TruyenId).Max(c => c.SoChuong) + 1;
                                }

                                var newChapter = new Chuong
                                {
                                    MaTruyen = request.TruyenId,
                                    TieuDe = result.Title,
                                    SoChuong = nextChapterNumber,
                                    TrangThai = TrangThaiChuong.BanNhap, // LƯU DƯỚI DẠNG BẢN NHÁP
                                    NgayTao = DateTime.Now
                                };
                                
                                newChapter.NoiDungChuongs.Add(new NoiDungChuong
                                {
                                    NoiDung = result.ContentHtml,
                                    LoaiNoiDung = LoaiNoiDungChuong.BanGoc
                                });

                                context.Chuongs.Add(newChapter);
                                
                                tienTrinh.SoChuongDaCao++;
                                tienTrinh.UrlHienTai = currentUrl;
                                await context.SaveChangesAsync(stoppingToken);

                                currentUrl = result.NextUrl;
                                failedAttempts = 0;
                            }
                            else
                            {
                                failedAttempts++;
                                _logger.LogWarning($"Lỗi khi cào từ URL: {currentUrl}. Số lần thử: {failedAttempts}");
                            }

                            // Chờ 2s để tránh quá tải web đích
                            await Task.Delay(2000, stoppingToken);
                        }

                        tienTrinh.TrangThai = failedAttempts >= 3 ? "Loi" : "HoanThanh";
                        if (failedAttempts >= 3) tienTrinh.ThongBaoLoi = "Dừng do lỗi 3 lần liên tiếp hoặc không tìm thấy trang kế tiếp.";
                        tienTrinh.NgayKetThuc = DateTime.Now;
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi nghiêm trọng trong Background Task Leech.");
                }
            }
        }
    }
}
