using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.Enums
{
    public enum OrderStatus
    {
        PENDING,
        PAID,
        PROCESSING,
        SHIPPED,
        DELIVERED,
        CANCELLED,
        RETURNED
    }
}
