namespace TomocaCampaignAPI.DTOs
{
    public class UsersBot
    {
        public int Id { get; set; } // Primary key

        public string UserId { get; set; }

        public string PhoneNum { get; set; }

        public string Step { get; set; }

        public int Misc { get; set; }

        public string UserName { get; set; }

        public string Location { get; set; }

        public float TotalAmount { get; set; }

        public string Lat { get; set; }

        public string Longtiud { get; set; }

        public string LastName { get; set; }

        public string OrderType { get; set; }

        public string StartID { get; set; }

        public string LastMsg { get; set; }

        public string QuantityHolder { get; set; }

        public string CartStart { get; set; }

        public string CartEnd { get; set; }

        public string NumProducts { get; set; }

        public string UserProductId { get; set; }

        public string Phase { get; set; }

        public string ShopLocation { get; set; }

        public string TinName { get; set; }

        public string TinNumber { get; set; }

        public string LastID { get; set; }

        public string Roast { get; set; }

        public string CoffeeType { get; set; }

        public int UserSubscriptionId { get; set; }

        public DateTime? StartDate { get; set; } // Nullable datetime

    }
}
