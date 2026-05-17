using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrderRegistrationService : IOrderRegistrationService
    {
        private readonly IOrdersAdderService _ordersAdderService;
        private readonly IOrderItemsAdderService _orderItemsAdderService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderRegistrationService(IOrdersAdderService ordersAdderService, IOrderItemsAdderService orderItemsAdderService, IUnitOfWork unitOfWork)
        {
            _ordersAdderService = ordersAdderService;
            _orderItemsAdderService = orderItemsAdderService;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResponse> RegisterFullOrder(OrderAddRequest? orderRequest, List<OrderItemAddRequest>? items)
        {
            if (orderRequest is null || items is null)
            {
                throw new ArgumentNullException("Order request and items cannot be null.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _ordersAdderService.AddOrder(orderRequest);

                items.ForEach(i => i.OrderID = order.OrderID);

                await _orderItemsAdderService.AddMultipleOrderItems(items);

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
