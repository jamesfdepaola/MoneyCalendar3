using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections;
using System.Windows.Media;

namespace MoneyCalendar.ValueConverters
{
    #region Numeric
    public class MultiplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal? result = 1;
            try
            {
                if (value != null && value != DependencyProperty.UnsetValue)
                {
                    result = System.Convert.ToDecimal(value);

                    if (parameter != null && parameter != DependencyProperty.UnsetValue)
                        result *= decimal.Parse(parameter?.ToString());
                }
            }
            catch (Exception ex)
            {
                result = null;
                MoneyApplication.ErrorHandler(ex);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiMultiplicationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal? result = 1;
            try
            {
                foreach (object value in values)
                {
                    if (value == DependencyProperty.UnsetValue)
                        return null;

                    result *= decimal.Parse(value?.ToString());
                }

            }
            catch (Exception ex)
            {
                result = null;
                MoneyApplication.ErrorHandler(ex);
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AdditionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal? result = 0;

            try
            {
                foreach (object value in values)
                {
                    if (value is GridLength gridlength)
                        result += decimal.Parse(gridlength.Value.ToString());
                    else if (value != null && value != DependencyProperty.UnsetValue)
                        result += decimal.Parse(value?.ToString());
                }
            }
            catch (Exception ex)
            {
                result = null;
                MoneyApplication.ErrorHandler(ex);
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridLengthAdditionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 0;

            try
            {
                foreach (object value in values)
                {
                    if (value is GridLength gridlength)
                        result += double.Parse(gridlength.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return new GridLength(result);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntegerToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out int result))
                return result;
            else
                return null;
        }
    }

    public class NumberToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue) value = 0;
            return new GridLength((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridLengthToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLengthConverter converter = new GridLengthConverter();
            double length = 0;

            try
            {
                //we have to convert to a string first, cause THE GRIDLENGTH CONVERTER CLASS doesn't go straight to double
                string lengthstring = converter.ConvertToString(value);
                double.TryParse(lengthstring, out length);
            }
            catch
            {
                length = 0;
            }
            return length;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiBindingToMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double[] margins = new double[4] { 0, 0, 0, 0 };

            for (int i = 0; i < values.Length; i++)
            {
                try
                {
                    if (values[i] is GridLength gridlength)
                        margins[i] = gridlength.Value;
                    else
                        margins[i] = System.Convert.ToDouble(values[i]);
                }
                catch { }
            }

            return new Thickness(margins[0], margins[1], margins[2], margins[3]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NumberIsEvenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (int)value % 2 == 0;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NumberIsNegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (decimal)(value ?? 0M) < 0;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PercentToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = value.ToString();

            if (value is decimal percentage)
            {
                try
                {
                    string format = parameter?.ToString() ?? "0.00%";

                    result = percentage.ToString(format);
                }
                catch { }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (decimal.TryParse(value.ToString().Replace("%", ""), out decimal result))
                    return result / 100M;
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
    #endregion

    #region String
    public class EmptyStringToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString().Trim() == "")
                value = null;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString() == "")
                value = null;

            return value;
        }
    }

    public partial class FullNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue) values[0] = "";
            if (values[1] == DependencyProperty.UnsetValue) values[1] = "";

            return values[0]?.ToString() + " " + values[1]?.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueIsEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null || value.ToString() == "")
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueIsNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)(new ValueIsEmptyConverter().Convert(value, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Visibility
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (parameter != null && parameter.ToString() == "wtf")
                    System.Diagnostics.Debugger.Break();

                if (value != null && (bool)value)
                    return Visibility.Visible;
                else
                {
                    //Check for a paramter to make object Hidden instead of Collapsed
                    if (parameter == null)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Hidden;
                }
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public Visibility DoConvert(bool value)
        {
            return (Visibility)this.Convert(value, typeof(Visibility), null, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NegateBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanNorToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            try
            {
                result = (bool)new BooleanNorConverter().Convert(values, null, null, null);

                return new BooleanToVisibilityConverter().Convert(result, null, null, null);
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanOrToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            try
            {
                result = (bool)new BooleanOrConverter().Convert(values, null, null, null);

                return new BooleanToVisibilityConverter().Convert(result, null, null, null);
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanAndToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            try
            {
                result = (bool)new BooleanAndConverter().Convert(values, null, null, null);

                return new BooleanToVisibilityConverter().Convert(result, null, null, null);
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemCountVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value > 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class NegateVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
                value = Visibility.Collapsed;
            else
                value = Visibility.Visible;

            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HideEmptyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null || value.ToString() == "")
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Boolean
    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = true;

            try
            {
                foreach (bool value in values)
                    result &= value;

                return result;
            }
            catch
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] result = { false };

            result = new object[targetTypes.Length];
            for (int i = 0; i < targetTypes.Length; i++)
            {
                result[i] = value;
            }

            return result;
        }
    }

    public class BooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            try
            {
                foreach (bool value in values)
                    result |= value;

                return result;
            }
            catch
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanNorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            try
            {
                foreach (bool value in values)
                    result |= value;

                return !result;
            }
            catch
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class AutoCompletePopupIsOpenConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        //Parameters:
    //        // bool TextBox.IsFocused, bool showlist

    //        try
    //        {
    //            return (bool)values[0] && (int)values[1] > 0;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class RecordIDToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if (value != null && (int)value > 0)
                result = true;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToGoToOrSelectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value == true)
            {
                return "Go To...";
            }
            else
                return "Select...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToGoToOrAddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "";

            if (value != null && (bool)value == true)
            {
                result = "Go To";
            }
            else
                result = "Add";

            if (parameter != null && parameter is string)
                result += $" {parameter.ToString()}";

            result += "...";
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return "Yes";
            else
                return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString().ToUpper() == "YES")
                return true;
            else
                return false;
        }
    }
    #endregion

    #region Other
    public class TesterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debugger.Break();

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedRecordToByIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? orderid = null;

            try
            {
                if (value != CollectionView.NewItemPlaceholder && value != null && parameter != null)
                    orderid = (int)value.GetType().GetProperty(parameter.ToString()).GetValue(value);
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return orderid;
        }
    }

    //public class AutoCompleteSelectionConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return new AutoCompleteSelectionArgs() { FieldName = parameter?.ToString(), Value = value?.ToString() };
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class ObjectSelector : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //values (in PAIRS)
            //object [the object to select], bool [object shoudl be selected]

            object result = null;
            object objectvalue = null;

            try
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (i % 2 == 0)   //first in pair
                        objectvalue = values[i];
                    else if (values[i] is bool boolvalue && boolvalue)
                    {
                        result = objectvalue;
                        break;
                    }
                }
            }
            catch { }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrentDateBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime datetime && datetime == DateTime.Today)
                return Brushes.Cyan;
            else
                return Brushes.Beige;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region List Sorting
    public class ListSortingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList collection = value as IList;
            ListCollectionView view = new ListCollectionView(collection);
            SortDescription sort = new SortDescription(parameter.ToString(), ListSortDirection.Ascending);
            view.SortDescriptions.Add(sort);

            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion
}
