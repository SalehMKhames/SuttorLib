using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLib.Models.Library
{
    [Table("Languages")]
    public class Languages
    {
        public string Id { get; set; }
        public string Language { get; set; }
    }
}
