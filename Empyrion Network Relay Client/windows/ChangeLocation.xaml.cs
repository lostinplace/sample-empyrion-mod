using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ENRC.windows
{
    /// <summary>
    /// Interaktionslogik für ChangeLocation.xaml
    /// </summary>
    public partial class ChangeLocation : Window
    {
        public ChangeLocation()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Txt_SelectAll(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender != null)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        private void Txt_SelectAllMouseDClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        private void Txt_SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                if (!((TextBox)sender).IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    ((TextBox)sender).Focus();
                }
            }
        }
    }
}
