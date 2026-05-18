using Microsoft.AspNetCore.Mvc;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.UI.ViewComponents
{
    public class TableHeaderViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string propertyName)
        {
            ViewBag.PropertyName = propertyName;
            //ViewBag.SortOrder = sortOrder;
            return View();
        }
    }
}
