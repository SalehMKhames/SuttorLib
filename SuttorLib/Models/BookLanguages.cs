using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("BookLanguages")]
    public class BookLanguages
    {
        public string BookId { get; set; }
        public string LanguageId { get; set; }
    }
}
