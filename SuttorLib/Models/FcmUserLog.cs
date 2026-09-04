using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLib.Models
{
    [Table("FcmUserLog")]
    public class FcmUserLog
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public Guid LogId { get; set; }
    }
}