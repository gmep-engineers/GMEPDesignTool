using System;
using System.Collections.Generic;
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
    /// Interaction logic for SetVaPerPhaseWindow.xaml
    /// </summary>
    public partial class SetVaPerPhaseWindow : Window
    {
        SetVaPerPhaseViewModel ViewModel;

        public SetVaPerPhaseWindow(ElectricalEquipment equip)
        {
            ViewModel = new SetVaPerPhaseViewModel(equip);
            this.DataContext = ViewModel;
            InitializeComponent();
        }

        public void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Reset();
            Close();
        }
    }
}
