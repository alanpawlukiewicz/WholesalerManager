using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.Helpers
{
    public static class ErrorMessagesHelper
    {
        public static List<string> GetErrorMessages(this ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage).ToList();
        }
    }
}
