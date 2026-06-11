namespace HutechNovel.Models
{
    public class MassLeechRequest
    {
        public int TruyenId { get; set; }
        public string StartUrl { get; set; } = string.Empty;
        public string TitleSelector { get; set; } = string.Empty;
        public string ContentSelector { get; set; } = string.Empty;
        public string NextSelector { get; set; } = string.Empty;
        public int? MaxChapters { get; set; }
    }
}
