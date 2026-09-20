using Bizbox.Data;
using Bizbox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

// Pillar 3: "new model available" notifications. Computed on the fly rather than
// stored as a separate table — for any product the current user has purchased,
// check whether a newer product exists that supersedes it.
[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var purchasedProductIds = await _db.OrderItems
            .Where(oi => oi.Order!.UserId == userId)
            .Select(oi => oi.ProductId)
            .Distinct()
            .ToListAsync();

        var upgrades = await _db.Products
            .Where(p => p.SupersedesProductId != null && purchasedProductIds.Contains(p.SupersedesProductId.Value))
            .Include(p => p.Supersedes)
            .ToListAsync();

        return View(upgrades);
    }
}
