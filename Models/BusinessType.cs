using System.ComponentModel.DataAnnotations;

namespace Bizbox.Models;

// A shopping category, tagged with which segment it belongs to
// (small business / big business / hobbyist — see BusinessSegment).
public class BusinessType
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? IconOrImagePath { get; set; }

    public BusinessSegment Segment { get; set; } = BusinessSegment.SmallBusiness;

    // Navigation: all catalog products that belong to this business type's starter kit
    public List<Product> Products { get; set; } = new();
}
