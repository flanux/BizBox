using Bizbox.Data;
using Bizbox.Models;
using Bizbox.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AdminController> _logger;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxImageBytes = 5 * 1024 * 1024;

    public AdminController(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        ILogger<AdminController> logger)
    {
        _db = db;
        _environment = environment;
        _logger = logger;
    }

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
}
