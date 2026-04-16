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
        public string Descritpion { get; set; } = string.Empty;
        public string language { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int PublishedAT { get; set; }
        public List<string> Authors_Names { get; set; }
        public List<string> Categories_Names { get; set; }
    }
}
