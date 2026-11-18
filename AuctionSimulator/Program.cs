using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = "http://localhost:5000";
var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine("=== SIMULASI LELANG: 20 PENAWAR BERSAMAAN ===");
Console.Write("Pilih mode (1 = UNSAFE, 2 = SAFE): ");
var mode = Console.ReadLine() == "1" ? "bids/unsafe" : "bids";
Console.WriteLine($"Mode: {mode}\n");

// 1) Buat lelang baru yang bersih
var create = await http.PostAsJsonAsync("/api/auctions", new
{
    title = "Simulasi Race Condition",
    description = "Lelang uji concurrency",
    startingPrice = 100000,
    endTime = DateTime.UtcNow.AddDays(1)
});
var created = await create.Content.ReadFromJsonAsync<JsonElement>();
int auctionId = created.GetProperty("id").GetInt32();
Console.WriteLine($"Lelang dibuat, ID = {auctionId}, harga awal = 100000\n");

// 2) Siapkan 20 penawar, tawaran 110000, 120000, ... 300000
var bidders = Enumerable.Range(1, 20)
    .Select(i => (Name: $"Penawar{i:00}", Amount: 100000L + i * 10000L))
    .ToList();

// 3) Kirim semuanya BERSAMAAN
var tasks = bidders.Select(async b =>
{
    var res = await http.PostAsJsonAsync($"/api/auctions/{auctionId}/{mode}",
        new { bidderName = b.Name, amount = b.Amount });
    return (b.Name, b.Amount, Status: (int)res.StatusCode);
}).ToList();

var results = await Task.WhenAll(tasks);

Console.WriteLine("--- HASIL TIAP REQUEST ---");
foreach (var r in results.OrderBy(r => r.Amount))
    Console.WriteLine($"{r.Name} | {r.Amount,8} | HTTP {r.Status}");

// 4) Cek kondisi akhir lelang
var final = await http.GetFromJsonAsync<JsonElement>($"/api/auctions/{auctionId}");
long finalPrice = final.GetProperty("currentPrice").GetInt64();
string? winner = final.GetProperty("highestBidder").GetString();
long highestSent = bidders.Max(b => b.Amount);

Console.WriteLine("\n--- KONDISI AKHIR ---");
Console.WriteLine($"Harga akhir di database : {finalPrice}");
Console.WriteLine($"Pemenang tercatat       : {winner}");
Console.WriteLine($"Tawaran tertinggi dikirim: {highestSent}");
Console.WriteLine(finalPrice == highestSent
    ? "KONSISTEN: harga akhir = tawaran tertinggi"
    : "TIDAK KONSISTEN: terjadi RACE CONDITION (lost update)!");

Console.WriteLine("\nTekan Enter untuk keluar...");
Console.ReadLine();