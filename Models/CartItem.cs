namespace Bizbox.Models;

// A single line in a user's shopping cart. Kept simple: one row per product per user.
public class CartItem
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; } = 1;
}
