using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectDotNet.Models
{
    public class AddToCart
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }


        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<AddToCartDetails> AddToCartDetails { get; set; }
    }
}
