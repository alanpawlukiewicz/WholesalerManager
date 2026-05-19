using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;

namespace WholesalerManager.UI.Filters.ActionFilters
{
    public class ForcePasswordChangeFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>();

                var user = await userManager.GetUserAsync(context.HttpContext.User);

                if (user?.MustChangePassword == true)
                {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var tempDataFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();

                    ITempDataDictionary tempData = tempDataFactory.GetTempData(context.HttpContext);

                    tempData["InfoMessage"] = "You must change your password before accessing your account.";
                    context.Result = new RedirectToActionResult(
                        "SetPassword",
                        "Account",
                        new { email = user.Email, token = encodedToken }
                    );
                    return;
                }
            }

            await next();
        }
    }
}
