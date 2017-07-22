using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace ENRC
{
    public partial class MainWindow : Window
    {
        private Timer aTimer;
        private int counter;
        private client.Client client;

        public MainWindow()
        {
            InitializeComponent();
            client = new client.Client();
            client.GameEventReceived += onGameEvent;
            client.ClientMessages += output;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //Reload all Online Player and Playfields
            Get_PlayfieldList();
            Get_PlayerList();

            //Timer to refresh all data
            counter = 5;
            aTimer = new Timer(5000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Disconnect();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            GetDediStats();
            GetAllPlayfieldStats();

            if (counter >= 5)
            {
                // Refresh Data
                GetPlayerInfo();
                GetAllStructureUpdates();
                Get_Strucutre_List();

                RefreshInventory();

                counter = 0;
            }
            counter += 1;
        }

        private void dgPlayer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshInventory();
        }

        private void RefreshInventory()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)(() =>
            {
                if (dgPlayer.SelectedItem != null)
                {
                    dgToolbar.ItemsSource = ((data.PlayerInfo)dgPlayer.SelectedItem).toolbar;
                    dgBackback.ItemsSource = ((data.PlayerInfo)dgPlayer.SelectedItem).bag;
                }
                else
                {
                    dgToolbar.ItemsSource = null;
                    dgBackback.ItemsSource = null;
                }
            }));
        }

        private void btnGetPlayfields_Click(object sender, RoutedEventArgs e)
        {
            Get_PlayfieldList();
        }

        private void btnGetPlayerList_Click(object sender, RoutedEventArgs e)
        {
            Get_PlayerList();
        }

        private void btnGetDediStats_Click(object sender, RoutedEventArgs e)
        {
            GetDediStats();
        }

        private void btnGetPlayfieldStats_Click(object sender, RoutedEventArgs e)
        {
            GetAllPlayfieldStats();
        }

        private void btnGetStructureList_Click(object sender, RoutedEventArgs e)
        {
            Get_Strucutre_List();
        }

        private void btnGetStructureUpdate_Click(object sender, RoutedEventArgs e)
        {
            GetAllStructureUpdates();
        }

        private void btnGetPlayerInfo_Click(object sender, RoutedEventArgs e)
        {
            GetPlayerInfo();
        }

        private void btnSentMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                windows.InputMessage wdInput = new windows.InputMessage();
                wdInput.ShowDialog();
                SendMessage_All(wdInput.txtInput.Text, System.Convert.ToByte(wdInput.txtPrio.Text), System.Convert.ToSingle(wdInput.txtTime.Text));
                wdInput = null;
            }
            catch
            {
                mainWindowDataContext.output.Add("Cant convert string");
            }
        }

        private void btnGetEntities_Click(object sender, RoutedEventArgs e)
        {
            GetAllEntities();
        }

        private void mnuAddCredits_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlayer.SelectedItem != null)
            {
                windows.InputBox wdInput = new windows.InputBox();
                wdInput.ShowDialog();

                try
                {
                    Player_AddCredits(((data.PlayerInfo)dgPlayer.SelectedItem).entityId, System.Convert.ToDouble(wdInput.txtInput.Text));
                }
                catch
                {
                    mainWindowDataContext.output.Add("Cant convert string to double");
                }
                wdInput = null;
            }
        }

        private void mnuChangeLocationPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlayer.SelectedItem != null)
            {
                var player = ((data.PlayerInfo)dgPlayer.SelectedItem);
                var oldPlayfield = player.playfield;

                windows.ChangeLocation wdChangeLocation = new windows.ChangeLocation { DataContext = player };

                if (wdChangeLocation.ShowDialog() == true)
                {
                    if (player.playfield != oldPlayfield)
                    {
                        Player_ChangePlayerfield(player.entityId, player.playfield, player.pos.ToPVector3(), player.rot.ToPVector3());
                    }
                    else
                    {
                        Entity_SetPosition(player.entityId, player.pos.ToPVector3(), player.rot.ToPVector3());
                    }
                }
            }
        }

        private void mnuChangeLocationStructure_Click(object sender, RoutedEventArgs e)
        {
            if (dgStructures.SelectedItem != null)
            {
                var structure = ((data.StructureInfo)dgStructures.SelectedItem);
                var oldPlayfield = structure.playfield;

                windows.ChangeLocation wdChangeLocation = new windows.ChangeLocation();
                wdChangeLocation.DataContext = structure;
                if (wdChangeLocation.ShowDialog() == true)
                {
                    if (structure.playfield != oldPlayfield)
                    {
                        Entity_ChangePlayfield(structure.id, structure.playfield, structure.pos.ToPVector3(), structure.rot.ToPVector3());
                    }
                    else
                    {
                        Entity_SetPosition(structure.id, structure.pos.ToPVector3(), structure.rot.ToPVector3());
                    }
                }
            }
        }

        private void mnuDestroyStructure_Click(object sender, RoutedEventArgs e)
        {
            if (dgStructures.SelectedItem != null)
            {
                Entity_Destroy(((data.StructureInfo)dgStructures.SelectedItem).id);
            }
        }

        private void mnuPlayerSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlayer.SelectedItem != null)
            {
                try
                {
                    windows.InputMessage wdInput = new windows.InputMessage();
                    wdInput.ShowDialog();
                    Player_SendMessage(((data.PlayerInfo)dgPlayer.SelectedItem).entityId, wdInput.txtInput.Text, System.Convert.ToByte(wdInput.txtPrio.Text), System.Convert.ToSingle(wdInput.txtTime.Text));
                    wdInput = null;
                }
                catch
                {
                    mainWindowDataContext.output.Add("Cant convert string");
                }
            }
        }

        private void mnuPlayerShowDialog_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlayer.SelectedItem != null)
            {
                try
                {
                    windows.InputMessage wdInput = new windows.InputMessage();
                    wdInput.ShowDialog();
                    Player_ShowDialog_SinglePlayer(((data.PlayerInfo)dgPlayer.SelectedItem).entityId, wdInput.txtInput.Text, System.Convert.ToByte(wdInput.txtPrio.Text), System.Convert.ToSingle(wdInput.txtTime.Text));
                    wdInput = null;
                }
                catch
                {
                    mainWindowDataContext.output.Add("Cant convert string");
                }
            }
        }

        private void mnuPlayerFinishBP_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlayer.SelectedItem != null)
            {
                Blueprint_Finish(((data.PlayerInfo)dgPlayer.SelectedItem).entityId);
            }
        }

        private void btnSentCommand_Click(object sender, RoutedEventArgs e)
        {
            Send_Command_Text();
        }

        private void txtCommand_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key) {
                case System.Windows.Input.Key.Enter:
                    Send_Command_Text();
                        break;
                }
        }

        private void Send_Command_Text()
        {
            if (txtCommand.Text != "")
            {
                Send_Command(txtCommand.Text);
                txtCommand.Text = "";
            }
        }

        private void btnGetAlliances_Click(object sender, RoutedEventArgs e)
        {
            Get_Alliances();
        }

        private void btnGetFactions_Click(object sender, RoutedEventArgs e)
        {
            Get_Factions();
        }

        private void mnuGet_Structure_BlockStatistics_Click(object sender, RoutedEventArgs e)
        {
            if (dgStructures.SelectedItem != null)
            {
                Get_Structure_BlockStatistics(((data.StructureInfo)dgStructures.SelectedItem).id);
            }            
        }

        private void mnuTouch_Click(object sender, RoutedEventArgs e)
        {
            if (dgStructures.SelectedItem != null)
            {
                Touch_Structure(((data.StructureInfo)dgStructures.SelectedItem).id);
            }
        }

        private void btnClearOutput_Click(object sender, RoutedEventArgs e)
        {
            mainWindowDataContext.output.Clear();
            mainWindowDataContext.events.Clear();
            mainWindowDataContext.stats.Clear();
        }

        private void btnReloadPlayfield_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("playfield");
        }

        private void btnGetBannedPlayers_Click(object sender, RoutedEventArgs e)
        {
            GetBannedPlayers();
        }

        private void mnuGetAndRemoveInventory_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlayer.SelectedItem != null)
            {
                GetAndRemoveInventory(((data.PlayerInfo)dgPlayer.SelectedItem).entityId);
            }            
        }

        private void mnuItemExchange_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlayer.SelectedItem != null)
            {
                ItemExchange(((data.PlayerInfo)dgPlayer.SelectedItem).entityId);
            }
        }

        private void mnuDestroyEntity_Click(object sender, RoutedEventArgs e)
        {
            if (dgEntities.SelectedItem != null)
            {
                Entity_Destroy2(((data.EntityInfo)dgEntities.SelectedItem).id, ((data.EntityInfo)dgEntities.SelectedItem).playfield);
            }
        }

        private void mnuGetExportInfo_Click(object sender, RoutedEventArgs e)
        {
            if (dgStructures.SelectedItem != null)
            {
                Request_Entity_Export(((data.StructureInfo)dgStructures.SelectedItem).id);
            }
        }

        private void mnuImportStructure_Click(object sender, RoutedEventArgs e)
        {
            windows.SpawnEntity wdInput = new windows.SpawnEntity();
                        wdInput.ShowDialog();

            if (System.IO.File.Exists(wdInput.txtExportFile.Text) == false)
            {
                mainWindowDataContext.output.Add("Export file not found");
                return;
            }

            EntitySpawn(System.Convert.ToInt32(wdInput.txtID.Text), wdInput.txtPrefabName.Text, wdInput.txtExportFile.Text, wdInput.txtPlayfield.Text);
            wdInput = null;
        }

        private void btnGetNewID_Click(object sender, RoutedEventArgs e)
        {
            Request_NewID();
        }
    }
}
