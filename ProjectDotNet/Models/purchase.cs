namespace ProjectDotNet.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public int carId { get; set; }
        public int quantity { get; set; }

        public int purchaseDate { get; set; }

    }
}
