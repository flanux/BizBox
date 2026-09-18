# Bizbox

Student e-commerce project (ASP.NET Core MVC). See `bizbox-project-context.md`
(kept alongside this repo, not inside it) for the full vision, scope, and
development plan.

## Running in GitHub Codespaces

1. Open this repo in a Codespace (`Code` → `Codespaces` → `Create codespace on main`).
2. Codespace should auto-detect the .NET SDK. If not: `dotnet --version` to confirm 8.x is available.
3. Restore + run:
   ```
   dotnet restore
   dotnet run
   ```
4. The dev server URL will pop up in a "Ports" notification — click it to open the app.
5. First run creates `bizbox.db` (SQLite) and seeds the 4 business types + starter products automatically.

## Adding EF Core migrations

This starter has models + DbContext but no migration yet. First time only:

```
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## What's here vs. what's next

Already scaffolded:
- Project structure (Controllers / Models / ViewModels / Data / Services / Views)
- Identity (login/register) wired up
- BusinessType + Product + CartItem + Order/OrderItem models
- DbSeeder with the 4 business types and starter catalog items, including one
  example "supersedes" relationship (Pillar 3 demo data)
- Home page (business type grid) and Catalog page (per-business-type product list + add to cart)
- CartService (add to cart wired up; view-cart/checkout pages not built yet)

Not yet built (see project context doc for the intended build order):
- Cart page / Checkout page / Order placement
- eSewa / Khalti payment integration
- Resale/trade-in listing flow
- Notification UI for superseded products
- Admin panel
