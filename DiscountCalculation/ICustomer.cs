using System.Threading.Tasks;

namespace DiscountCalculation
{
    public interface ICustomer
    {
        Task<decimal> AddToWeekTotal(decimal orderTotal);
        Task<decimal> SubstractFromWeekTotal(decimal orderTotal);
        Task ResetWeekTotal();
    }
}
