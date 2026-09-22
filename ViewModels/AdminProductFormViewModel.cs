using System.ComponentModel.DataAnnotations;
using Bizbox.Models;
using Microsoft.AspNetCore.Http;

namespace Bizbox.ViewModels;

public class AdminProductFormViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(600)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Manufacturer { get; set; }

    [MaxLength(400)]
    public string? Specs { get; set; }

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    [Required]
    public int BusinessTypeId { get; set; }

    public int? SupersedesProductId { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ExistingImagePath { get; set; }

    public IFormFile? Image { get; set; }

    public List<BusinessType> BusinessTypes { get; set; } = new();
    public List<Product> ExistingProducts { get; set; } = new();
}
