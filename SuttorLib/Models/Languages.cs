using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Languages")]
    public class Languages
    {
        public string Id { get; set; }
        public string Language { get; set; }
    }
}
