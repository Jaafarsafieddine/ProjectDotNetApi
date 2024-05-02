using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class AddToCart
    {
        [Key]
        public int Id { get; set; }
        public int userId { get; set; }
        public User user { get; set; }
    }
}
