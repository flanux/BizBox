using Bizbox.Models;
using Bizbox.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bizbox.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IEsewaService _esewaService;
    private readonly IKhaltiService _khaltiService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentController(IOrderService orderService, IEsewaService esewaService,
        IKhaltiService khaltiService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _esewaService = esewaService;
        _khaltiService = khaltiService;
        _userManager = userManager;
    }

    // ---- eSewa ----
    public async Task<IActionResult> EsewaSuccess(int orderId, string data)
    {
        var verified = await _esewaService.VerifyPaymentAsync(data);
        if (verified)
        {
            await _orderService.MarkOrderPaidAsync(orderId);
            return RedirectToAction("Success", new { orderId });
        }
        return RedirectToAction("Failure", new { orderId });
    }

    public IActionResult EsewaFailure(int orderId) => RedirectToAction("Failure", new { orderId });

    // ---- Khalti ----
    public async Task<IActionResult> KhaltiCallback(int orderId, string pidx, string status)
    {
        if (status == "Completed" && await _khaltiService.VerifyPaymentAsync(pidx))
        {
            await _orderService.MarkOrderPaidAsync(orderId);
            return RedirectToAction("Success", new { orderId });
        }
        return RedirectToAction("Failure", new { orderId });
    }

    // ---- Simulated international payment ----
    // See project context doc, Section 5: intentionally not a real gateway integration.
    public IActionResult InternationalSimulated(int orderId)
    {
        ViewBag.OrderId = orderId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmInternationalSimulated(int orderId, string cardNumber, string expiry, string cvv)
    {
        var digitsOnly = new string((cardNumber ?? "").Where(char.IsDigit).ToArray());
        var validExpiry = System.Text.RegularExpressions.Regex.IsMatch(expiry ?? "", @"^(0[1-9]|1[0-2])/\d{2}$");
        var validCvv = System.Text.RegularExpressions.Regex.IsMatch(cvv ?? "", @"^\d{3}$");

        if (digitsOnly.Length < 12 || digitsOnly.Length > 19 || !validExpiry || !validCvv)
        {
            TempData["Message"] = "Invalid card details.";
            return RedirectToAction("InternationalSimulated", new { orderId });
        }

        // No real card processing happens here — this exists purely to demonstrate
        // the UX flow for a payment path that isn't practically available to a
        // student project. See README / project context doc.
        await _orderService.MarkOrderPaidAsync(orderId);
        return RedirectToAction("Success", new { orderId });
    }

    // ---- Shared result pages ----
    public async Task<IActionResult> Success(int orderId)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderAsync(orderId, userId);
        return View(order);
    }

    public async Task<IActionResult> Failure(int orderId)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderAsync(orderId, userId);
        return View(order);
    }
}
