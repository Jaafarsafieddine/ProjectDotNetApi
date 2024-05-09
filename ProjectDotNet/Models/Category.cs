using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string? subCategory { get; set; }
        public ICollection<Car> Cars { get; set; }
    }
}
