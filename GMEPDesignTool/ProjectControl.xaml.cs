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
    public ProjectControl()
    {
      InitializeComponent();
      ElectricalPanels = new ObservableCollection<ElectricalPanel>();
    }

    public void AddElectricalPanel(ElectricalPanel electricalPanel)
    {
      ElectricalPanels.Add(electricalPanel);
    }

    public void AddNewElectricalPanel(object sender, EventArgs e)
    {
      Trace.WriteLine("new panel");
      ElectricalPanel electricalPanel = new ElectricalPanel("0", 100, 100, false, false, "", 0, "");
      ElectricalPanels.Add(electricalPanel);
      
    }

    public void RemoveElectricalPanel(ElectricalPanel electricalPanel)
    {
      ElectricalPanels.Remove(electricalPanel);
    }

  }
}
