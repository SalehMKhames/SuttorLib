using System.ComponentModel.DataAnnotations;

namespace SuttorLibrary.DTOs
{
    public class UploadBookDTO
    {
        [Required]
        public required IFormFile File { get; set; }
        [Required]
        public required IFormFile CoverPic { get; set; }
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;
        public string language { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int PublishedAT { get; set; }
        public List<string> Authors_Names { get; set; }
        public List<string> Categories_Names { get; set; }
    }

    public class GetBookDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int PublishedAT { get; set; }
        public string FilePath { get; set; }
        public IFormFile Photo { get; set; }
        public DateTime UploadedAt { get; set; }
        public string language { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public List<string> Authors_Names { get; set; }
        public List<string> Categories_Names { get; set; }
    }
}
