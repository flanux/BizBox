using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bizbox.Models;

public enum ListingSellerType
{
    Platform = 0,   // part of a curated starter kit, sold by Bizbox itself
    User = 1        // resale/trade-in listing, sold by another user
}

public enum ProductCondition
{
    New = 0,
    LikeNew = 1,
    Good = 2,
    Fair = 3
}

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(600)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Manufacturer { get; set; }

    // Short spec lines, one per line (e.g. "230V · 15A", "2.5L tank", "Stainless steel body").
    // Kept as free text rather than a separate table — this project's scale doesn't need
    // structured spec attributes, and free text is simplest for the seeded catalog to show.
    [MaxLength(400)]
    public string? Specs { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public string? ImagePath { get; set; }

    public int BusinessTypeId { get; set; }
    public BusinessType? BusinessType { get; set; }

    // ---- Pillar 2: resale/trade-in ----
    public ListingSellerType SellerType { get; set; } = ListingSellerType.Platform;
    public ProductCondition Condition { get; set; } = ProductCondition.New;

    // If SellerType == User, this is who listed it. Null for platform listings.
    public string? SellerUserId { get; set; }
    public ApplicationUser? SellerUser { get; set; }

    // ---- Pillar 3: "new model available" notifications ----
    // If this product is a newer model that replaces an older one,
    // SupersedesProductId points at the old product it replaces.
    public int? SupersedesProductId { get; set; }
    public Product? Supersedes { get; set; }

    public bool IsActive { get; set; } = true;
}
