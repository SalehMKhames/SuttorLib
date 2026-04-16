using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Languages", Schema = "SuttorDB")]
    public class Languages
    {
        public Guid Id { get; set; }
        public string LanguageCode { get; set; }
        public string Language { get; set; }
    }
}
