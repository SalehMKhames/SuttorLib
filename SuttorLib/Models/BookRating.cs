using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Book_Rating")]
    public class BookRating
    {
        [Key]
        public string Id { get; set; }

        public string BookId { get; set; }

        public string UserId { get; set; }

        public float Rating { get; set; } = 0f;
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
