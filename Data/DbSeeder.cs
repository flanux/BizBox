using Bizbox.Models;

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
            new() { Name = "Espresso Machine (Entry-level)", Price = 85000, BusinessTypeId = coffee.Id,
                    Description = "Single-group espresso machine suitable for a small shop." },
            new() { Name = "Coffee Grinder", Price = 18000, BusinessTypeId = coffee.Id,
                    Description = "Burr grinder for consistent espresso grounds." },
            new() { Name = "POS Billing System", Price = 25000, BusinessTypeId = coffee.Id,
                    Description = "Simple point-of-sale system for order and payment tracking." },
            new() { Name = "Display Fridge", Price = 40000, BusinessTypeId = coffee.Id,
                    Description = "Glass-front fridge for pastries and cold drinks." },

            // ---- Bakery ----
            new() { Name = "Deck Oven", Price = 120000, BusinessTypeId = bakery.Id,
                    Description = "Multi-deck oven for breads and pastries." },
            new() { Name = "Dough Mixer", Price = 45000, BusinessTypeId = bakery.Id,
                    Description = "Spiral mixer for bread and pastry dough." },
            new() { Name = "Proofing Cabinet", Price = 30000, BusinessTypeId = bakery.Id,
                    Description = "Temperature-controlled cabinet for dough proofing." },
            new() { Name = "Display Shelving", Price = 15000, BusinessTypeId = bakery.Id,
                    Description = "Front-of-shop shelving for finished baked goods." },

            // ---- Restaurant ----
            new() { Name = "Commercial Gas Range", Price = 95000, BusinessTypeId = restaurant.Id,
                    Description = "Multi-burner gas range for a small kitchen." },
            new() { Name = "Walk-in Fridge Unit", Price = 150000, BusinessTypeId = restaurant.Id,
                    Description = "Compact walk-in refrigeration unit." },
            new() { Name = "Prep Tables (Stainless Steel)", Price = 20000, BusinessTypeId = restaurant.Id,
                    Description = "Stainless steel prep table set for kitchen workflow." },
            new() { Name = "Dining Furniture Set", Price = 60000, BusinessTypeId = restaurant.Id,
                    Description = "Tables and chairs for a small dining area." },

            // ---- Salon ----
            new() { Name = "Styling Chair", Price = 22000, BusinessTypeId = salon.Id,
                    Description = "Adjustable hydraulic styling chair." },
            new() { Name = "Wash Station", Price = 35000, BusinessTypeId = salon.Id,
                    Description = "Basin and chair combo for hair washing." },
            new() { Name = "Styling Tool Kit", Price = 15000, BusinessTypeId = salon.Id,
                    Description = "Dryers, straighteners, and trimmers starter set." },
            new() { Name = "Wall Mirror Set", Price = 10000, BusinessTypeId = salon.Id,
                    Description = "Mounted mirrors for styling stations." },
        };

        db.Products.AddRange(products);
        db.SaveChanges();

        // ---- Demonstrate the "supersedes" relationship (Pillar 3) ----
        // Add one newer model that replaces an older coffee-shop item, to show
        // the notification concept end-to-end.
        var oldGrinder = products.First(p => p.Name == "Coffee Grinder");
        var newGrinder = new Product
        {
            Name = "Coffee Grinder (2026 Efficient Model)",
            Price = 21000,
            BusinessTypeId = coffee.Id,
            Description = "Newer grinder model — lower energy use, same grind consistency.",
            SupersedesProductId = oldGrinder.Id
        };
        db.Products.Add(newGrinder);
        db.SaveChanges();
    }
}
