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

        /// <summary>Notification payload (type, deep-link ids, etc.) serialized as JSON.</summary>
        public string? Data { get; set; }

        /// <summary>Deserialized view of <see cref="Data"/> for convenience. Not persisted.</summary>
        [NotMapped]
        public Dictionary<string, string>? Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}