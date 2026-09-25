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

    // Discovery homepage: a segment picker (Small Business / Big Business /
    // Hobbyist) up top, a category grid per segment, and a short "Popular
    // bundles" teaser row. Search bypasses all of that and shows a flat
    // results grid instead.
    public async Task<IActionResult> Index(string? q)
    {
        var businessTypes = await _db.BusinessTypes
            .Include(bt => bt.Products.Where(p => p.IsActive))
            .OrderBy(bt => bt.Name)
            .ToListAsync();

        ViewBag.BusinessTypes = businessTypes;
        ViewBag.SearchTerm = q;

        if (!string.IsNullOrWhiteSpace(q))
        {
            var results = await _db.Products
                .Include(p => p.BusinessType)
                .Where(p => p.IsActive && p.SellerType == ListingSellerType.Platform)
                .Where(p => p.Name.Contains(q) || p.Description.Contains(q))
                .OrderBy(p => p.BusinessTypeId).ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.SearchResults = results;
            return View();
        }

        ViewBag.SearchResults = null;

        // A handful of bundles across every category for the homepage teaser
        // row — cheapest few items in memory, not a full browse experience;
        // "see all" on each category page covers the rest.
        var teaserBundles = await _db.ProductBundles
            .AsNoTracking()
            .Include(b => b.BusinessType)
            .Include(b => b.Items).ThenInclude(i => i.Product)
            .Where(b => b.IsActive)
            .ToListAsync();

        ViewBag.TeaserBundles = teaserBundles
            .OrderBy(b => b.Items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity))
            .Take(8)
            .ToList();

        return View();
    }

    public IActionResult About() => View();

    public IActionResult Error() => View();
}
