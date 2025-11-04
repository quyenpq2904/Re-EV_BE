namespace ReEV.Service.Marketplace.Models
{
    public sealed class Listing : Entity
    {
        public Guid SellerId { get; set; }
        public User Seller { get; set; } = null!;
        public string Title { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public float? BiddingIncrements { get; set; } = null;
        public float? EndPrice { get; set; } = null;
        public ListingType ListingType { get; set; }
        public bool IsVerified { get; set; } = false;
        public string[]? Images { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int BatteryPercentage { get; set; }
        public int YearOfManufacture { get; set; }
        public Condition Condition { get; set; }
        public DateTimeOffset? AuctionStartTime { get; set; } = null;
        public DateTimeOffset? AuctionEndTime { get; set; } = null;
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

        private Listing() { }

        // Constructor for BuyNow Listing
        private Listing(Guid sellerId, string title, string description, float price, string[] images,
            string brand, string model, int batteryPercentage, int yearOfManufacture, Condition condition)
        {
            SellerId = sellerId;
            Title = title;
            Description = description;
            Price = price;
            ListingType = ListingType.BUYNOW;
            Images = images;
            Brand = brand;
            Model = model;
            BatteryPercentage = batteryPercentage;
            YearOfManufacture = yearOfManufacture;
            Condition = condition;
        }

        // Constructor for Auction Listing
        private Listing(Guid sellerId, string title, string description, float price, float biddingIncrements,
            string[] images, string brand, string model, int batteryPercentage, int yearOfManufacture,
            Condition condition, DateTimeOffset auctionStartTime, DateTimeOffset auctionEndTime)
        {
            SellerId = sellerId;
            Title = title;
            Description = description;
            Price = price;
            BiddingIncrements = biddingIncrements;
            ListingType = ListingType.AUCTION;
            Images = images;
            Brand = brand;
            Model = model;
            BatteryPercentage = batteryPercentage;
            YearOfManufacture = yearOfManufacture;
            Condition = condition;
            AuctionStartTime = auctionStartTime;
            AuctionEndTime = auctionEndTime;
        }


        public static Listing CreateAuctionListing(Guid sellerId, string title, string description, float price, float biddingIncrements,
            string[] images, string brand, string model, int batteryPercentage, int yearOfManufacture,
            Condition condition, DateTimeOffset auctionStartTime, DateTimeOffset auctionEndTime)
        {
            return new Listing(sellerId, title, description, price, biddingIncrements, images, brand,
                model, batteryPercentage, yearOfManufacture, condition, auctionStartTime, auctionEndTime);
        }

        public static Listing CreateBuyNowListing(Guid sellerId, string title, string description, float price, string[] images,
            string brand, string model, int batteryPercentage, int yearOfManufacture, Condition condition)
        {
            return new Listing(sellerId, title, description, price, images, brand, model,
                batteryPercentage, yearOfManufacture, condition);
        }

    }
}
