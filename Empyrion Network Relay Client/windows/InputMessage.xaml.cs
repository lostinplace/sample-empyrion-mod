using System.Windows;
using System.Windows.Input;

namespace ENRC.windows
{
    /// <summary>
    /// Interaktionslogik für InputMessage.xaml
    /// </summary>
    public partial class InputMessage : Window
    {
        public InputMessage()
        {
            InitializeComponent();
            txtInput.Focus();
        }

        private void txtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Close();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
