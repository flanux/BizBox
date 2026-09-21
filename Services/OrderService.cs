using Bizbox.Data;
using Bizbox.Models;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Services;

public interface IOrderService
{
    Task<Order> CreateOrderFromCartAsync(string userId, PaymentMethod method);
    Task<Order?> GetOrderAsync(int orderId, string userId);
    Task MarkOrderPaidAsync(int orderId);
    Task<List<Order>> GetOrdersForUserAsync(string userId);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly ICartService _cartService;

    public OrderService(ApplicationDbContext db, ICartService cartService)
    {
        _db = db;
        _cartService = cartService;
    }

    public async Task<Order> CreateOrderFromCartAsync(string userId, PaymentMethod method)
    {
        var cartItems = await _cartService.GetCartAsync(userId);
        if (!cartItems.Any())
            throw new InvalidOperationException("Cannot check out an empty cart.");

        var order = new Order
        {
            UserId = userId,
            PaymentMethod = method,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = cartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPriceAtPurchase = ci.Product?.Price ?? 0
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.UnitPriceAtPurchase * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // NOTE: cart is intentionally NOT cleared here. It's only cleared once
        // payment is actually confirmed (see OrderService.MarkOrderPaidAsync
        // and PaymentController) — so a cancelled/failed payment leaves the
        // cart intact for the user to retry instead of losing their selection.

        return order;
    }

    public async Task<Order?> GetOrderAsync(int orderId, string userId)
    {
        return await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }

    public async Task MarkOrderPaidAsync(int orderId)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Processing; // Pending -> Processing once payment confirmed
            await _db.SaveChangesAsync();
            await _cartService.ClearCartAsync(order.UserId); // only clear cart on confirmed payment
        }
    }

    public async Task<List<Order>> GetOrdersForUserAsync(string userId)
    {
        return await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
