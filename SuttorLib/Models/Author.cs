using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Authors")]
    public class Author
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

}
