using Bizbox.Models;
using Bizbox.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bizbox.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IEsewaService _esewaService;
    private readonly IKhaltiService _khaltiService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(ICartService cartService, IOrderService orderService,
        IEsewaService esewaService, IKhaltiService khaltiService, UserManager<ApplicationUser> userManager)
    {
        _cartService = cartService;
        _orderService = orderService;
        _esewaService = esewaService;
        _khaltiService = khaltiService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var items = await _cartService.GetCartAsync(userId);
        if (!items.Any())
        {
            TempData["Message"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }
        ViewBag.Total = await _cartService.GetCartTotalAsync(userId);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PaymentMethod paymentMethod)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.CreateOrderFromCartAsync(userId, paymentMethod);

        switch (paymentMethod)
        {
            case PaymentMethod.Esewa:
                {
                    var successUrl = Url.Action("EsewaSuccess", "Payment", new { orderId = order.Id }, Request.Scheme)!;
                    var failureUrl = Url.Action("EsewaFailure", "Payment", new { orderId = order.Id }, Request.Scheme)!;
                    var form = _esewaService.BuildPaymentForm(order, successUrl, failureUrl);
                    return View("RedirectToEsewa", form);
                }

            case PaymentMethod.Khalti:
                {
                    var returnUrl = Url.Action("KhaltiCallback", "Payment", new { orderId = order.Id }, Request.Scheme)!;
                    var websiteUrl = $"{Request.Scheme}://{Request.Host}";
                    var (success, paymentUrl, pidx) = await _khaltiService.InitiatePaymentAsync(order, returnUrl, websiteUrl);
                    if (!success || paymentUrl == null)
                    {
                        TempData["Message"] = "Could not start Khalti payment. Please try again.";
                        return RedirectToAction("Index", "Cart");
                    }
                    return Redirect(paymentUrl);
                }

            case PaymentMethod.InternationalCard_Simulated:
            default:
                return RedirectToAction("InternationalSimulated", "Payment", new { orderId = order.Id });
        }
    }
}
