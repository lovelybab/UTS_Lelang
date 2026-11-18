namespace AuctionApi.Models;

public class Bid
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public string BidderName { get; set; } = "";
    public long Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}