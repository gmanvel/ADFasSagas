using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Shipping
{
    public static class ShippingPolicy
    {
        [FunctionName(nameof(OrderAcceptedHandler))]
        public static async Task OrderAcceptedHandler(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "OrderAccepted")] HttpRequest request,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            var entityId = new EntityId(nameof(OrderShipping), request.Query["orderId"].ToString());

            await entityClient.SignalEntityAsync<IShipOrder>(entityId, orderShipping => orderShipping.SetOrderAccepted());
        }

        [FunctionName(nameof(OrderBilledHandler))]
        public static async Task OrderBilledHandler(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "OrderBilled")] HttpRequest request,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            var entityId = new EntityId(nameof(OrderShipping), request.Query["orderId"].ToString());

            await entityClient.SignalEntityAsync<IShipOrder>(entityId, orderShipping => orderShipping.SetOrderBilled());
        }
    }
}