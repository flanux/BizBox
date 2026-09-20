# Bizbox

Student e-commerce project (ASP.NET Core MVC). See the project context doc
(kept alongside this repo) for the full vision and scope.

## Running in GitHub Codespaces

```
dotnet restore
dotnet ef database update   # only if bizbox.db / migrations don't already exist
dotnet run
```

No new EF migration should be needed for this update — Order/OrderItem/CartItem/
Identity roles all use tables that existed since the first migration.

## Demo admin account
- Email: `admin@bizbox.com`
- Password: `Admin@123`
(Seeded automatically on first run. Log in with this to reach `/Admin`.)

## What's now built
- Home page (4 business types) + per-type Catalog page
- Identity Register/Login/Logout, with role support (Admin role + demo admin account)
- Cart (add/view/remove)
- Checkout with 3 payment paths:
  - **eSewa** — real sandbox integration (RC/UAT test environment, public test
    merchant code `EPAYTEST`)
  - **Khalti** — real sandbox integration (KPG-2 epayment API, public test secret key)
  - **International card** — intentionally simulated UI-only flow (see project
    context doc, Section 5, for why)
- Resale/trade-in marketplace (Pillar 2) — any logged-in user can list a used item
- Upgrade notifications (Pillar 3) — computed on the fly: if you've bought a
  product that a newer product's `SupersedesProductId` points at, you'll see
  it under **Notifications**
- Admin panel (`/Admin`, Admin role only) — add products, mark a product as
  superseding an older one, toggle active/inactive

## Known simplifications (intentional — see project context doc Sections 5 & 6)
- Manufacturer listings are simulated via the admin panel, not real manufacturer accounts
- Shipping/logistics has no real courier integration — order status is admin-controlled only (no UI to change status yet beyond Pending -> Processing on payment)
- International payment is a UI-only simulated flow, no real gateway
- eSewa/Khalti integrations use documented **public sandbox test credentials** —
  fine for demoing, but note in your defense that these are test keys, not
  production merchant credentials (Bizbox itself doesn't have a real merchant account)

## Important: this has not been compiled or run by the assistant that wrote it
No .NET toolchain was available in the environment that generated this code —
everything was written by hand without a compiler. Treat your first
`dotnet build` after pulling this as a real test, same as before. Paste any
error output back for a fast fix rather than guessing.
