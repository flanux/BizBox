using Bizbox.Data;
using Bizbox.Models;
using Bizbox.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

public class BusinessTypeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessTypeController(ApplicationDbContext db, ICartService cartService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _cartService = cartService;
        _userManager = userManager;
    }

    // GET /BusinessType/Catalog/5
    public async Task<IActionResult> Catalog(int id)
    {
        var businessType = await _db.BusinessTypes
            .Include(bt => bt.Products)
            .FirstOrDefaultAsync(bt => bt.Id == id);

        if (businessType == null) return NotFound();

        // Show both new (platform) and resale listings together — one browsing
        // surface per business type, distinguished only by badge on the card.
        businessType.Products = businessType.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.SellerType) // platform (0) listings first, then resale (1)
            .ToList();

        return View(businessType);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int businessTypeId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            await _cartService.AddToCartAsync(userId, productId);
        }
        return RedirectToAction("Catalog", new { id = businessTypeId });
    }
}
