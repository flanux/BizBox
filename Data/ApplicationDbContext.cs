using Bizbox.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<BusinessType> BusinessTypes => Set<BusinessType>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Prevent a delete cascade cycle between Product.Supersedes -> Product
        builder.Entity<Product>()
            .HasOne(p => p.Supersedes)
            .WithMany()
            .HasForeignKey(p => p.SupersedesProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(p => p.BusinessType)
            .WithMany(bt => bt.Products)
            .HasForeignKey(p => p.BusinessTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
