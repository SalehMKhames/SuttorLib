using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("User_Interests")]
    public class UserInterests
    {
        [Key]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Category_Id{ get; set; }
    }
}
