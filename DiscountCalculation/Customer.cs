using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DiscountCalculation
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Customer : ICustomer
    {
        [JsonProperty("weektotal")]
        public decimal WeekTotal { get; set; }

        public Task<decimal> AddToWeekTotal(decimal orderTotal)
        {
            WeekTotal += orderTotal;

            return Task.FromResult(WeekTotal);
        }

        public Task<decimal> SubstractFromWeekTotal(decimal orderTotal)
        {
            if (WeekTotal == 0)
                return Task.FromResult(0m);

            WeekTotal -= orderTotal;

            return Task.FromResult(WeekTotal);
        }

        public Task ResetWeekTotal()
        {
            WeekTotal = 0;

            return Task.CompletedTask;
        }

        [FunctionName(nameof(Customer))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
            => context.DispatchAsync<Customer>();
    }
}
