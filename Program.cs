using Bizbox.Data;
using Bizbox.Models;
using Bizbox.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Database ----
// Using SQLite for now: zero external setup, works instantly in Codespaces.
// Swap to SQL Server / Postgres later by changing this line + the connection string
// in appsettings.json if you move to a real hosting DB (e.g. Azure SQL).
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Identity (login/register, user accounts) ----
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // keep simple for a student project
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ---- App services (business logic lives here, not in controllers) ----
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddRazorPages(); // required for Identity's built-in Login/Register pages

// ---- MVC ----
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ---- Seed the database on startup (business types + starter products) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// ---- Middleware pipeline ----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // needed for Identity's built-in Login/Register pages

app.Run();

