using Bizbox.Data;
using Bizbox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

// Simulated manufacturer/supplier role — admin pushes new listings and marks
// old ones as superseded. See project context doc, Section 6.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products
            .Include(p => p.BusinessType)
            .Where(p => p.SellerType == ListingSellerType.Platform)
            .ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.BusinessTypes = await _db.BusinessTypes.ToListAsync();
        ViewBag.ExistingProducts = await _db.Products
            .Where(p => p.SellerType == ListingSellerType.Platform)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, string description, decimal price, int businessTypeId, string? manufacturer, string? specs, string? imagePath, int? supersedesProductId)
    {
        var product = new Product
        {
            Name = name,
            Description = description,
            Manufacturer = manufacturer,
            Specs = specs,
            ImagePath = imagePath,
            Price = price,
            BusinessTypeId = businessTypeId,
            SellerType = ListingSellerType.Platform,
            Condition = ProductCondition.New,
            SupersedesProductId = supersedesProductId,
            IsActive = true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            product.IsActive = !product.IsActive;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
