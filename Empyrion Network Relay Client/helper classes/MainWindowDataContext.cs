using ENRC.data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ENRC
{
    public class MainWindowDataContext : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        private string _connected = "Disconnected";

        public ObservableCollection<string> output { get; set; }
        public ObservableCollection<string> onlinePlayfields { get; set; }
        public ObservableCollection<StructureInfo> structures { get; set; }
        public ObservableCollection<int> onlinePlayer { get; set; }
        public ObservableCollection<PlayerInfo> playerInfos { get; set; }
        public ObservableCollection<string> events { get; set; }
        public ObservableCollection<string> stats { get; set; }
        public ObservableCollection<EntityInfo> entities { get; set; }

        public bool EnableOutput_SendRequest { get; set; } = true;
        public bool EnableOutput_Event_Player_List { get; set; } = true;
        public bool EnableOutput_Event_Player_Info { get; set; } = true;
        public bool EnableOutput_Event_Player_Inventory { get; set; } = true;
        public bool EnableOutput_Event_Entity_PosAndRot { get; set; } = true;
        public bool EnableOutput_Event_Player_Credits { get; set; } = true;
        public bool EnableOutput_Event_GlobalStructure_List { get; set; } = true;
        public bool EnableOutput_Event_Playfield_List { get; set; } = true;
        public bool EnableOutput_DataRecieved { get; set; } = true;
        public string Connected { get => _connected; set { _connected = value; OnPropertyChanged("Connected"); } }

        public MainWindowDataContext()
        {
            output = new ObservableCollection<string>();
            onlinePlayfields = new ObservableCollection<string>();
            structures = new ObservableCollection<StructureInfo>();
            onlinePlayer = new ObservableCollection<int>();
            playerInfos = new ObservableCollection<PlayerInfo>();
            events = new ObservableCollection<string>();
            stats = new ObservableCollection<string>();
            entities = new ObservableCollection<EntityInfo>();
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
