using Bizbox.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Bizbox.Data;

// Simulated catalog data — see project context doc, Section 6.
// This stands in for real manufacturer listings. In a real deployment this
// would come from an admin panel; here it's seeded directly so the app
// has something to demo immediately after clone + run.
public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.BusinessTypes.Any()) return; // already seeded

        var coffee = new BusinessType { Name = "Coffee Shop", ShortDescription = "Everything to open a small coffee shop." };
        var bakery = new BusinessType { Name = "Bakery", ShortDescription = "Everything to open a small bakery." };
        var restaurant = new BusinessType { Name = "Restaurant", ShortDescription = "Everything to open a small restaurant." };
        var salon = new BusinessType { Name = "Salon", ShortDescription = "Everything to open a small salon." };

        db.BusinessTypes.AddRange(coffee, bakery, restaurant, salon);
        db.SaveChanges(); // need IDs before attaching products

        var products = new List<Product>
        {
            // ---- Coffee Shop ----
            new() { Name = "Espresso Machine — Single Group", Price = 85000, BusinessTypeId = coffee.Id,
                    Manufacturer = "Nuova Ricambi",
                    Description = "Entry-level commercial espresso machine built for a small shop's daily volume. Direct plumb-in with a reliable single steam wand for milk drinks.",
                    Specs = "1 group head · 230V, 2000W · 3.5L boiler · Direct water line" },
            new() { Name = "Commercial Burr Grinder", Price = 18000, BusinessTypeId = coffee.Id,
                    Manufacturer = "Mahlkönig",
                    Description = "Flat burr grinder for consistent, repeatable espresso grounds throughout a busy service.",
                    Specs = "64mm flat burrs · Stepless grind adjustment · 1.2kg bean hopper" },
            new() { Name = "POS Billing System", Price = 25000, BusinessTypeId = coffee.Id,
                    Manufacturer = "SwiftPOS",
                    Description = "Order and payment tracking system with a touchscreen till and receipt printer, sized for a single-counter shop.",
                    Specs = "10\" touchscreen · Thermal receipt printer · Offline mode" },
            new() { Name = "Glass-Front Display Fridge", Price = 40000, BusinessTypeId = coffee.Id,
                    Manufacturer = "ColdLine",
                    Description = "Front-of-counter fridge for pastries and cold drinks, keeping stock visible to customers.",
                    Specs = "200L capacity · 2–8°C range · LED interior lighting" },

            // ---- Bakery ----
            new() { Name = "Deck Oven — 2 Deck", Price = 120000, BusinessTypeId = bakery.Id,
                    Manufacturer = "Bakermax",
                    Description = "Two-deck electric oven for breads and pastries, with independent temperature control per deck.",
                    Specs = "2 decks · Up to 300°C · Independent deck controls · Steam injection" },
            new() { Name = "Spiral Dough Mixer", Price = 45000, BusinessTypeId = bakery.Id,
                    Manufacturer = "Doughline",
                    Description = "Spiral mixer for bread and pastry dough, built for repeated daily batches without overheating the motor.",
                    Specs = "25kg flour capacity · 2-speed motor · Removable bowl" },
            new() { Name = "Proofing Cabinet", Price = 30000, BusinessTypeId = bakery.Id,
                    Manufacturer = "Bakermax",
                    Description = "Temperature and humidity-controlled cabinet for consistent dough proofing before baking.",
                    Specs = "8-tray capacity · 25–40°C range · Humidity control" },
            new() { Name = "Bakery Display Shelving", Price = 15000, BusinessTypeId = bakery.Id,
                    Manufacturer = "ShopFit",
                    Description = "Front-of-shop shelving for finished baked goods, with adjustable tiers for different tray sizes.",
                    Specs = "3-tier adjustable · Stainless steel frame · 90cm width" },

            // ---- Restaurant ----
            new() { Name = "Commercial Gas Range — 4 Burner", Price = 95000, BusinessTypeId = restaurant.Id,
                    Manufacturer = "Ironclad Kitchen",
                    Description = "Four-burner gas range for a small kitchen line, with an integrated oven below for baking or holding.",
                    Specs = "4 burners · Integrated oven · Cast iron grates · LPG/NG compatible" },
            new() { Name = "Walk-in Fridge Unit — Compact", Price = 150000, BusinessTypeId = restaurant.Id,
                    Manufacturer = "ColdLine",
                    Description = "Compact walk-in refrigeration unit for bulk ingredient storage, sized for a small restaurant kitchen.",
                    Specs = "6m³ capacity · 0–5°C range · Insulated panel construction" },
            new() { Name = "Stainless Steel Prep Table Set", Price = 20000, BusinessTypeId = restaurant.Id,
                    Manufacturer = "Ironclad Kitchen",
                    Description = "Set of stainless steel prep tables for kitchen workflow, with under-shelf storage.",
                    Specs = "3-table set · 304-grade stainless steel · Under-shelf storage" },
            new() { Name = "Dining Furniture Set", Price = 60000, BusinessTypeId = restaurant.Id,
                    Manufacturer = "SeatCo",
                    Description = "Tables and chairs for a small dining area, seating up to 16 guests.",
                    Specs = "4 tables · 16 chairs · Seats 16 · Stackable chairs" },

            // ---- Salon ----
            new() { Name = "Hydraulic Styling Chair", Price = 22000, BusinessTypeId = salon.Id,
                    Manufacturer = "SalonPro",
                    Description = "Adjustable hydraulic styling chair with a reclining backrest for cutting and styling services.",
                    Specs = "Hydraulic pump lift · 360° swivel · Reclining backrest" },
            new() { Name = "Wash Station — Basin & Chair", Price = 35000, BusinessTypeId = salon.Id,
                    Manufacturer = "SalonPro",
                    Description = "Combined basin and reclining chair for hair washing, with adjustable neck rest.",
                    Specs = "Ceramic basin · Reclining chair · Adjustable neck rest" },
            new() { Name = "Styling Tool Kit", Price = 15000, BusinessTypeId = salon.Id,
                    Manufacturer = "Vellora",
                    Description = "Starter set of dryers, straighteners, and trimmers for a new styling station.",
                    Specs = "1 dryer · 1 flat iron · 1 trimmer set · Dual voltage" },
            new() { Name = "Wall Mirror Set", Price = 10000, BusinessTypeId = salon.Id,
                    Manufacturer = "ShopFit",
                    Description = "Mounted mirrors for styling stations, with an integrated shelf for tools.",
                    Specs = "3-mirror set · Integrated tool shelf · Wall-mounted" },
        };

        db.Products.AddRange(products);
        db.SaveChanges();

        // ---- Demonstrate the "supersedes" relationship (Pillar 3) ----
        var oldGrinder = products.First(p => p.Name == "Commercial Burr Grinder");
        var newGrinder = new Product
        {
            Name = "Commercial Burr Grinder — Efficient Model",
            Price = 21000,
            BusinessTypeId = coffee.Id,
            Manufacturer = "Mahlkönig",
            Description = "Newer grinder model with the same grind consistency as the original, at lower energy draw.",
            Specs = "64mm flat burrs · Stepless grind adjustment · 30% lower idle power draw",
            SupersedesProductId = oldGrinder.Id
        };
        db.Products.Add(newGrinder);
        db.SaveChanges();
    }

    // Seeds an "Admin" role and one demo admin account, so there's an immediate
    // way to reach the admin panel without manually promoting a user via SQL.
    // Demo login: admin@bizbox.com / Admin@123
    public static async Task SeedIdentityAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = "admin@bizbox.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = "Bizbox Admin"
            };
            await userManager.CreateAsync(admin, "Admin@123");
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
