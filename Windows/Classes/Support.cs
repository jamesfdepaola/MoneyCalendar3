using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoneyCalendar.Windows
{
    public class AttachedProperties
    {
        public static DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsFocusedProperty, value);
        }

        public static void OnIsFocusedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is Control control)
            {
                bool newValue = (bool)dependencyPropertyChangedEventArgs.NewValue;
                bool oldValue = (bool)dependencyPropertyChangedEventArgs.OldValue;

                if (newValue && !oldValue && !control.IsFocused)
                {
                    control.Focus();
                    Keyboard.Focus(control);
                }
            }
        }
    }
}
