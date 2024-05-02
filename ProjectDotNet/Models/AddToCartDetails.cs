using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class AddToCartDetails
    {
        [Key]
        public int Id { get; set; }
        public int cartId { get; set; }
        public int carId { get; set; }
        public int quantity { get; set; }
        public AddToCart cart { get; set; }
        public Car car { get; set; }
    }
}
