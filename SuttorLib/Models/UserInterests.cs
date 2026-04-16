using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("User_Interests", Schema = "SuttorDB")]
    public class UserInterests
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public Guid Category_Id{ get; set; }
    }
}
