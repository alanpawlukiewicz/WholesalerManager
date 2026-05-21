using Microsoft.AspNetCore.Mvc;

namespace WholesalerManager.UI.ViewComponents
{
    public class TableHeaderViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string propertyName, string? displayName, string? actionName = null)
        {
            ViewBag.PropertyName = propertyName;
            ViewBag.DisplayName = displayName ?? propertyName;
            ViewBag.ActionName = actionName ?? "Index";

            return View();
        }
    }
}
