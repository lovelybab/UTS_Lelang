using System.ComponentModel.DataAnnotations;

namespace AuctionApi.Models;

public class Auction
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public long StartingPrice { get; set; }
    public long CurrentPrice { get; set; }
    public string? HighestBidder { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Open"; // Open | Closed

    // Dipakai nanti untuk optimistic concurrency
    [ConcurrencyCheck]
    public int Version { get; set; }
}