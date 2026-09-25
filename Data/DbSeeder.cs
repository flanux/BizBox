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
        if (!db.BusinessTypes.Any())
            SeedCatalog(db);

        SeedExpansionCatalog(db);
        SeedBundles(db);
    }

    private static void SeedCatalog(ApplicationDbContext db)
    {
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

    // Small helper types for the data-driven expansion catalog below — much
    // less repetitive than hand-writing ~20 more category blocks in
    // SeedCatalog's verbose style.
    private record ExpansionProduct(string Name, decimal Price, string Manufacturer, string Description, string Specs);
    private record ExpansionCategory(string Name, BusinessSegment Segment, string ShortDescription, string Icon, ExpansionProduct[] Products);

    private static readonly ExpansionCategory[] Expansion = new ExpansionCategory[]
    {
        // ---- Small Business ----
        new("Tea Stall", BusinessSegment.SmallBusiness, "Everything to run a small tea stall.", "🫖", new ExpansionProduct[]
        {
            new("Milk Boiler & Tea Kettle", 6000m, "ChaiWorks", "Gas-fired kettle for boiling milk tea in volume through a rush.", "8L capacity · LPG · Insulated handle"),
            new("Stainless Tea Cart", 12000m, "StreetFit", "Mobile serving cart with a small storage shelf underneath.", "Mobile · 2-shelf · Stainless top"),
            new("Cup & Flask Set", 3000m, "ChaiWorks", "Reusable glass cup set plus insulated flasks for keeping tea hot through service.", "50-cup set · 2 insulated flasks"),
        }),
        new("Cloud Kitchen", BusinessSegment.SmallBusiness, "Everything to run a delivery-only cloud kitchen.", "🍱", new ExpansionProduct[]
        {
            new("Commercial Induction Cooktop", 18000m, "HeatLine", "Fast-heating induction cooktop suited to a compact delivery-only kitchen.", "2 burners · 3500W · Digital controls"),
            new("Insulated Delivery Bags (Set)", 4000m, "TiffinGo", "Insulated bags that keep food hot through a delivery run.", "Set of 5 · Thermal lining · Strap included"),
            new("Stainless Prep & Packing Table", 15000m, "Ironclad Kitchen", "Prep table sized for packing orders quickly between cooking and pickup.", "304-grade stainless · Under-shelf storage"),
        }),
        new("Home Baking", BusinessSegment.SmallBusiness, "Everything to start a home baking business.", "🧁", new ExpansionProduct[]
        {
            new("Countertop Convection Oven", 22000m, "Bakermax", "Home-scale convection oven with even heat for consistent bakes.", "45L capacity · Convection fan · Up to 250°C"),
            new("Stand Mixer — Home Scale", 12000m, "Doughline", "Stand mixer sized for home-batch cake and dough work.", "5L bowl · 6-speed · Dough hook included"),
            new("Piping & Decorating Kit", 3500m, "SweetTools", "Starter set of tips, bags, and turntables for cake decorating.", "20-tip set · Turntable · Reusable bags"),
        }),
        new("Freelance Repair — Phones, Laptops & Appliances", BusinessSegment.SmallBusiness, "Everything for a solo mobile/laptop/appliance repair business.", "🔧", new ExpansionProduct[]
        {
            new("Precision Repair Toolkit", 5000m, "FixPro", "Full driver and pry-tool set for phone, laptop, and small appliance teardown.", "60-piece set · Anti-static · Carry case"),
            new("Hot Air Rework Station", 9000m, "FixPro", "Hot air station for chip-level repair work on phones and boards.", "Adjustable temp/airflow · Digital display"),
            new("Digital Multimeter & Diagnostic Kit", 4000m, "VoltCheck", "Diagnostic kit for tracing faults before committing to a repair.", "Multimeter · Power probe · Continuity tester"),
        }),
        new("Print-on-Demand", BusinessSegment.SmallBusiness, "Everything to start a print-on-demand apparel/merch business.", "🖨️", new ExpansionProduct[]
        {
            new("Heat Press Machine", 25000m, "PressCo", "Flat heat press for transferring designs onto shirts, mugs, and merch.", "38x38cm plate · Digital timer/temp"),
            new("Sublimation Printer", 35000m, "InkJetPro", "Dedicated sublimation printer for transferring full-colour designs.", "A3 size · Sublimation ink included"),
            new("DTF Transfer Kit", 18000m, "PrintLine", "Direct-to-film transfer kit for printing designs onto dark fabrics.", "DTF film · Powder shaker · Curing tips"),
        }),
        new("Stationery Shop", BusinessSegment.SmallBusiness, "Everything to open a small stationery shop.", "✏️", new ExpansionProduct[]
        {
            new("Display Shelving Set", 15000m, "ShopFit", "Modular shelving for stationery, books, and small goods.", "5-tier · Adjustable · Steel frame"),
            new("Paper Cutter & Laminator", 12000m, "OfficeLine", "Guillotine paper cutter paired with a pouch laminator for print finishing.", "A3 cutter · A3 laminator"),
            new("Stock Starter Pack — Notebooks & Pens", 8000m, "PaperCo", "Opening stock of notebooks, pens, and everyday stationery.", "200+ item starter assortment"),
        }),
        new("Tailoring & Alteration", BusinessSegment.SmallBusiness, "Everything to open a small tailoring and alteration shop.", "🧵", new ExpansionProduct[]
        {
            new("Industrial Sewing Machine", 30000m, "StitchPro", "Heavy-duty machine for daily tailoring and alteration volume.", "Single needle · Servo motor · Table & stand"),
            new("Overlock Machine", 22000m, "StitchPro", "Overlock machine for finishing seams and edges cleanly.", "4-thread · Adjustable stitch length"),
            new("Pressing Table & Steam Iron", 10000m, "PressCo", "Pressing station for finishing garments before pickup.", "Vacuum table · Steam generator iron"),
        }),

        // ---- Big Business ----
        new("Print Shop", BusinessSegment.BigBusiness, "Commercial printing equipment for a full print shop team.", "🖨️", new ExpansionProduct[]
        {
            new("Commercial Offset Printer", 450000m, "PrintMaster", "Offset press for high-volume commercial print runs.", "4-colour · A2 sheet size · High throughput"),
            new("Large Format Plotter", 180000m, "WidePrint", "Wide-format plotter for banners, posters, and signage.", "44\" width · Pigment ink · Roll-fed"),
            new("Industrial Paper Cutter & Binder", 90000m, "OfficeLine", "Heavy-duty cutter and binder for finishing large print jobs.", "Programmable cuts · Perfect binding"),
        }),
        new("Computer & Mobile Repair Shop", BusinessSegment.BigBusiness, "A full repair shop setup for a team handling computers and phones.", "💻", new ExpansionProduct[]
        {
            new("BGA Rework & Reballing Station", 120000m, "ChipFix", "Rework station for chip-level board repair and reballing.", "Infrared preheat · Hot air head · Vision alignment"),
            new("Diagnostic Bench Setup", 60000m, "VoltCheck", "Multi-station diagnostic bench for a repair team working in parallel.", "4 bench stations · Shared power/data rails"),
            new("Parts Inventory Shelving", 30000m, "ShopFit", "Bin shelving for organizing spare parts across many device models.", "120-bin wall unit · Labelled slots"),
        }),
        new("Coffee Roastery", BusinessSegment.BigBusiness, "Commercial roasting equipment for a coffee roastery business.", "🔥", new ExpansionProduct[]
        {
            new("Commercial Drum Roaster — 15kg", 850000m, "RoastTech", "Drum roaster sized for a small-to-mid roastery's daily batch volume.", "15kg batch · Gas-fired · Profile logging"),
            new("Green Bean Storage Silo", 120000m, "RoastTech", "Bulk storage silo for green coffee bean inventory.", "500kg capacity · Moisture-controlled"),
            new("Cupping & QC Station", 45000m, "RoastTech", "Cupping station for quality control across roast batches.", "12-cup setup · Grinder · Scale included"),
        }),

        // ---- Hobbyist ----
        new("3D Printing", BusinessSegment.Hobbyist, "Gear for a 3D printing hobby — FDM and resin.", "🖨️", new ExpansionProduct[]
        {
            new("FDM 3D Printer", 45000m, "PrintForge", "Entry-to-mid FDM printer for everyday hobby prints.", "220x220x250mm bed · Auto bed leveling"),
            new("Filament Dryer Box", 8000m, "PrintForge", "Keeps filament dry for consistent print quality.", "Holds 2 spools · Digital humidity control"),
            new("Resin Printer Starter Kit", 25000m, "ResinWorks", "Resin printer bundle for high-detail miniatures and parts.", "LCD resin printer · Wash & cure station"),
        }),
        new("Electronics Repair", BusinessSegment.Hobbyist, "A hobby electronics repair and tinkering bench.", "🔌", new ExpansionProduct[]
        {
            new("Bench Power Supply", 12000m, "VoltCheck", "Variable-voltage bench supply for testing and repair work.", "0-30V · 0-5A · Dual digital display"),
            new("Component Tester Kit", 6000m, "VoltCheck", "Quick-test kit for identifying and checking passive/active components.", "Auto-detect · LCD readout"),
            new("Anti-Static Repair Mat & Tools", 4000m, "FixPro", "ESD-safe mat and basic tool set for safe board-level work.", "ESD mat · Wrist strap · Basic tool set"),
        }),
        new("Soldering & PCB Work", BusinessSegment.Hobbyist, "A hobby soldering and PCB prototyping bench.", "🔧", new ExpansionProduct[]
        {
            new("Soldering Station — Temperature Controlled", 8000m, "WeldTech", "Adjustable-temperature soldering iron and stand for precise work.", "200-450°C · Digital display · Sponge & stand"),
            new("Hot Air Rework Station", 9000m, "WeldTech", "Hot air station for SMD work and small rework jobs.", "Adjustable temp/airflow · Nozzle set"),
            new("PCB Fabrication Starter Kit", 15000m, "CircuitLab", "Starter kit for etching and assembling simple custom boards at home.", "Etchant · Copper boards · UV exposure box"),
        }),
        new("Laser Engraving", BusinessSegment.Hobbyist, "A hobby laser engraving and cutting setup.", "🔥", new ExpansionProduct[]
        {
            new("Diode Laser Engraver", 55000m, "BeamWorks", "Diode laser engraver for wood, leather, and acrylic hobby projects.", "10W module · 400x400mm work area"),
            new("Fume Extraction Unit", 15000m, "BeamWorks", "Extracts smoke and fumes during engraving sessions.", "Inline fan · Carbon filter · Ducting included"),
            new("Engraving Material Starter Pack", 5000m, "CraftSupply", "Assorted wood, acrylic, and leather blanks to start engraving.", "20-piece mixed material pack"),
        }),
        new("CNC", BusinessSegment.Hobbyist, "A hobby desktop CNC routing setup.", "🪚", new ExpansionProduct[]
        {
            new("Desktop CNC Router", 90000m, "RouteTech", "Desktop CNC router for hobby woodworking and prototyping.", "300x300x100mm work area · 3-axis"),
            new("Spindle & Bit Set", 20000m, "RouteTech", "Upgraded spindle plus a set of common cutting bits.", "300W spindle · 20-bit set"),
            new("Dust Collection System", 18000m, "RouteTech", "Dust shoe and vacuum setup to keep a home CNC station clean.", "Dust shoe · Compact vacuum · Hose kit"),
        }),
        new("Home Brewing", BusinessSegment.Hobbyist, "A home brewing hobby setup.", "🍺", new ExpansionProduct[]
        {
            new("Fermenter & Airlock Set", 12000m, "BrewCraft", "Fermenting vessel with airlock for consistent home brews.", "30L fermenter · Airlock · Thermometer strip"),
            new("Home Brewing Kettle", 8000m, "BrewCraft", "Brew kettle sized for standard home-batch brewing.", "20L · Stainless steel · Built-in thermometer"),
            new("Kegging System", 20000m, "BrewCraft", "Kegging setup for carbonating and serving finished brews.", "2-keg system · CO2 regulator · Taps included"),
        }),
        new("Woodworking", BusinessSegment.Hobbyist, "A home woodworking hobby shop.", "🪚", new ExpansionProduct[]
        {
            new("Table Saw — Benchtop", 60000m, "Woodline", "Benchtop table saw for hobby-scale woodworking projects.", "254mm blade · Rip fence · Folding stand"),
            new("Hand Plane & Chisel Set", 8000m, "Woodline", "Core hand tools for finishing and joinery work.", "3 planes · 6-piece chisel set"),
            new("Wood Lathe", 45000m, "Woodline", "Benchtop lathe for turning bowls, spindles, and small projects.", "Variable speed · 350mm swing"),
        }),
        new("Leather Crafting", BusinessSegment.Hobbyist, "A leather crafting hobby setup.", "🥾", new ExpansionProduct[]
        {
            new("Leather Cutting & Stitching Kit", 8000m, "HideCraft", "Core cutting and hand-stitching tools for leather goods.", "Rotary cutter · Stitching pony · Needle set"),
            new("Leather Skiving Machine", 25000m, "HideCraft", "Thins leather edges evenly for folding and stitching.", "Adjustable thickness · Electric motor"),
            new("Edge Beveling & Finishing Tools", 4000m, "HideCraft", "Tools for smoothing and finishing raw leather edges.", "Bevel set · Burnisher · Edge paint"),
        }),
        new("Sewing & Textiles", BusinessSegment.Hobbyist, "A home sewing and textile crafting hobby setup.", "🧵", new ExpansionProduct[]
        {
            new("Home Sewing Machine", 15000m, "StitchPro", "Everyday sewing machine for garment and craft projects.", "20 stitch patterns · Automatic threading"),
            new("Embroidery Machine", 40000m, "StitchPro", "Machine embroidery for custom designs on fabric.", "USB pattern import · 5x7\" hoop"),
            new("Fabric Cutting Table", 10000m, "StitchPro", "Large flat surface for laying out and cutting fabric.", "150x90cm · Folding legs · Grid marked"),
        }),
        new("Candle, Soap & Resin Making", BusinessSegment.Hobbyist, "A candle, soap, and resin craft hobby setup.", "🕯️", new ExpansionProduct[]
        {
            new("Wax Melting Pot & Pouring Set", 6000m, "WaxWorks", "Double-boiler pot and pouring tools for candle making.", "4L pot · Thermometer · Pouring pitcher"),
            new("Soap Mold & Curing Rack Set", 4000m, "WaxWorks", "Silicone molds and a curing rack for soap batches.", "12-cavity mold set · Curing rack"),
            new("UV Resin Curing Lamp Kit", 5000m, "ResinWorks", "UV lamp and tools for curing resin art and jewelry.", "36W UV lamp · Mixing cups · Stir sticks"),
        }),
        new("Sticker & Vinyl Cutting", BusinessSegment.Hobbyist, "A sticker and vinyl cutting hobby setup.", "🏷️", new ExpansionProduct[]
        {
            new("Vinyl Cutting Plotter", 18000m, "CutLine", "Cutting plotter for stickers, decals, and vinyl designs.", "34cm cutting width · USB/Bluetooth"),
            new("Sticker Laminator", 8000m, "CutLine", "Pouch laminator for durable, weatherproof stickers.", "A4 width · Hot/cold settings"),
            new("Design & Weeding Tool Kit", 3000m, "CutLine", "Hand tools for weeding and applying vinyl designs.", "Weeding hooks · Squeegee · Transfer tape"),
        }),
        new("Drone & RC Building", BusinessSegment.Hobbyist, "A drone and RC building hobby setup.", "🚁", new ExpansionProduct[]
        {
            new("FPV Drone Build Kit", 25000m, "SkyForge", "Parts kit for building a custom FPV racing drone.", "Frame · Motors · Flight controller · ESCs"),
            new("Transmitter & Receiver Set", 12000m, "SkyForge", "Radio control set for flying custom-built drones and RC models.", "16-channel · Telemetry · Long range"),
            new("Soldering & Field Repair Kit", 5000m, "SkyForge", "Compact soldering and repair kit for on-site fixes.", "Portable iron · Wire set · Spare connectors"),
        }),
    };

    // Runs independently of SeedCatalog's guard, so it also fills these in for
    // a DB that already has just the original 4 categories. Skips any category
    // that already exists by name, so it's safe to call on every startup.
    private static void SeedExpansionCatalog(ApplicationDbContext db)
    {
        var existingNames = db.BusinessTypes.Select(bt => bt.Name).ToHashSet();

        foreach (var category in Expansion)
        {
            if (existingNames.Contains(category.Name)) continue;

            var bt = new BusinessType
            {
                Name = category.Name,
                Segment = category.Segment,
                ShortDescription = category.ShortDescription,
                IconOrImagePath = category.Icon
            };
            db.BusinessTypes.Add(bt);
            db.SaveChanges(); // need the Id before attaching products

            foreach (var p in category.Products)
            {
                db.Products.Add(new Product
                {
                    Name = p.Name,
                    Price = p.Price,
                    BusinessTypeId = bt.Id,
                    Manufacturer = p.Manufacturer,
                    Description = p.Description,
                    Specs = p.Specs,
                    SellerType = ListingSellerType.Platform,
                    Condition = ProductCondition.New,
                    IsActive = true
                });
            }
            db.SaveChanges();
        }
    }

    // Runs independently of SeedCatalog's guard, so it also fills in bundles
    // for anyone who already has products (e.g. an existing dev DB) — as
    // long as no bundles exist yet. Picks tiers from whatever products
    // actually exist per business type rather than hardcoding names, so it
    // stays correct even after products are renamed/added via the admin.
    private static void SeedBundles(ApplicationDbContext db)
    {
        // Per-category, not global: an existing DB may already have bundles
        // for its original categories but none yet for newly-added ones, so
        // a single "any bundles at all?" guard would skip those forever.
        var businessTypes = db.BusinessTypes
            .Where(bt => !db.ProductBundles.Any(pb => pb.BusinessTypeId == bt.Id))
            .ToList();

        foreach (var bt in businessTypes)
        {
            var products = db.Products
                .Where(p => p.BusinessTypeId == bt.Id && p.SellerType == ListingSellerType.Platform && p.IsActive)
                .OrderBy(p => p.Price)
                .ToList();

            if (products.Count < 2) continue;

            var starterCount = Math.Max(2, products.Count / 2);
            var starterItems = products.Take(starterCount).ToList();

            db.ProductBundles.Add(new ProductBundle
            {
                Name = $"Starter {bt.Name} Kit",
                Description = $"The essentials to open a small, single-room {bt.Name.ToLowerInvariant()} — lower-cost equipment to get running without overcommitting.",
                BusinessTypeId = bt.Id,
                IsActive = true,
                Items = starterItems.Select(p => new BundleItem { ProductId = p.Id, Quantity = 1 }).ToList()
            });

            if (products.Count >= 3)
            {
                db.ProductBundles.Add(new ProductBundle
                {
                    Name = $"Enterprise {bt.Name} Setup",
                    Description = $"A full, higher-capacity setup for a larger {bt.Name.ToLowerInvariant()} handling more volume and guests.",
                    BusinessTypeId = bt.Id,
                    IsActive = true,
                    Items = products.Select(p => new BundleItem { ProductId = p.Id, Quantity = 1 }).ToList()
                });
            }
        }

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
