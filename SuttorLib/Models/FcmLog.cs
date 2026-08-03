using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLib.Models
{
    [Table("FcmLog")]
    public class FcmLog
    {
        [Required]
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        [NotMapped]
        public Dictionary<string, string> Type { get; set; } = new();
    }
}
