using MoneyCalendar.Attributes;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MoneyCalendar
{
    public abstract class BindableBasePlus : BindableBase
    {
        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            //Check if this value needs to be reset first (originally for IsFocused properties)
            ResetOnToggleAttribute resetter = this.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(ResetOnToggleAttribute)).FirstOrDefault() as ResetOnToggleAttribute;
            if (resetter != null && resetter.NeedsValueReset(storage, value, out object resetvalue))
                base.SetProperty<T>(ref storage, (T)resetvalue, propertyName);

            if (base.SetProperty(ref storage, value, propertyName))
            {
                IEnumerable<PropertyInfo> observesproperties = this.GetType().GetProperties()
                    .Where(prop => prop.GetCustomAttributes(typeof(ObservesPropertyAttribute)).Any(attr => (attr as ObservesPropertyAttribute).PropertyName == propertyName));

                if (observesproperties.Any())
                {
                    foreach (PropertyInfo property in observesproperties)
                    {
                        this.RaisePropertyChanged(property.Name);
                    }
                }

                return true;
            }
            else
                return false;
        }
    }

    public static class Extensions
    {
        public static TTarget CopyProperties<TSource, TTarget>(this TSource source, bool includekeys = false) 
            where TSource : BindableBasePlus
            where TTarget : BindableBasePlus, new()
        {
            TTarget clone = new TTarget();

            foreach (PropertyInfo sourceproperty in source.GetType().GetProperties())
            {
                try
                {
                    if ((includekeys || !sourceproperty.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute)).Any())
                        && !sourceproperty.PropertyType.IsSubclassOf(typeof(BindableBase)))
                    {
                        PropertyInfo cloneproperty = clone.GetType().GetProperty(sourceproperty.Name);

                        if (cloneproperty != null && cloneproperty.SetMethod != null)
                            cloneproperty.SetValue(clone, source.GetType().GetProperty(sourceproperty.Name).GetValue(source));
                    }
                }
                catch (Exception ex)
                {
                    MoneyApplication.ErrorHandler(ex);
                }
            }

            return clone;
        }

        public static T Clone<T>(this T source, bool includekeys = false)
            where T : BindableBasePlus, new()
        {
            return source.CopyProperties<T, T>(includekeys);
        }
    }

    public class ViewModelBase : BindableBasePlus
    {
        #region Members
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Commands
        public DelegateCommand TesterCommand { get; protected set; } = new DelegateCommand(() => System.Windows.MessageBox.Show("test!"));
        #endregion

        #region AutoComplete
        protected void StartGetAutoCompleteListTask(Expression<Func<List<string>>> listpropertyexpression, Func<List<string>> getsuggestionsexpression) //string procedurename, string startwith)
        {
            if (this._cancellationTokenSource != null)
                this._cancellationTokenSource.Cancel();

            //Check for a TargetProperty to set directly
            MemberExpression memberexpression = (MemberExpression)listpropertyexpression.Body;
            PropertyInfo listpropertyinfo = (PropertyInfo)memberexpression.Member;

            this._cancellationTokenSource = new CancellationTokenSource();
            CancellationToken ct = this._cancellationTokenSource.Token;

            new TaskFactory().StartNew(() =>
            {
                IEnumerable<string> autocompletelist = null;

                try
                {
                    autocompletelist = getsuggestionsexpression.Invoke();

                    if (!ct.IsCancellationRequested)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                        {
                            listpropertyinfo.SetValue(this, autocompletelist);
                            this.RaisePropertyChanged(listpropertyinfo.Name);
                        });
                    }
                }
                catch (Exception ex)
                {
                    MoneyApplication.ErrorHandler(ex);
                }
            }, ct);
        }

        //public virtual void ClearAutoCompleteLists()
        //{
        //    foreach (PropertyInfo property in this.GetType().GetProperties().Where(prop => prop.GetCustomAttributes<AutoCompleteListAttribute>().Any()))
        //    {
        //        if (property.GetValue(this) is IEnumerable<string>)
        //        {
        //            property.SetValue(this, null);
        //            this.RaisePropertyChanged(property.Name);
        //        }
        //    }
        //}
        #endregion
    }
}
