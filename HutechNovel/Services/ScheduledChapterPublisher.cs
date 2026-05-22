using HutechNovel.Data;
using HutechNovel.Models;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Services
{
    public class ScheduledChapterPublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScheduledChapterPublisher> _logger;

        public ScheduledChapterPublisher(IServiceScopeFactory scopeFactory, ILogger<ScheduledChapterPublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishDueChapters(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not publish scheduled chapters.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task PublishDueChapters(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.Now;

            var dueChapters = await context.Chuongs
                .Where(c => c.TrangThai == TrangThaiChuong.HenGio && c.NgayHenGio != null && c.NgayHenGio <= now)
                .ToListAsync(cancellationToken);

            if (!dueChapters.Any())
            {
                return;
            }

            var storyIds = dueChapters.Select(c => c.MaTruyen).Distinct().ToList();
            foreach (var chapter in dueChapters)
            {
                chapter.TrangThai = TrangThaiChuong.DaXuatBan;
                chapter.NgayTao = now;
                chapter.NgayHenGio = null;
            }

            var stories = await context.Truyens
                .Where(t => storyIds.Contains(t.MaTruyen))
                .ToListAsync(cancellationToken);

            foreach (var story in stories)
            {
                story.TongSoChuong = await context.Chuongs
                    .CountAsync(c => c.MaTruyen == story.MaTruyen, cancellationToken);
                story.NgayCapNhat = now;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
