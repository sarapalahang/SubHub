using SubHub.Domain.Entities;

namespace SubHub.Application.Interfaces;

public interface IPdfService
{
    byte[] GenerateInvoicePdf(Invoice invoice);
}
