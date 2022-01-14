using System;

namespace DiscountCalculation
{
    public record SubmitOrder(Guid CustomerId, Guid OrderId, decimal Total);
}
