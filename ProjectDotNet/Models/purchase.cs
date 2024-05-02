using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }
        public int userId { get; set; }
        public int carId { get; set; }
        public int quantity { get; set; }

        public int purchaseDate { get; set; }
        public User user { get; set; }
        public Car car { get; set; }

    }
}
