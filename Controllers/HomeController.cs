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

    public async Task<IActionResult> Index(int? category, string? q)
    {
        var businessTypes = await _db.BusinessTypes.ToListAsync();

        var productsQuery = _db.Products
            .Include(p => p.BusinessType)
            .Where(p => p.IsActive && p.SellerType == ListingSellerType.Platform)
            .AsQueryable();

        if (category.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.BusinessTypeId == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            productsQuery = productsQuery.Where(p => p.Name.Contains(q) || p.Description.Contains(q));
        }

        var products = await productsQuery.OrderBy(p => p.BusinessTypeId).ThenBy(p => p.Name).ToListAsync();

        ViewBag.BusinessTypes = businessTypes;
        ViewBag.SelectedCategory = category;
        ViewBag.SearchTerm = q;

        return View(products);
    }

    public IActionResult About() => View();

    public IActionResult Error() => View();
}
