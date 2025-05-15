using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Interaction logic for ElectricalPanelMiniBreakerWindow.xaml
    /// </summary>
    public partial class ElectricalPanelMiniBreakerWindow : Window
    {
        ElectricalPanelMiniBreakerViewModel ViewModel { get; set; }

        public ElectricalPanelMiniBreakerWindow(
            Database.Database database,
            ObservableCollection<ElectricalComponent> unassignedEquipment,
            string panelId,
            string panelName,
            Circuit circuit,
            Circuit? nextCircuit
        )
        {
            ViewModel = new ElectricalPanelMiniBreakerViewModel(
                database,
                unassignedEquipment,
                panelId,
                panelName,
                circuit,
                nextCircuit
            );
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void WindowClosing(object sender, EventArgs e)
        {
            ViewModel.Save();
        }

        private void DeleteMiniBreakerButton_Click(object sender, EventArgs e)
        {
            ViewModel.Delete();
            Close();
        }
    }
}
