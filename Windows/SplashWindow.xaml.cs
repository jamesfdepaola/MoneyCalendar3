using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace MoneyCalendar.Windows
{
    public partial class SplashWindow : Window
    {
        public string AssemblyVersion { get => Assembly.GetExecutingAssembly().GetName().Version.ToString(); }

        public string FileVersion { get => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion; }

        public string ProductVersion { get => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion; }

        public SplashWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
