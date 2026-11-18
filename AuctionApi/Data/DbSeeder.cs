using AuctionApi.Models;

namespace AuctionApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Auctions.Any()) return;

        db.Auctions.AddRange(
            new Auction
            {
                Title = "Laptop Bekas ThinkPad T480",
                Description = "Kondisi mulus, RAM 16GB",
                StartingPrice = 100000,
                CurrentPrice = 100000,
                EndTime = DateTime.UtcNow.AddDays(7)
            },
            new Auction
            {
                Title = "Jam Tangan Vintage",
                Description = "Koleksi tahun 1990-an",
                StartingPrice = 50000,
                CurrentPrice = 50000,
                EndTime = DateTime.UtcNow.AddDays(3)
            });

        db.SaveChanges();
    }
}