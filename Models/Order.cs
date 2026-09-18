using System.ComponentModel.DataAnnotations.Schema;

namespace Bizbox.Models;

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3
}

public enum PaymentMethod
{
    Esewa = 0,
    Khalti = 1,
    InternationalCard_Simulated = 2   // UI-only simulated flow — see project context doc, Section 5
}

public class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    // Snapshot the price at time of purchase — never rely on Product.Price later,
    // since that can change or the product could be removed.
    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPriceAtPurchase { get; set; }
}
