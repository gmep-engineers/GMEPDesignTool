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
    /// Interaction logic for LoadSummary.xaml
    /// </summary>
    public partial class LoadSummary : Window
    {
        LoadSummaryViewModel viewModel { get; set; }
        public LoadSummary(ElectricalService service)
        {
            InitializeComponent();
            viewModel = new LoadSummaryViewModel(service);
            this.DataContext = viewModel;
        }
    }
}
