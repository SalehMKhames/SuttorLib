using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Languages", Schema = "db51138")]
    public class Languages
    {
        public Guid Id { get; set; }
        public string LanguageCode { get; set; }
        public string Language { get; set; }
    }
}
