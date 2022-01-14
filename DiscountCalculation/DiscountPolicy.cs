using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscountCalculation
{
    public static class DiscountPolicy
    {
        [FunctionName(nameof(OrderDiscountOrchestrator))]
        public static async Task OrderDiscountOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var submitOrder = context.GetInput<SubmitOrder>();

            var entityId = new EntityId(nameof(Customer), submitOrder.CustomerId.ToString());

            var customer = context.CreateEntityProxy<ICustomer>(entityId);

            var weekTotal = await customer.AddToWeekTotal(submitOrder.Total);

            var discount = weekTotal > 100 ? 10 : 0;

            await context.CallActivityAsync(nameof(ProcessOrderWithDiscount), new ProcessOrder(submitOrder.OrderId, discount));

            context.SignalEntity(entityId, context.CurrentUtcDateTime.AddSeconds(30), nameof(Customer.SubstractFromWeekTotal), submitOrder.Total);
        }

        [FunctionName(nameof(ProcessOrderWithDiscount))]
        public static Task ProcessOrderWithDiscount([ActivityTrigger] ProcessOrder processOrder,
            [ServiceBus("processorder", Connection = "sbcss", EntityType = EntityType.Queue)] IAsyncCollector<Message> processOrderMessages)
        {
            var processOrderMessage =
                new Message(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(processOrder)));

            return processOrderMessages.AddAsync(processOrderMessage);
        }

        [FunctionName(nameof(SubmitOrder))]
        public static async Task<IActionResult> SubmitOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SubmitOrder")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var submitOrder = request.Body.Deserialize<SubmitOrder>();

            var existingInstance = await starter.GetStatusAsync(submitOrder.OrderId.ToString());

            if (existingInstance is null)
            {

                // we're using orchestration so that we can 'call' entity and not signal (CQRS)
                // because we need to read the state immediately
                var instanceId = await starter.StartNewAsync(nameof(OrderDiscountOrchestrator), submitOrder.OrderId.ToString(), submitOrder);

                return starter.CreateCheckStatusResponse(request, instanceId);
            }
            else
            {
                return new AcceptedResult();
            }
        }
    }
}