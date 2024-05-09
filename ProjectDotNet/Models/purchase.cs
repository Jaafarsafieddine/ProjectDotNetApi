using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectDotNet.Models
{
    [Table("Purchase")]
    public class Purchase
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public int Quantity { get; set; }
        public String PurchaseDate { get; set; }
        public User User { get; set; }
        public Car Car { get; set; }

    }
}
