namespace Bizbox.ViewModels;

public class AdminBundleRowViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BusinessTypeName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsActive { get; set; }
}
