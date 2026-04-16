using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("BookLanguages", Schema = "SuttorDB")]
    public class BookLanguages
    {
        public Guid BookId { get; set; }
        public Guid LanguageId { get; set; }
    }
}
