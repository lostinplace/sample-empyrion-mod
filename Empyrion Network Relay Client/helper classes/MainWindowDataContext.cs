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
