using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Book_Categories", Schema = "db51138")]
    public class BookCategories
    {
        [Key]
        public Guid Id { get; set; }

        public Guid bookId { get; set; }
        
        public Guid categoryId { get; set; }
    }
}
