using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace OodHelper
{
    class HMSValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult ret;
            Regex rxt = new Regex("^[0-9]{2}([: ][0-9]{2}){2}$");
            string input = (value ?? string.Empty).ToString();
            if (rxt.IsMatch(input))
                ret = new ValidationResult(true, null);
            else
                ret = new ValidationResult(false, "Not valid time format");
            return ValidationResult.ValidResult;
        }
    }
}
