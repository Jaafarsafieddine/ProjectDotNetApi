namespace ProjectDotNet.Models.OutPutModels
{
    public class UserPurchaseDto
    {
        public int PurchaseId { get; set; }
        public int CarId { get; set; }
        public string CarName { get; set; }
        public int Quantity { get; set; }
        public String PurchaseDate { get; set; }
        public string CarModel { get; set; }
        public string CarImage { get; set; }
    }

}
