using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Empyrion_Mod_Server
{
    [PropertyChanged.ImplementPropertyChanged()]
    public class ObservableClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
       
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
