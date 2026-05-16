using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.Helpers
{
    public static class ParseHelper
    {
        public static decimal ToDecimalSafe(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0m;

            var style = System.Globalization.NumberStyles.Any;
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            if (decimal.TryParse(input.Replace(",", "."), style, culture, out var result))
            {
                return Math.Round(result, 2, MidpointRounding.ToNegativeInfinity);
            }

            return 0m;
        }
    }
}
