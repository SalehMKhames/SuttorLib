using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLib.Models.Library
{
    [Table("Book_Authors")]
    public class BookAuthors
    {
        [Key]
        public string Id { get; set; }
        
        public string Book_Id { get; set; }

        public string Author_Id { get; set; }
    }
}
