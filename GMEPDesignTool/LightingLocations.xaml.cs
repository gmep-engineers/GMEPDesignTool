using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
    /// Interaction logic for LightingLocations.xaml
    /// </summary>
    public partial class LightingLocations : Window
    {
        public Database.Database database = new Database.Database();
        public ObservableCollection<Location> Locations { get; set; } = new ObservableCollection<Location>();
        public ElectricalLighting Lighting { get; set; }

        private Location selectedLocation;

        public Location SelectedLocation
        {
            get => selectedLocation;
            set
            {
                selectedLocation = value;
                //OnPropertyChanged();
            }
        }
        public LightingLocations(ObservableCollection<Location> locations, ElectricalLighting lighting)
        {
            Locations = locations;
            Lighting = lighting;
            InitializeComponent();
            DataContext = this;

            var converter = (IdToLocationDescriptionConverter)Resources["IdToLocationDescriptionConverter"];
            converter.Locations = Locations;
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            Lighting.LocationId = SelectedLocation.Id;
        }
    }
    public class IdToLocationDescriptionConverter : IValueConverter
    {
        public ObservableCollection<Location> Locations { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string id && Locations != null)
            {
                var location = Locations.FirstOrDefault(l => l.Id == id);
                return location?.LocationDescription ?? string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string description && Locations != null)
            {
                var location = Locations.FirstOrDefault(l => l.LocationDescription == description);
                return location?.Id ?? string.Empty;
            }
            return string.Empty;
        }
    }
    public class Location : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string id;
        public string locationDescription;
        public bool isOutside;


        public Location()
        {
            id = Guid.NewGuid().ToString();
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
        public string LocationDescription
        {
            get => locationDescription;
            set
            {
                if (locationDescription != value)
                {
                    locationDescription = value;
                    OnPropertyChanged(nameof(LocationDescription));
                }
            }
        }
        public bool IsOutside
        {
            get => isOutside;
            set
            {
                if (isOutside != value)
                {
                    isOutside = value;
                    OnPropertyChanged(nameof(IsOutside));
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
