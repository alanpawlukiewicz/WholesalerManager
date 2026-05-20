using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.UI.ViewComponents
{
    public class TableHeaderViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string propertyName, string? displayName)
        {
            ViewBag.PropertyName = propertyName;
            ViewBag.DisplayName = displayName ?? propertyName;
            return View();
        }
    }
}
