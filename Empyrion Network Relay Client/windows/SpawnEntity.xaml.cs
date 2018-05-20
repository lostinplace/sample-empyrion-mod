using System.Windows;

namespace ENRC.windows
{
    /// <summary>
    /// Interaktionslogik für InputBox.xaml
    /// </summary>
    public partial class SpawnEntity : Window
    {
        public MainWindow mainWd { get; set; }

        public SpawnEntity()
        {
            InitializeComponent();
            txtID.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (txtExportFile.Text != "" && System.IO.File.Exists(txtExportFile.Text) == false)
                {
                    mainWd.mainWindowDataContext.output.Add("Export file not found");
                    return;
                }

                mainWd.EntitySpawn(txtID.Text, txtPrefabName.Text, txtExportFile.Text, txtPlayfield.Text, txtEntityTypeName.Text, txtName.Text, txtFactionGroup.Text, txtFactionID.Text, txtType.Text, txtNS.Text, txtHeight.Text, txtEW.Text, txtX.Text, txtY.Text, txtZ.Text, txtPrefabDir.Text);
            }
            catch
            {
            }
        }
    }
}
