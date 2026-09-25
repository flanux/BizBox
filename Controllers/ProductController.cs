using Bizbox.Data;
using Bizbox.Models;
using Bizbox.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductController(ApplicationDbContext db, ICartService cartService, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _cartService = cartService;
        _userManager = userManager;
    }

    // GET /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.BusinessType)
            .Include(p => p.Supersedes)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        return View(product);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            await _cartService.AddToCartAsync(userId, productId);
        }
        return RedirectToAction(nameof(Details), new { id = productId });
    }
}
