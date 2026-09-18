using System.ComponentModel.DataAnnotations;

namespace Bizbox.Models;

// One of the 4 fixed business types: Coffee Shop, Bakery, Restaurant, Salon.
// Scope note: exactly these 4 for this project — see project context doc.
public class BusinessType
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? IconOrImagePath { get; set; }

    // Navigation: all catalog products that belong to this business type's starter kit
    public List<Product> Products { get; set; } = new();
}
