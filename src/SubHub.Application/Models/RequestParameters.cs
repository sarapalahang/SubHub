using System.Text.Json.Serialization;

namespace SubHub.Application.Models;

public class RequestParameters
{
    public string merchant_id { get; set; }
    public long amount { get; set; }
    public string description { get; set; }
    public string callback_url { get; set; }

    // این خط کلید حل مشکل -۹ هست:
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? metadata { get; set; } = null; // پیش‌فرض نال باشه

    public RequestParameters(string merchantId, long amount, string description, string callbackUrl, string? userMobile = null, string? userEmail = null)
    {
        this.merchant_id = merchantId;
        this.amount = amount;
        this.description = description;
        this.callback_url = callbackUrl;

        // فقط اگر دیتا داریم، آرایه رو بساز
        if (!string.IsNullOrEmpty(userMobile) || !string.IsNullOrEmpty(userEmail))
        {
            this.metadata = new string[] { userMobile ?? "", userEmail ?? "" };
        }
    }
}
