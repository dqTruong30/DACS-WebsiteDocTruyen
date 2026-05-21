using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Services
{
    public class TrendingScoreUpdaterService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TrendingScoreUpdaterService> _logger;

        public TrendingScoreUpdaterService(IServiceScopeFactory scopeFactory, ILogger<TrendingScoreUpdaterService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Trì hoãn 1 chút lúc khởi động để không ảnh hưởng đến startup time
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateTrendingScores(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật điểm thịnh hành (Trending Score).");
                }

                // Cập nhật mỗi 10 phút
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task UpdateTrendingScores(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;
            var sevenDaysAgo = now.AddDays(-7);

            var stories = await context.Truyens.ToListAsync(cancellationToken);

            foreach (var story in stories)
            {
                // Tính toán điểm trending
                // Sử dụng Lượt xem 7 ngày gần nhất, Lượt đẩy 7 ngày gần nhất, và Điểm đánh giá
                // Để đảm bảo không bị 0 điểm nếu truyện cũ, cộng thêm Tổng lượt xem nhân với hệ số nhỏ
                
                var recentViews = await context.LuotXems
                    .CountAsync(l => l.MaTruyen == story.MaTruyen && l.ThoiGianXem >= sevenDaysAgo, cancellationToken);
                
                var recentBoosts = await context.DayTruyens
                    .CountAsync(d => d.MaTruyen == story.MaTruyen && d.NgayTao >= sevenDaysAgo, cancellationToken);

                var totalBoosts = await context.DayTruyens
                    .CountAsync(d => d.MaTruyen == story.MaTruyen, cancellationToken);

                double newScore = (story.TongLuotXem * 0.1) 
                                + (recentViews * 1.0) 
                                + (totalBoosts * 2.0)
                                + (recentBoosts * 10.0) 
                                + (story.DiemDanhGiaTrungBinh * 5.0);
                
                story.DiemTrending = newScore;
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Đã cập nhật điểm thịnh hành cho {stories.Count} truyện lúc {DateTime.Now}");
        }
    }
}
