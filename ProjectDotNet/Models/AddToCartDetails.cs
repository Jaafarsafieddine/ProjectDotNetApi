using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class AddToCartDetails
    {
        [Key]
        public int Id { get; set; }
        public int CartId { get; set; }
        public int CarId { get; set; }
        public int Quantity { get; set; }
        public AddToCart Cart { get; set; }
        public Car Car { get; set; }
    }
}
