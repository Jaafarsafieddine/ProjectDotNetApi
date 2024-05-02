using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }
        public string carName { get; set; }
        public string carModel { get; set; }
        public string carType { get; set; }
        public string carColor { get; set; }
        public int carPrice { get; set; }
        public string carImage { get; set; }
        public int carQuantity { get; set; }

        public int categoryId { get; set; }
        public Category category { get; set; }
    }
}
