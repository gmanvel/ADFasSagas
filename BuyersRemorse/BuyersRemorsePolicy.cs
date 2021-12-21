using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BuyersRemorse
{
    public static class BuyersRemorsePolicy
    {
        [FunctionName(nameof(BuyersRemorseOrchestrator))]
        public static async Task BuyersRemorseOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var logger = context.CreateReplaySafeLogger(log);

            var orderId = context.GetInput<Guid>();

            try
            {
                logger.LogInformation("Entering wait period for CancelOrder event");

                await context.WaitForExternalEvent("CancelOrder", TimeSpan.FromSeconds(30));

                logger.LogInformation("Order was cancelled");
            }
            catch (TimeoutException)
            {
                // order wasn't cancelled, proceed to proces the order
                await context.CallActivityAsync(nameof(ProcessOrder), orderId);
            }
        }

        [FunctionName(nameof(ProcessOrder))]
        public static async Task ProcessOrder([ActivityTrigger] Guid orderId, ILogger log)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            log.LogInformation("Processing the order");
        }

        [FunctionName(nameof(SubmitOrder))]
        public static async Task<IActionResult> SubmitOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            var orderId = request.Query["orderId"].ToString();

            // https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-singletons?tabs=csharp
            // https://github.com/Azure/durabletask/pull/528   
            var existingInstance = await starter.GetStatusAsync(orderId);

            if (existingInstance is null)
                await starter.StartNewAsync(nameof(BuyersRemorseOrchestrator), orderId, orderId);

            return new AcceptedResult();
        }

        [FunctionName(nameof(CancelOrder))]
        public static async Task<IActionResult> CancelOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            var orderId = request.Query["orderId"].ToString();

            await starter.RaiseEventAsync(orderId, "CancelOrder");

            return new OkObjectResult("Order has been cancelled");
        }
    }
}