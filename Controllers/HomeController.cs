using Bizbox.Data;
using Bizbox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Default: show a mini-grid of a few products under each business type,
    // right on the home page, so a first-time visitor sees real equipment
    // and prices without an extra click.
    public async Task<IActionResult> Index(string? q)
    {
        var businessTypes = await _db.BusinessTypes.ToListAsync();

        var allProducts = await _db.Products
            .Include(p => p.BusinessType)
            .Where(p => p.IsActive && p.SellerType == ListingSellerType.Platform)
            .Where(p => string.IsNullOrEmpty(q) || p.Name.Contains(q) || p.Description.Contains(q))
            .OrderBy(p => p.BusinessTypeId).ThenBy(p => p.Name)
            .ToListAsync();

        // Group products by business type for the per-category mini-grids.
        var byCategory = businessTypes.ToDictionary(
            bt => bt.Id,
            bt => allProducts.Where(p => p.BusinessTypeId == bt.Id).Take(4).ToList()
        );

        ViewBag.BusinessTypes = businessTypes;
        ViewBag.ProductsByCategory = byCategory;
        ViewBag.SearchTerm = q;

        // If searching, also return a flat filtered list so results aren't
        // hidden inside per-category groups of only 4.
        ViewBag.SearchResults = string.IsNullOrWhiteSpace(q) ? null : allProducts;

        return View();
    }

    public IActionResult About() => View();

    public IActionResult Error() => View();
}
