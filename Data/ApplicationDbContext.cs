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
    public DbSet<ProductBundle> ProductBundles => Set<ProductBundle>();
    public DbSet<BundleItem> BundleItems => Set<BundleItem>();

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

        // Deleting a bundle takes its line items with it...
        builder.Entity<BundleItem>()
            .HasOne(bi => bi.ProductBundle)
            .WithMany(pb => pb.Items)
            .HasForeignKey(bi => bi.ProductBundleId)
            .OnDelete(DeleteBehavior.Cascade);

        // ...but a product referenced by a bundle can't be hard-deleted out from under it.
        builder.Entity<BundleItem>()
            .HasOne(bi => bi.Product)
            .WithMany()
            .HasForeignKey(bi => bi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductBundle>()
            .HasOne(pb => pb.BusinessType)
            .WithMany()
            .HasForeignKey(pb => pb.BusinessTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
