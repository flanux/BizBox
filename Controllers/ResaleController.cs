using Bizbox.Data;
using Bizbox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

// Pillar 2: resale/trade-in marketplace. Seller = another user, not the platform.
public class ResaleController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResaleController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Anyone can browse resale listings, logged in or not.
    public async Task<IActionResult> Index()
    {
        var listings = await _db.Products
            .Include(p => p.BusinessType)
            .Where(p => p.SellerType == ListingSellerType.User && p.IsActive)
            .ToListAsync();
        return View(listings);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        ViewBag.BusinessTypes = await _db.BusinessTypes.ToListAsync();
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(string name, string description, decimal price, int businessTypeId, ProductCondition condition)
    {
        var userId = _userManager.GetUserId(User)!;

        var listing = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            BusinessTypeId = businessTypeId,
            Condition = condition,
            SellerType = ListingSellerType.User,
            SellerUserId = userId,
            IsActive = true
        };

        _db.Products.Add(listing);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
