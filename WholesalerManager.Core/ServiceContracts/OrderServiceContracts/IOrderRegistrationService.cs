using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    /// <summary>
    /// Represents a service contract for registering a full order, including the order details and its associated items. This service is responsible for handling the logic of creating a new order in the system, ensuring that all necessary information is provided and that the order is properly saved to the database. The method takes an OrderAddRequest object containing the order details and a list of OrderItemAddRequest objects representing the items included in the order. It returns an OrderResponse object containing the result of the registration process, such as success status and any relevant messages or data.
    /// </summary>
    public interface IOrderRegistrationService
    {
        /// <summary>
        /// Asynchronously registers a full order, including the order details and its associated items. This method takes an OrderAddRequest object containing the order details and a list of OrderItemAddRequest objects representing the items included in the order. It processes the registration logic, ensuring that all necessary information is provided and that the order is properly saved to the database. The method returns an OrderResponse object containing the result of the registration process, such as success status and any relevant messages or data.
        /// </summary>
        /// <param name="orderRequest">The request containing the order information to be registered.</param>
        /// <param name="items">The list of requests containing the order item information to be registered.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the registered order response.</returns>
        Task<OrderResponse> RegisterFullOrder(OrderAddRequest? orderRequest, List<OrderItemAddRequest>? items);
    }
}
