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
        public string UserId { get; set; }
        [Required]
        public Guid logId { get; set; }
    }
}
