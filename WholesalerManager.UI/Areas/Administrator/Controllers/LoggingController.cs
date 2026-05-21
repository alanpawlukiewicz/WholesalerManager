using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _auditLoggerService.GetAuditLogsAsync();
            return View(logs);
        }

        public async Task<IActionResult> InventoryLogs()
        {
            var logs = await _inventoryLoggerService.GetInventoryLogsAsync();
            return View(logs);
        }
    }
}
