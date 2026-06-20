using System.ComponentModel.DataAnnotations; 

namespace SuttorLibrary.DTOs
{
    public class UploadBookDTO
    {
        [Required]
        public required IFormFile File { get; set; }
        [Required]
        public required IFormFile CoverPic { get; set; }
        public string Description { get; set; } = string.Empty;
        public string language { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int PublishedAT { get; set; }
        public List<string> Authors_Names { get; set; }
        public List<string> Categories_Names { get; set; }
    }

    public class GetBookDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int PublishedAT { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public IFormFile? Photo { get; set; } = null;
        public string FileLink { get; set; } = string.Empty;
        public string CoverLink { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string language { get; set; } = string.Empty;
        public List<string> Authors_Names { get; set; } = new List<string>();
        public List<string> Categories_Names { get; set; } = new List<string>();
    }

    public class RatingDTO
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public float Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class GetRatingDTO
    {
        public string UserId { get; set; }
        public float Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class AuthorDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IFormFile? Picture { get; set; }
        public string? Description { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class AddAuthorDto
    {
        public string Author { get; set; }
        public IFormFile Picture { get; set; }
        public string Desc { get; set; }
    }
}
