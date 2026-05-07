using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("BookLanguages", Schema = "db51138")]
    public class BookLanguages
    {
        public Guid BookId { get; set; }
        public Guid LanguageId { get; set; }
    }
}
