using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public int Quantity { get; set; }

        public int PurchaseDate { get; set; }
        public User User { get; set; }
        public Car Car { get; set; }

    }
}
