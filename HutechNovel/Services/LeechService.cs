using HutechNovel.Models;
using System;
using System.Threading.Tasks;
using PuppeteerSharp;
using HtmlAgilityPack;
using System.Linq;

namespace HutechNovel.Services
{
    public class LeechService : ILeechService
    {
        private readonly IPuppeteerProvider _puppeteerProvider;

        public LeechService(IPuppeteerProvider puppeteerProvider)
        {
            _puppeteerProvider = puppeteerProvider;
        }

        public async Task<LeechPreviewResult> PreviewChapterAsync(string url, string titleSelector, string contentSelector, string nextSelector)
        {
            var result = new LeechPreviewResult { Success = false };
            IPage? page = null;
            try
            {
                var browser = await _puppeteerProvider.GetBrowserAsync();
                page = await browser.NewPageAsync();

                // Chờ cho đến khi JS chạy xong và network idle
                await page.GoToAsync(url, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
                    Timeout = 60000 // Chờ tối đa 60s
                });

                // Xóa các thẻ rác bằng JS trực tiếp trên trình duyệt ảo
                await page.EvaluateExpressionAsync(@"
                    document.querySelectorAll('script, style, iframe, meta, link').forEach(el => el.remove());
                ");

                // Lấy Title
                if (!string.IsNullOrEmpty(titleSelector))
                {
                    try {
                        var title = await page.EvaluateFunctionAsync<string>($@"() => {{
                            var el = document.querySelector('{titleSelector}');
                            return el ? el.innerText.trim() : '';
                        }}");
                        result.Title = title;
                    } catch {}
                }

                // Lấy Content HTML (Không dùng innerText để giữ nguyên format, sau đó Clean)
                if (!string.IsNullOrEmpty(contentSelector))
                {
                    try {
                        var rawHtml = await page.EvaluateFunctionAsync<string>($@"() => {{
                            var el = document.querySelector('{contentSelector}');
                            return el ? el.innerHTML : '';
                        }}");
                        result.ContentHtml = CleanHtml(rawHtml);
                    } catch {}
                }

                // Lấy Link Next
                if (!string.IsNullOrEmpty(nextSelector))
                {
                    try {
                        var nextHref = await page.EvaluateFunctionAsync<string>($@"() => {{
                            var el = document.querySelector('{nextSelector}');
                            return el ? el.getAttribute('href') : '';
                        }}");
                        
                        if (!string.IsNullOrEmpty(nextHref))
                        {
                            if (!nextHref.StartsWith("http"))
                            {
                                var uri = new Uri(url);
                                string hostStr = $"{uri.Scheme}://{uri.Host}";
                                result.NextUrl = $"{hostStr.TrimEnd('/')}/{nextHref.TrimStart('/')}";
                            }
                            else
                            {
                                result.NextUrl = nextHref;
                            }
                        }
                    } catch {}
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                if (page != null)
                {
                    await page.CloseAsync();
                }
            }
            
            return result;
        }

        public string CleanHtml(string rawHtml)
        {
            if (string.IsNullOrEmpty(rawHtml)) return "";

            var doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);

            // 1. Xóa các thẻ rác và quảng cáo dựa trên tên thẻ và class/id
            var nodesToRemove = doc.DocumentNode.Descendants()
                .Where(n => 
                    n.Name == "script" || 
                    n.Name == "style" || 
                    n.Name == "iframe" || 
                    n.Name == "meta" || 
                    n.Name == "link" ||
                    n.Name == "noscript" ||
                    n.Name == "ins" || // Thường dùng cho Google Adsense
                    n.Name == "svg" ||
                    IsAdNode(n) ||
                    (n.GetAttributeValue("style", "").ToLower().Contains("display:none")) ||
                    (n.GetAttributeValue("style", "").ToLower().Contains("display: none"))
                )
                .ToList();

            foreach (var n in nodesToRemove)
            {
                n.Remove();
            }
            
            // 2. Lọc các đoạn text quảng cáo rác thường gặp
            var textNodes = doc.DocumentNode.DescendantsAndSelf()
                .Where(n => n.NodeType == HtmlNodeType.Text)
                .ToList();
                
            foreach (var n in textNodes)
            {
                var text = n.InnerText.ToLower();
                if (text.Contains("bạn đang đọc truyện") || 
                    text.Contains("đọc truyện tại") || 
                    text.Contains("truyện được copy tại") || 
                    text.Contains("nguồn truyện") ||
                    text.Contains("truyện được cập nhật tại"))
                {
                    n.InnerHtml = "";
                }
            }
            
            // 3. Xóa các thẻ p, div rỗng không có nội dung
            var emptyNodes = doc.DocumentNode.Descendants()
                .Where(n => (n.Name == "p" || n.Name == "div") && string.IsNullOrWhiteSpace(n.InnerText) && n.ChildNodes.Count == 0)
                .ToList();
                
            foreach (var n in emptyNodes)
            {
                n.Remove();
            }

            return doc.DocumentNode.InnerHtml.Trim();
        }

        private bool IsAdNode(HtmlNode n)
        {
            var className = n.GetAttributeValue("class", "").ToLower();
            var id = n.GetAttributeValue("id", "").ToLower();

            string[] adKeywords = { "ads", "advert", "quang-cao", "quangcao", "qc", "adsbygoogle", "banner", "ads-box" };
            
            // Kiểm tra các từ khóa trong class (chia cắt bởi khoảng trắng)
            var classTokens = className.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (classTokens.Any(c => adKeywords.Contains(c) || c.StartsWith("ads-"))) return true;

            // Kiểm tra các từ khóa trong id
            var idTokens = id.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (idTokens.Any(i => adKeywords.Contains(i))) return true;

            return false;
        }
    }
}
