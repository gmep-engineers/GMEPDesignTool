using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace GMEPDesignTool
{
    public partial class Calculator : Window
    {
        public Calculator(
            ObservableCollection<ElectricalService> services,
            ObservableCollection<ElectricalPanel> panels,
            ObservableCollection<ElectricalTransformer> transformers,
            ObservableCollection<ElectricalEquipment> equipments,
            ObservableCollection<ElectricalLighting> lightings
        )
        {
            InitializeComponent();

            // Create the ViewModel
            var viewModel = new CalculatorViewModel(
                services,
                panels,
                transformers,
                equipments,
                lightings
            );

            // Set the DataContext of the Window (or the specific DataGrid)
            this.DataContext = viewModel;
        }
    }
}
