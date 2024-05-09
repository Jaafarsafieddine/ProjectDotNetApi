using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }
        public string CarName { get; set; }
        public string CarModel { get; set; }
        public string CarType { get; set; }
        public string CarDescription { get; set; }
        public decimal CarPrice { get; set; }
        public string CarImage { get; set; }
        public int CarQuantity { get; set; }

        public string CarColor { get; set; }    
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
