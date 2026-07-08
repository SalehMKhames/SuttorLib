using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLib.Models.Library
{
    [Table("Downloads")]
    public class Download
    {
        [Key]
        public string Id { get; set; }

        public string BookID { get; set; }

        public string UserID { get; set; }

        public DateTime DownloadedAt { get; set; }
        public bool IsFinishReading { get; set; }
    }
}
