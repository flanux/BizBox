namespace Bizbox.ViewModels;

public class AdminUserRowViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsCurrentUser { get; set; }
    public int OrderCount { get; set; }
}
