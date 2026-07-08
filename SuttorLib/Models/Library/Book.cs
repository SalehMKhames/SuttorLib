using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLib.Models.Library
{
    [Table("Books")]
    public class Book
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public required string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int PublishedAT { get; set; }
        public float Rating { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public double FileSize { get; set; }
        public string PhotoPath { get; set; }
        public DateTime UploadedAt { get; set; }

        [ForeignKey("LanguageId")]
        public string LanguageId { get; set; }
    }
}
