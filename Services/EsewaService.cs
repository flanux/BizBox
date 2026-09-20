using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bizbox.Models;
using Microsoft.Extensions.Configuration;

namespace Bizbox.Services;

// eSewa ePay v2 integration, wired to the public RC (sandbox) test environment.
// Sandbox test credentials (public, documented by eSewa for developers):
//   Merchant/product code: EPAYTEST
//   Secret key: 8gBm/:&EnhH.1/q
// These are already set as defaults in appsettings.json under PaymentGateways:eSewa.
public interface IEsewaService
{
    EsewaFormData BuildPaymentForm(Order order, string successUrl, string failureUrl);
    Task<bool> VerifyPaymentAsync(string base64EncodedResponse);
}

public class EsewaFormData
{
    public string FormActionUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Fields { get; set; } = new();
}

public class EsewaService : IEsewaService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public EsewaService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public EsewaFormData BuildPaymentForm(Order order, string successUrl, string failureUrl)
    {
        var merchantCode = _config["PaymentGateways:eSewa:MerchantId"] ?? "EPAYTEST";
        var secretKey = _config["PaymentGateways:eSewa:SecretKey"] ?? "8gBm/:&EnhH.1/q";
        var baseUrl = _config["PaymentGateways:eSewa:BaseUrl"] ?? "https://rc-epay.esewa.com.np";

        var transactionUuid = $"{order.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var totalAmount = order.TotalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

        var signedFieldNames = "total_amount,transaction_uuid,product_code";
        var message = $"total_amount={totalAmount},transaction_uuid={transactionUuid},product_code={merchantCode}";
        var signature = Sign(message, secretKey);

        var fields = new Dictionary<string, string>
        {
            ["amount"] = totalAmount,
            ["tax_amount"] = "0",
            ["total_amount"] = totalAmount,
            ["transaction_uuid"] = transactionUuid,
            ["product_code"] = merchantCode,
            ["product_service_charge"] = "0",
            ["product_delivery_charge"] = "0",
            ["success_url"] = successUrl,
            ["failure_url"] = failureUrl,
            ["signed_field_names"] = signedFieldNames,
            ["signature"] = signature
        };

        return new EsewaFormData
        {
            FormActionUrl = $"{baseUrl}/api/epay/main/v2/form",
            Fields = fields
        };
    }

    private static string Sign(string message, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    // eSewa redirects back with a base64-encoded JSON blob in the "data" query param.
    // We decode it, then independently re-check status against eSewa's status API
    // rather than trusting the redirect payload alone.
    public async Task<bool> VerifyPaymentAsync(string base64EncodedResponse)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedResponse));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetString();
            var transactionUuid = root.GetProperty("transaction_uuid").GetString();
            var totalAmount = root.GetProperty("total_amount").GetString();

            if (status != "COMPLETE" || transactionUuid == null || totalAmount == null)
                return false;

            var merchantCode = _config["PaymentGateways:eSewa:MerchantId"] ?? "EPAYTEST";
            var statusBaseUrl = (_config["PaymentGateways:eSewa:BaseUrl"] ?? "https://rc-epay.esewa.com.np")
                .Replace("rc-epay", "rc"); // status check lives on the rc. subdomain, not rc-epay.

            var client = _httpClientFactory.CreateClient();
            var url = $"{statusBaseUrl}/api/epay/transaction/status/?product_code={merchantCode}&total_amount={totalAmount}&transaction_uuid={transactionUuid}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;

            var body = await response.Content.ReadAsStringAsync();
            using var statusDoc = JsonDocument.Parse(body);
            var confirmedStatus = statusDoc.RootElement.GetProperty("status").GetString();

            return confirmedStatus == "COMPLETE";
        }
        catch
        {
            // Any parse/network failure = treat as unverified, never assume success.
            return false;
        }
    }
}
