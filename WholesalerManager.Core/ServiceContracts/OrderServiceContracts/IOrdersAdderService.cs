using WholesalerManager.Core.DTO.OrderDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    /// <summary>
    /// Represents buisness logic for registering new deliveries to database.
    /// </summary>
    public interface IOrdersAdderService
    {
        /// <summary>
        /// Asynchronously adds new order to database
        /// </summary>
        /// <param name="orderAddRequest"></param>
        /// <returns></returns>
        Task<OrderResponse> AddOrder(OrderAddRequest? orderAddRequest);
    }
}
