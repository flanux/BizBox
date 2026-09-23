using Bizbox.Data;
using Bizbox.Models;
using Bizbox.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const string AdminRole = "Admin";

    public AdminController(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        ILogger<AdminController> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _environment = environment;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ================= Dashboard =================

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            ProductCount = await _db.Products.CountAsync(p => p.SellerType == ListingSellerType.Platform),
            ActiveProductCount = await _db.Products.CountAsync(p => p.SellerType == ListingSellerType.Platform && p.IsActive),
            BusinessTypeCount = await _db.BusinessTypes.CountAsync(),
            UserCount = await _db.Users.CountAsync(),
            OrderCount = await _db.Orders.CountAsync(),
            OrderValue = await _db.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,
            RecentProducts = await _db.Products
                .AsNoTracking()
                .Include(p => p.BusinessType)
                .Where(p => p.SellerType == ListingSellerType.Platform)
                .OrderByDescending(p => p.Id)
                .Take(10)
                .ToListAsync()
        };

        return View(model);
    }

    // ================= Products =================

    public async Task<IActionResult> Products()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.BusinessType)
            .Where(p => p.SellerType == ListingSellerType.Platform)
            .OrderBy(p => p.BusinessTypeId)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AdminProductFormViewModel();
        await PopulateProductFormAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductFormViewModel model)
    {
        await PopulateProductFormAsync(model);

        if (!ModelState.IsValid)
            return View(model);

        var imagePath = await SaveImageAsync(model.Image);
        if (model.Image != null && imagePath == null)
            return View(model);

        var product = new Product
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            Manufacturer = Clean(model.Manufacturer),
            Specs = Clean(model.Specs),
            ImagePath = imagePath,
            Price = model.Price,
            BusinessTypeId = model.BusinessTypeId,
            SellerType = ListingSellerType.Platform,
            Condition = ProductCondition.New,
            SupersedesProductId = model.SupersedesProductId,
            IsActive = model.IsActive
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Product '{product.Name}' created.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerType == ListingSellerType.Platform);

        if (product == null)
            return NotFound();

        var model = new AdminProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Manufacturer = product.Manufacturer,
            Specs = product.Specs,
            Price = product.Price,
            BusinessTypeId = product.BusinessTypeId,
            SupersedesProductId = product.SupersedesProductId,
            IsActive = product.IsActive,
            ExistingImagePath = product.ImagePath
        };

        await PopulateProductFormAsync(model, product.Id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminProductFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerType == ListingSellerType.Platform);

        if (product == null)
            return NotFound();

        await PopulateProductFormAsync(model, product.Id);
        model.ExistingImagePath = product.ImagePath;

        if (!ModelState.IsValid)
            return View(model);

        var oldImagePath = product.ImagePath;
        var newImagePath = await SaveImageAsync(model.Image);
        if (model.Image != null && newImagePath == null)
            return View(model);

        product.Name = model.Name.Trim();
        product.Description = model.Description?.Trim() ?? string.Empty;
        product.Manufacturer = Clean(model.Manufacturer);
        product.Specs = Clean(model.Specs);
        product.Price = model.Price;
        product.BusinessTypeId = model.BusinessTypeId;
        product.SupersedesProductId = model.SupersedesProductId;
        product.IsActive = model.IsActive;

        if (newImagePath != null)
            product.ImagePath = newImagePath;

        await _db.SaveChangesAsync();

        if (newImagePath != null)
            DeleteLocalImage(oldImagePath);

        TempData["AdminMessage"] = $"Product '{product.Name}' updated.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerType == ListingSellerType.Platform);

        if (product == null)
            return NotFound();

        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Product '{product.Name}' is now {(product.IsActive ? "active" : "inactive")}.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerType == ListingSellerType.Platform);

        if (product == null)
            return NotFound();

        var isReferenced = await _db.Products.AnyAsync(p => p.SupersedesProductId == id)
            || await _db.OrderItems.AnyAsync(oi => oi.ProductId == id)
            || await _db.CartItems.AnyAsync(ci => ci.ProductId == id);

        if (isReferenced)
        {
            TempData["AdminMessage"] = $"'{product.Name}' is referenced by an order, cart, or another product, so it was deactivated instead of deleted.";
            product.IsActive = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        var imagePath = product.ImagePath;
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        DeleteLocalImage(imagePath);

        TempData["AdminMessage"] = $"Product '{product.Name}' deleted.";
        return RedirectToAction(nameof(Products));
    }

    private async Task PopulateProductFormAsync(AdminProductFormViewModel model, int? currentProductId = null)
    {
        model.BusinessTypes = await _db.BusinessTypes
            .AsNoTracking()
            .OrderBy(bt => bt.Name)
            .ToListAsync();

        model.ExistingProducts = await _db.Products
            .AsNoTracking()
            .Where(p => p.SellerType == ListingSellerType.Platform && (!currentProductId.HasValue || p.Id != currentProductId.Value))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    private async Task<string?> SaveImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0)
            return null;

        if (image.Length > MaxImageBytes)
        {
            ModelState.AddModelError(nameof(AdminProductFormViewModel.Image), "Image must be 5 MB or smaller.");
            return null;
        }

        var extension = Path.GetExtension(image.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(AdminProductFormViewModel.Image), "Only JPG, JPEG, PNG, and WEBP images are allowed.");
            return null;
        }

        var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await image.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }

    private void DeleteLocalImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !imagePath.StartsWith("/uploads/products/", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            var relative = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relative);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete old product image {ImagePath}", imagePath);
        }
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // ================= Business types (categories) =================

    public async Task<IActionResult> BusinessTypes()
    {
        var rows = await _db.BusinessTypes
            .AsNoTracking()
            .OrderBy(bt => bt.Name)
            .Select(bt => new AdminBusinessTypeRowViewModel
            {
                Id = bt.Id,
                Name = bt.Name,
                ShortDescription = bt.ShortDescription,
                ProductCount = bt.Products.Count
            })
            .ToListAsync();

        return View(rows);
    }

    [HttpGet]
    public IActionResult BusinessTypeCreate()
    {
        return View(new AdminBusinessTypeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BusinessTypeCreate(AdminBusinessTypeFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var businessType = new BusinessType
        {
            Name = model.Name.Trim(),
            ShortDescription = model.ShortDescription?.Trim() ?? string.Empty
        };

        _db.BusinessTypes.Add(businessType);
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Category '{businessType.Name}' created.";
        return RedirectToAction(nameof(BusinessTypes));
    }

    [HttpGet]
    public async Task<IActionResult> BusinessTypeEdit(int id)
    {
        var businessType = await _db.BusinessTypes.FindAsync(id);
        if (businessType == null)
            return NotFound();

        var model = new AdminBusinessTypeFormViewModel
        {
            Id = businessType.Id,
            Name = businessType.Name,
            ShortDescription = businessType.ShortDescription
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BusinessTypeEdit(int id, AdminBusinessTypeFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var businessType = await _db.BusinessTypes.FindAsync(id);
        if (businessType == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        businessType.Name = model.Name.Trim();
        businessType.ShortDescription = model.ShortDescription?.Trim() ?? string.Empty;
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Category '{businessType.Name}' updated.";
        return RedirectToAction(nameof(BusinessTypes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BusinessTypeDelete(int id)
    {
        var businessType = await _db.BusinessTypes.FindAsync(id);
        if (businessType == null)
            return NotFound();

        var hasProducts = await _db.Products.AnyAsync(p => p.BusinessTypeId == id);
        if (hasProducts)
        {
            TempData["AdminMessage"] = $"'{businessType.Name}' still has products assigned to it, so it can't be deleted. Move or remove those products first.";
            return RedirectToAction(nameof(BusinessTypes));
        }

        _db.BusinessTypes.Remove(businessType);
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Category '{businessType.Name}' deleted.";
        return RedirectToAction(nameof(BusinessTypes));
    }

    // ================= Orders =================

    public async Task<IActionResult> Orders()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Order #{order.Id} marked as {order.Status}.";
        return RedirectToAction(nameof(Orders));
    }

    // ================= Users =================

    public async Task<IActionResult> Users()
    {
        var currentUserId = _userManager.GetUserId(User);
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync();
        var orderCounts = await _db.Orders
            .GroupBy(o => o.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.UserId, g => g.Count);

        var rows = new List<AdminUserRowViewModel>();
        foreach (var user in users)
        {
            rows.Add(new AdminUserRowViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "—",
                DisplayName = user.DisplayName,
                IsAdmin = await _userManager.IsInRoleAsync(user, AdminRole),
                IsCurrentUser = user.Id == currentUserId,
                OrderCount = orderCounts.TryGetValue(user.Id, out var c) ? c : 0
            });
        }

        return View(rows);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["AdminMessage"] = "You can't change your own admin access.";
            return RedirectToAction(nameof(Users));
        }

        if (!await _roleManager.RoleExistsAsync(AdminRole))
            await _roleManager.CreateAsync(new IdentityRole(AdminRole));

        if (await _userManager.IsInRoleAsync(user, AdminRole))
        {
            await _userManager.RemoveFromRoleAsync(user, AdminRole);
            TempData["AdminMessage"] = $"{user.Email} is no longer a Super Admin.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, AdminRole);
            TempData["AdminMessage"] = $"{user.Email} is now a Super Admin.";
        }

        return RedirectToAction(nameof(Users));
    }
}
