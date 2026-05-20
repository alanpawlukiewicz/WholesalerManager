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

        public LoggingController(IAuditLoggerService auditLoggerService)
        {
            _auditLoggerService = auditLoggerService;
        }

        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _auditLoggerService.GetAuditLogsAsync();
            return View(logs);
        }

        public async Task<IActionResult> InventoryLogs()
        {
            return View();
        }
    }
}
