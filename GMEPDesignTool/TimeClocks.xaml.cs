using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for TimeClocks.xaml
    /// </summary>
    public partial class TimeClocks : Window
    {
        public Database.Database database;
        public ObservableCollection<TimeClock> Clocks { get; set; } =
            new ObservableCollection<TimeClock>();
        public ObservableDictionary<string, string> PanelNames { get; set; } = new ObservableDictionary<string, string>();

        public TimeClocks(
            ObservableCollection<TimeClock> clocks,
            Database.Database database, ObservableDictionary<string, string> panelNames
        )
        {

            this.database = database;
            Clocks = clocks;
            PanelNames = panelNames;
            InitializeComponent();
            DataContext = this;
        }
    }
    public class TimeClock : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string id;
        public string name;
        public string bypassSwitchName;
        public string bypassSwitchLocation;
        public int voltageId;
        public string adjacentPanelId;

        public TimeClock()
        {
            id = Guid.NewGuid().ToString();
            voltageId = 1;
            bypassSwitchName = "";
            bypassSwitchLocation = "";
            adjacentPanelId = "";
            Name = "";

        }

        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        public string Name
        {
            get => name;
            set
            {
                if (name!= value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string BypassSwitchName
        {
            get => bypassSwitchName;
            set
            {
                if (bypassSwitchName!= value)
                {
                    bypassSwitchName = value;
                    OnPropertyChanged(nameof(BypassSwitchName));
                }
            }
        }
        public string BypassSwitchLocation
        {
            get => bypassSwitchLocation;
            set
            {
                if (bypassSwitchLocation!= value)
                {
                    bypassSwitchLocation = value;
                    OnPropertyChanged(nameof(BypassSwitchLocation));
                }
            }
        }
        public int VoltageId
        {
            get => voltageId;
            set
            {
                if (voltageId != value)
                {
                    voltageId = value;
                    OnPropertyChanged(nameof(VoltageId));
                }
            }
        }
        public string AdjacentPanelId
        {
            get => adjacentPanelId;
            set
            {
                if (adjacentPanelId!= value)
                {
                    adjacentPanelId = value;
                    OnPropertyChanged(nameof(AdjacentPanelId));
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
