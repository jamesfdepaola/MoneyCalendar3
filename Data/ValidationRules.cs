using System;
using System.Windows.Controls;
using System.Windows.Data;
using MoneyCalendar.DataModels;

namespace MoneyCalendar.Data
{
    public class AccountValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = ValidationResult.ValidResult;

            try
            {
                Account account = (value as BindingGroup).Items[0] as Account;

                if (string.IsNullOrEmpty(account.Name))
                {
                    result= new ValidationResult(false, "Enter a name.");
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
                result = new ValidationResult(false, "Error during validation.");
            }

            return result;
        }
    }
}
