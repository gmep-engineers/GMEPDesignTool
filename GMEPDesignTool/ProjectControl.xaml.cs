using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GMEPDesignTool
{
  /// <summary>
  /// Interaction logic for ProjectControl.xaml
  /// </summary>
  public partial class ProjectControl : UserControl
  {

        public ObservableCollection<ElectricalPanel> ElectricalPanels { get; set; }
        public ObservableCollection<ElectricalService> ElectricalServices { get; set; }
        public ProjectControl()
        {
              InitializeComponent();
              ElectricalPanels = new ObservableCollection<ElectricalPanel>();
              ElectricalServices = new ObservableCollection<ElectricalService>();
              this.DataContext = this;
        }

        public void AddElectricalPanel(ElectricalPanel electricalPanel)
        {
          ElectricalPanels.Add(electricalPanel);
        }

        public void AddNewElectricalPanel(object sender, EventArgs e)
        {
          Trace.WriteLine("new panel");
          ElectricalPanel electricalPanel = new ElectricalPanel("0", 100, 100, false, false, "", 0, "MS-1");
          ElectricalPanels.Add(electricalPanel);
        }

        public void RemoveElectricalPanel(ElectricalPanel electricalPanel)
        {
          ElectricalPanels.Remove(electricalPanel);
        }
        public void DeleteSelectedElectricalPanel(object sender, EventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.CommandParameter is ElectricalPanel electricalPanel)
                {
                    RemoveElectricalPanel(electricalPanel);
                }
        }

        //Service Functions
        public void AddElectricalService(ElectricalService electricalService)
        {
            ElectricalServices.Add(electricalService);
        }

        public void AddNewElectricalService(object sender, EventArgs e)
        {
            Trace.WriteLine("new service");
            ElectricalService electricalService = new ElectricalService("0", "0", "meow", "", 0);
            ElectricalServices.Add(electricalService);
        }

        public void RemoveElectricalService(ElectricalService electricalService)
        {
            ElectricalServices.Remove(electricalService);
        }
        public void DeleteSelectedElectricalService(object sender, EventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.CommandParameter is ElectricalService electricalService)
            {
                RemoveElectricalService(electricalService);
            }
        }
    }
}
