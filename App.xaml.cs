using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using MoneyCalendar.DataModels;
using MoneyCalendar.Properties;
using MoneyCalendar.ViewModels;
using MoneyCalendar.Windows;

namespace MoneyCalendar
{
    public partial class MoneyApplication : Application
    {        
        public static MoneyApplication MoneyApp { get => Application.Current as MoneyApplication; }
        public static DbSettings DbSettings { get; private set; }

        public const string ConnectionString= "data source=;integrated security=True;connect timeout=30;MultipleActiveResultSets=True;App=EntityFramework;initial catalog=MoneyCalendar";

        protected override void OnStartup(StartupEventArgs e)
        {
            SplashWindow splashwindow = new SplashWindow();
            splashwindow.Show();

            base.OnStartup(e);

            try
            {
                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    MoneyApplication.DbSettings = context.Database.SqlQuery<DataModels.DbSettings>("SELECT * FROM DbSettings").FirstOrDefault() ?? new DbSettings();
                }

                CalendarViewModel viewodel = new CalendarViewModel();// moneycontext);

                CalendarWindow CalendarWindow = new CalendarWindow();
                CalendarWindow.DataContext = viewodel;
                CalendarWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error");
                MoneyApplication.ErrorHandler(ex);
            }
            finally
            {
                splashwindow.Close();
            }
        }

        public static MoneyCalendarEntities CreateConext()
        {
            MoneyCalendarEntities moneycontext;

            moneycontext = new MoneyCalendarEntities();
            moneycontext.Configuration.ProxyCreationEnabled = false;
            return moneycontext;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        public static void SaveDbSetting(string fieldname, object value)
        {
            SqlParameter fieldparameter = new SqlParameter("@Value", value);

            using (MoneyCalendarEntities context = new MoneyCalendarEntities())
            {
                context.Database.ExecuteSqlCommand($"UPDATE DbSettings SET {fieldname} = @Value", fieldparameter);
            }
        }

        #region Error Handling
        internal static void ErrorHandler(Exception ex, string message = "")
        {
#if DEBUG
            System.Diagnostics.Debugger.Break();
#else
            MessageBox.Show(message + "\r\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
        }
        #endregion
    }
}
