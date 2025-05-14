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
            IEnumerable<ElectricalComponent> unassignedEquipment,
            string panelId,
            string panelName,
            int circuitNo,
            string equipIdA = "",
            string equipIdB = "",
            int breakerSizeA = 20,
            int breakerSizeB = 20,
            bool interlockA = false,
            bool interlockB = false
        )
        {
            ViewModel = new ElectricalPanelMiniBreakerViewModel(
                database,
                unassignedEquipment,
                panelId,
                panelName,
                circuitNo,
                equipIdA,
                equipIdB,
                breakerSizeA,
                breakerSizeB,
                interlockA,
                interlockB
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
