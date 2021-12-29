using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Shipping
{
    // https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-entities?tabs=csharp#general-concepts
    // To prevent conflicts, all operations on a single entity are guaranteed to execute serially, that is, one after another.
    [JsonObject(MemberSerialization.OptIn)]
    public class OrderShipping : IShipOrder
    {
        private readonly IQueueClient _queueClient;

        private readonly string _orderId;

        [JsonProperty("orderAccepted")]
        public bool OrderAccepted { get; set; }

        [JsonProperty("orderBilled")]
        public bool OrderBilled { get; set; }

        public OrderShipping(string orderId)
        {
            _orderId = orderId;
            // https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-dotnet-entities#entity-construction
            _queueClient = new QueueClient(connectionString: "", entityPath: "shipOrder");
        }

        public async Task SetOrderAccepted()
        {
            OrderAccepted = true;

            if (OrderBilled)
                await _queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(_orderId)));
        }

        public async Task SetOrderBilled()
        {
            OrderBilled = true;

            if (OrderAccepted)
                await _queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(_orderId)));
        }

        [FunctionName(nameof(OrderShipping))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
            => context.DispatchAsync<OrderShipping>(context.EntityKey);
    }
}
