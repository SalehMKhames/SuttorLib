using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Book_Authors" , Schema = "SuttorDB")]
    public class BookAuthors
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid Book_Id { get; set; }

        public Guid Author_Id { get; set; }
    }
}
