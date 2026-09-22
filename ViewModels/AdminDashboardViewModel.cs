using Bizbox.Models;

namespace Bizbox.ViewModels;

public class AdminDashboardViewModel
{
    public int ProductCount { get; set; }
    public int ActiveProductCount { get; set; }
    public int BusinessTypeCount { get; set; }
    public int UserCount { get; set; }
    public int OrderCount { get; set; }
    public decimal OrderValue { get; set; }
    public List<Product> RecentProducts { get; set; } = new();
}
