using System.Threading.Tasks;

namespace Shipping
{
    public interface IShipOrder
    {
        Task SetOrderBilled();

        Task SetOrderAccepted();
    }
}
