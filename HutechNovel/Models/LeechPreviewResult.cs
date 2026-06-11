namespace HutechNovel.Models
{
    public class LeechPreviewResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public string NextUrl { get; set; } = string.Empty;
    }
}
