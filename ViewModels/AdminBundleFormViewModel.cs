using System.ComponentModel.DataAnnotations;
using Bizbox.Models;

namespace Bizbox.ViewModels;

public class AdminBundleFormViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Choose a category")]
    public int BusinessTypeId { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? Image { get; set; }
    public string? ExistingImagePath { get; set; }

    // Ids of the products checked in the form. A bundle always includes
    // one of each selected product — no per-item quantity picker, to keep
    // the admin form simple.
    public List<int> SelectedProductIds { get; set; } = new();

    // Populated by the controller before the view renders.
    public List<BusinessType> BusinessTypes { get; set; } = new();
    public List<Product> AvailableProducts { get; set; } = new();
}
