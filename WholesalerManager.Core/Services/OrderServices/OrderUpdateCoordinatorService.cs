using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.OrderItemServices;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrderUpdateCoordinatorService : IOrderUpdateCoordinatorService
    {
        private readonly IOrdersUpdaterService _ordersUpdaterService;
        private readonly IOrderItemsUpdaterService _orderItemsUpdaterService;
        private readonly IUnitOfWork _unitOfWork;
        public OrderUpdateCoordinatorService(IOrdersUpdaterService ordersUpdaterService, IOrderItemsUpdaterService orderItemsUpdaterService, IUnitOfWork unitOfWork)
        {
            _ordersUpdaterService = ordersUpdaterService;
            _orderItemsUpdaterService = orderItemsUpdaterService;
            _unitOfWork = unitOfWork;
        }
        public async Task<OrderResponse> UpdateFullOrder(OrderUpdateRequest? orderRequest, List<OrderItemUpdateRequest>? items)
        {
            if (orderRequest is null || items is null)
            {
                throw new ArgumentNullException("Order request and items cannot be null.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _ordersUpdaterService.UpdateOrder(orderRequest);
                await _orderItemsUpdaterService.UpdateMultipleOrderItems(items!);

                await _unitOfWork.CommitTransactionAsync();

                return order;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
