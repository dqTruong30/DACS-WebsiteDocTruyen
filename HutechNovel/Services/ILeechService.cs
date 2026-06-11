using HutechNovel.Models;
using System.Threading.Tasks;

namespace HutechNovel.Services
{
    public interface ILeechService
    {
        Task<LeechPreviewResult> PreviewChapterAsync(string url, string titleSelector, string contentSelector, string nextSelector);
        string CleanHtml(string rawHtml);
    }
}
