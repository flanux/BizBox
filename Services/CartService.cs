using Bizbox.Data;
using Bizbox.Models;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Services;

// Business logic lives here, not in controllers. Controllers just call this
// and translate the result into a view/redirect.
public interface ICartService
{
    Task AddToCartAsync(string userId, int productId, int quantity = 1);
    Task<List<CartItem>> GetCartAsync(string userId);
    Task RemoveFromCartAsync(string userId, int cartItemId);
    Task<decimal> GetCartTotalAsync(string userId);
    Task ClearCartAsync(string userId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;

    public CartService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddToCartAsync(string userId, int productId, int quantity = 1)
    {
        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _db.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = quantity });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<CartItem>> GetCartAsync(string userId)
    {
        return await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task RemoveFromCartAsync(string userId, int cartItemId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetCartTotalAsync(string userId)
    {
        var items = await GetCartAsync(userId);
        return items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity);
    }

    public async Task ClearCartAsync(string userId)
    {
        var items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}
