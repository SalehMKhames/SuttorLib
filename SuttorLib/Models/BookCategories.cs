using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Book_Categories", Schema = "SuttorDB")]
    public class BookCategories
    {
        [Key]
        public Guid Id { get; set; }

        public Guid bookId { get; set; }
        
        public Guid categoryId { get; set; }
    }
}
