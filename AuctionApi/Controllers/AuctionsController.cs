using AuctionApi.Data;
using AuctionApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace AuctionApi.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuctionsController(AppDbContext db) => _db = db;

    // Satu kunci (mutex) per lelang
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Auctions.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var auction = await _db.Auctions.FindAsync(id);
        return auction == null
            ? NotFound(new { message = "Lelang tidak ditemukan" })
            : Ok(auction);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Auction input)
    {
        input.Id = 0;
        input.CurrentPrice = input.StartingPrice;
        input.Status = "Open";
        input.HighestBidder = null;
        input.Version = 0;

        _db.Auctions.Add(input);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    [HttpGet("{id}/bids")]
    public async Task<IActionResult> GetBids(int id) =>
        Ok(await _db.Bids
            .Where(b => b.AuctionId == id)
            .OrderByDescending(b => b.Amount)
            .ToListAsync());

    // ===== VERSI TIDAK AMAN (bukti race condition) =====
    [HttpPost("{id}/bids/unsafe")]
    public async Task<IActionResult> BidUnsafe(int id, BidRequest req)
    {
        var auction = await _db.Auctions.FindAsync(id);
        if (auction == null) return NotFound(new { message = "Lelang tidak ditemukan" });
        if (auction.Status != "Open" || DateTime.UtcNow > auction.EndTime)
            return BadRequest(new { message = "Lelang sudah ditutup" });

        // 1) CHECK: baca harga saat ini
        if (req.Amount <= auction.CurrentPrice)
            return BadRequest(new { message = "Tawaran harus lebih tinggi dari harga saat ini" });

        // Jeda kecil memperlebar celah race condition agar mudah dibuktikan
        await Task.Delay(50);

        // 2) ACT: simpan bid dan timpa harga tertinggi
        _db.Bids.Add(new Bid { AuctionId = id, BidderName = req.BidderName, Amount = req.Amount });
        auction.CurrentPrice = req.Amount;
        auction.HighestBidder = req.BidderName;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Bid diterima", auction.CurrentPrice, auction.HighestBidder });
    }

    // ===== VERSI AMAN (mutex + transaction) =====
    [HttpPost("{id}/bids")]
    public async Task<IActionResult> BidSafe(int id, BidRequest req)
    {
        var mutex = _locks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
        await mutex.WaitAsync();
        try
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var auction = await _db.Auctions.FirstOrDefaultAsync(a => a.Id == id);
            if (auction == null) return NotFound(new { message = "Lelang tidak ditemukan" });
            if (auction.Status != "Open" || DateTime.UtcNow > auction.EndTime)
                return BadRequest(new { message = "Lelang sudah ditutup" });
            if (req.Amount <= auction.CurrentPrice)
                return BadRequest(new { message = "Tawaran harus lebih tinggi dari harga saat ini" });

            await Task.Delay(50); // jeda sama seperti versi unsafe, supaya perbandingannya adil

            _db.Bids.Add(new Bid { AuctionId = id, BidderName = req.BidderName, Amount = req.Amount });
            auction.CurrentPrice = req.Amount;
            auction.HighestBidder = req.BidderName;
            auction.Version++;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { message = "Bid diterima", auction.CurrentPrice, auction.HighestBidder });
        }
        finally
        {
            mutex.Release();
        }
    }

    // ===== TUTUP LELANG =====
    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var mutex = _locks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
        await mutex.WaitAsync();
        try
        {
            var auction = await _db.Auctions.FindAsync(id);
            if (auction == null) return NotFound(new { message = "Lelang tidak ditemukan" });
            auction.Status = "Closed";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Lelang ditutup", winner = auction.HighestBidder, finalPrice = auction.CurrentPrice });
        }
        finally
        {
            mutex.Release();
        }
    }
}