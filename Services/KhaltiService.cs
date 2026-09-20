using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Bizbox.Models;
using Microsoft.Extensions.Configuration;

namespace Bizbox.Services;

// Khalti ePayment (KPG-2) integration against Khalti's public test environment.
// Test secret key (documented publicly by Khalti for sandbox testing):
//   test_secret_key_68791341cd1a42b78f594a8f1b7ff3f5  (their published test key)
// Already defaulted in appsettings.json under PaymentGateways:Khalti.
public interface IKhaltiService
{
    Task<(bool success, string? paymentUrl, string? pidx)> InitiatePaymentAsync(Order order, string returnUrl, string websiteUrl);
    Task<bool> VerifyPaymentAsync(string pidx);
}

public class KhaltiService : IKhaltiService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public KhaltiService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(bool success, string? paymentUrl, string? pidx)> InitiatePaymentAsync(Order order, string returnUrl, string websiteUrl)
    {
        var secretKey = _config["PaymentGateways:Khalti:SecretKey"] ?? "";
        var baseUrl = _config["PaymentGateways:Khalti:BaseUrl"] ?? "https://dev.khalti.com/api/v2";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", secretKey);

        var amountPaisa = (int)(order.TotalAmount * 100); // Khalti expects amount in paisa

        var payload = new
        {
            return_url = returnUrl,
            website_url = websiteUrl,
            amount = amountPaisa,
            purchase_order_id = order.Id.ToString(),
            purchase_order_name = $"Bizbox Order #{order.Id}"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{baseUrl}/epayment/initiate/", content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return (false, null, null);

            using var doc = JsonDocument.Parse(body);
            var paymentUrl = doc.RootElement.GetProperty("payment_url").GetString();
            var pidx = doc.RootElement.GetProperty("pidx").GetString();
            return (true, paymentUrl, pidx);
        }
        catch
        {
            return (false, null, null);
        }
    }

    public async Task<bool> VerifyPaymentAsync(string pidx)
    {
        var secretKey = _config["PaymentGateways:Khalti:SecretKey"] ?? "";
        var baseUrl = _config["PaymentGateways:Khalti:BaseUrl"] ?? "https://dev.khalti.com/api/v2";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", secretKey);

        var content = new StringContent(JsonSerializer.Serialize(new { pidx }), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{baseUrl}/epayment/lookup/", content);
            if (!response.IsSuccessStatusCode) return false;
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var status = doc.RootElement.GetProperty("status").GetString();
            return status == "Completed";
        }
        catch
        {
            return false;
        }
    }
}
