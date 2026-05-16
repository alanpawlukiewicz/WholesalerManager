using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.DTO
{
    public class EditUnitPriceDTO
    {
        public Guid ProductID { get; set; }
        public string? NewUnitPrice { get; set; }
    }
}
