using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Downloads", Schema = "SuttorDB")]
    public class Download
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookID { get; set; }

        public string UserID { get; set; }

        public DateTime DownloadedAt { get; set; }
        public bool IsFinishReading { get; set; }
    }
}
