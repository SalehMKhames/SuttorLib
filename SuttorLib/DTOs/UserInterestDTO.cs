namespace SuttorLibrary.DTOs
{
    public class UserInterestDTO
    {
        public string UserID { get; set; }
        public List<string> CategoriesNames { get; set; } = new List<string>();
    }
}
