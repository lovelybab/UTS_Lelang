namespace AuctionApi.Models
{
    public class BidRequest
    {
        public string BidderName { get; set; } = "";
        public long Amount { get; set; }
    }
}
