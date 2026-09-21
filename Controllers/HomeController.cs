using Bizbox.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bizbox.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var businessTypes = await _db.BusinessTypes.ToListAsync();
        return View(businessTypes);
    }

    public IActionResult About() => View();

    public IActionResult Error() => View();
}
