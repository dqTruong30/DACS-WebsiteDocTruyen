using PuppeteerSharp;
using System;
using System.Threading.Tasks;

namespace HutechNovel.Services
{
    public interface IPuppeteerProvider
    {
        Task<IBrowser> GetBrowserAsync();
    }

    public class PuppeteerProvider : IPuppeteerProvider, IDisposable
    {
        private IBrowser? _browser;
        private readonly object _lock = new object();
        private bool _isDownloading = false;
        private TaskCompletionSource<bool>? _downloadTask;

        public async Task<IBrowser> GetBrowserAsync()
        {
            if (_browser != null && !_browser.IsClosed)
            {
                return _browser;
            }

            TaskCompletionSource<bool> tcs;
            lock (_lock)
            {
                if (_browser != null && !_browser.IsClosed)
                {
                    return _browser;
                }

                if (_isDownloading && _downloadTask != null)
                {
                    tcs = _downloadTask;
                }
                else
                {
                    _isDownloading = true;
                    tcs = new TaskCompletionSource<bool>();
                    _downloadTask = tcs;
                }
            }

            if (_isDownloading && _downloadTask == tcs)
            {
                try
                {
                    // Tải Browser nếu chưa có
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();

                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                    });

                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    lock (_lock)
                    {
                        _isDownloading = false;
                        _downloadTask = null;
                    }
                    throw;
                }
                finally
                {
                    lock (_lock)
                    {
                        _isDownloading = false;
                        _downloadTask = null;
                    }
                }
            }
            else
            {
                await tcs.Task;
            }

            return _browser!;
        }

        public void Dispose()
        {
            _browser?.Dispose();
        }
    }
}
