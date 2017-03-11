﻿using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Empyrion_Mod_Server
{
    public partial class MainWindow : Window
    {
        private Timer aTimer;
        private int counter;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            startModServer();

            System.Threading.Thread.Sleep(1000);

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
            stopModServer();
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

                windows.ChangeLocation wdChangeLocation = new windows.ChangeLocation();
                wdChangeLocation.DataContext = player;
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
    }
}