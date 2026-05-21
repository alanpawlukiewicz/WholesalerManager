using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WholesalerManager.UI.Helpers
{
    public static class ErrorMessagesHelper
    {
        public static List<string> GetErrorMessages(this ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
        }
    }
}
