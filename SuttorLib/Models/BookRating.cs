using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Book_Rating", Schema = "db51138")]
    public class BookRating
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookId { get; set; }

        public string UserId { get; set; }

        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
