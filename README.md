# Sistem Lelang Online (Auction Engine)
UTS Sistem Terdistribusi - Web API dengan ASP.NET Core (.NET 8) dan SQLite.

## Studi Kasus
Sistem lelang online dengan penanganan race condition pada penawaran (bid) bersamaan.

## Komponen
- AuctionApi: Web API (REST, format JSON)
- AuctionSimulator: console app simulasi 20 penawar bersamaan

## Cara Menjalankan
1. Buka UTS_Lelang.slnx di Visual Studio, set AuctionApi sebagai startup project.
2. Jalankan profil http, buka http://localhost:5000/swagger
3. Jalankan AuctionSimulator, pilih mode 1 (UNSAFE) atau 2 (SAFE).

## Endpoint
- GET /api/auctions
- GET /api/auctions/{id}
- POST /api/auctions
- GET /api/auctions/{id}/bids
- POST /api/auctions/{id}/bids/unsafe
- POST /api/auctions/{id}/bids
- POST /api/auctions/{id}/close

## Anggota Kelompok
1. Malikhatun Naqi - 24SA21A100
2. Dwi Irma Apriliani - 24SA21A060
3. Chandrawati Kaca Nur Andhin - 24SA21A059
4. Kafita Natasari - 24SA21A057
5. Ony Maulin Kusworo - 24SA21A050
