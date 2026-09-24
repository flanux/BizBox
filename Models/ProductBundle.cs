using System.ComponentModel.DataAnnotations;

namespace Bizbox.Models;

// A curated set of existing Products sold together — a "Starter Cafe Kit",
// an "Enterprise Setup", or a single-manufacturer lineup. Price is never
// stored here; it's always the live sum of its items' current Product.Price,
// so editing a product's price keeps every bundle it's in accurate.
public class ProductBundle
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    public int BusinessTypeId { get; set; }
    public BusinessType? BusinessType { get; set; }

    public bool IsActive { get; set; } = true;

    public List<BundleItem> Items { get; set; } = new();
}

// One product inside a bundle, with how many of it the bundle includes.
public class BundleItem
{
    public int Id { get; set; }

    public int ProductBundleId { get; set; }
    public ProductBundle? ProductBundle { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; } = 1;
}
