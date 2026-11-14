namespace SubHub.Application.Interfaces;

public interface ISubscriptionExpiryService
{
    Task ProcessSubscriptionExpirationsAsync(CancellationToken cancellationToken = default);
}
