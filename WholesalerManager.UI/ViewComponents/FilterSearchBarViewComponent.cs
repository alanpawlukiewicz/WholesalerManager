using Microsoft.AspNetCore.Mvc;

namespace WholesalerManager.UI.ViewComponents
{
    public class FilterSearchBarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string controllerName, string actionName)
        {
            ViewBag.ControllerName = controllerName;
            ViewBag.ActionName = actionName;
            return View();
        }
    }
}
