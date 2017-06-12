using ENRC.data;
using System.Collections.ObjectModel;

namespace ENRC
{
    public class MainWindowDataContext
    {
        public ObservableCollection<string> output { get; set; }
        public ObservableCollection<string> onlinePlayfields { get; set; }
        public ObservableCollection<StructureInfo> structures { get; set; }
        public ObservableCollection<int> onlinePlayer { get; set; }
        public ObservableCollection<PlayerInfo> playerInfos { get; set; }
        public ObservableCollection<string> events{ get; set; }
        public ObservableCollection<string> stats { get; set; }

        public bool EnableOutput_SendRequest { get; set; } = true;
        public bool EnableOutput_Event_Player_List { get; set; } = true;
        public bool EnableOutput_Event_Player_Info { get; set; } = true;
        public bool EnableOutput_Event_Player_Inventory { get; set; } = true;
        public bool EnableOutput_Event_Entity_PosAndRot { get; set; } = true;
        public bool EnableOutput_Event_Player_Credits { get; set; } = true;
        public bool EnableOutput_Event_GlobalStructure_List { get; set; } = true;
        public bool EnableOutput_Event_Playfield_List { get; set; } = true;
        public bool EnableOutput_DataRecieved { get; set; } = true;

        public MainWindowDataContext()
        {
            output = new ObservableCollection<string>();
            onlinePlayfields = new ObservableCollection<string>();
            structures = new ObservableCollection<StructureInfo>();
            onlinePlayer = new ObservableCollection<int>();
            playerInfos = new ObservableCollection<PlayerInfo>();
            events = new ObservableCollection<string>();
            stats = new ObservableCollection<string>();
        }
    }
}
