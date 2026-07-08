using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLib.Models.Library
{
    [Table("Book_Categories")]
    public class BookCategories
    {
        [Key]
        public string Id { get; set; }

        public string bookId { get; set; }
        
        public string categoryId { get; set; }
    }
}
