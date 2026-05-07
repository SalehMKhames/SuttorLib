using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Books", Schema = "db51138")]
    public class Book
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public required string Title { get; set; } = string.Empty;
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int PublishedAT { get; set; }
        
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string PhotoPath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
