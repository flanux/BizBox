using System.ComponentModel.DataAnnotations;

namespace Bizbox.ViewModels;

public class AdminBusinessTypeFormViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string ShortDescription { get; set; } = string.Empty;
}
