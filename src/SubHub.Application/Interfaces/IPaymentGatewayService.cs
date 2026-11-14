namespace SubHub.Application.Interfaces
{
    public interface IPaymentGatewayService
    {
        string ProviderName { get; }

        Task<(bool IsSuccess, string? Authority, string? PaymentUrl, string? ErrorMessage)> RequestPaymentAsync(
            long amount, string description, string callbackUrl, string? userEmail = null, string? userMobile = null);

        Task<(bool IsSuccess, string? RefId, string? ErrorMessage)> VerifyPaymentAsync(
            string authority, long amount);

        Task<(bool IsPaid, string? RefId, string? ErrorMessage)> InquiryPaymentAsync(
            string authority);
    }
}
