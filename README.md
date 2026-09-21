# Bizbox

An ASP.NET Core MVC e-commerce platform that helps users start a small
business by providing a curated equipment catalog for their chosen business
type (Coffee Shop, Bakery, Restaurant, or Salon), with a secondary resale
marketplace for used equipment and an upgrade-notification system.

## Getting Started

**One-command setup:**
```
./setup.sh          # Mac/Linux/Codespaces
.\setup.ps1          # Windows PowerShell
```
Then `dotnet run`.

**Or manually:**
```
dotnet restore
dotnet ef database update
dotnet run
```

Before testing Khalti checkout, replace the placeholder in `appsettings.json`
(`PaymentGateways:Khalti:SecretKey`) with a real test key from
`test-admin.khalti.com` — Khalti doesn't offer a shared public test key the
way eSewa does.

The database is seeded automatically on first run with business types,
starter catalog items, and a demo admin account (see project notes).

## Features

- Curated per-business-type equipment catalogs
- Cart and checkout with eSewa and Khalti sandbox payment integration, plus
  a simulated international payment flow
- Resale/trade-in marketplace for used equipment
- Upgrade notifications when a newer model of an owned product becomes available
- Admin panel for managing the product catalog

## Tech Stack

ASP.NET Core MVC, Entity Framework Core, SQLite, ASP.NET Core Identity.
