using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.UI.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Administrator")]
    [Route("[area]/[controller]/[action]")]
    public class LoggingController : Controller
    {
        private readonly IAuditLoggerService _auditLoggerService;
        private readonly IInventoryLoggerService _inventoryLoggerService;

        public LoggingController(IAuditLoggerService auditLoggerService, IInventoryLoggerService inventoryLoggerService)
        {
            _auditLoggerService = auditLoggerService;
            _inventoryLoggerService = inventoryLoggerService;
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogs([FromQuery] string? propertyName, [FromQuery] string? filter, [FromQuery] bool? ignoreCase, [FromQuery] SortOrderOptions? sortOrder)
        {
            List<AuditLog> logs;
            if (filter is not null && propertyName is not null)
            {
                logs = await _auditLoggerService.GetFilteredAuditLogs(propertyName, filter, ignoreCase ?? false);
            }

            else if (sortOrder is not null && propertyName is not null)
            {
                logs = await _auditLoggerService.GetSortedAuditLogs(propertyName, sortOrder ?? SortOrderOptions.ASC);
            }
            else
            {
                logs = await _auditLoggerService.GetAuditLogsAsync();
            }

            ViewBag.FieldNames = new List<string>{ nameof(AuditLog.Timestamp), nameof(AuditLog.User.Email), nameof(AuditLog.AttemptedUsername), nameof(AuditLog.IpAddress)};

            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> InventoryLogs([FromQuery] string? propertyName, [FromQuery] string? filter, [FromQuery] bool? ignoreCase, [FromQuery] SortOrderOptions? sortOrder)
        {
            List<InventoryLog> logs;
            if (filter is not null && propertyName is not null)
            {
                logs = await _inventoryLoggerService.GetFilteredInventoryLogs(propertyName, filter, ignoreCase ?? false);
            }

            else if (sortOrder is not null && propertyName is not null)
            {
                logs = await _inventoryLoggerService.GetSortedInventoryLogs(propertyName, sortOrder ?? SortOrderOptions.ASC);
            }
            else
            {
                logs = await _inventoryLoggerService.GetInventoryLogsAsync();
            }

            ViewBag.FieldNames = new List<string>{ nameof(InventoryLog.CreatedAt), nameof(InventoryLog.OperationType), nameof(InventoryLog.Product.ProductName), nameof(InventoryLog.PreviousStock), nameof(InventoryLog.NewStock), };
            return View(logs);
        }
    }
}
