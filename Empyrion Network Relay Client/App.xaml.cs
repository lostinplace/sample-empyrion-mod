using System.Windows;
using System.Windows.Threading;

namespace ENRC
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
            void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            MessageBox.Show(e.Exception.Message);
            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}
