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
    /// Interaction logic for UpdatePasswordWindow.xaml
    /// </summary>
    public partial class UpdatePasswordWindow : Window
    {
        UpdatePasswordViewModel UpdatePasswordViewModel { get; set; }

        public UpdatePasswordWindow(Database.Database database, string employeeId)
        {
            UpdatePasswordViewModel = new UpdatePasswordViewModel(database, employeeId);
            this.DataContext = UpdatePasswordViewModel;
            InitializeComponent();
        }

        public void UpdatePasswordClick(object sender, RoutedEventArgs e)
        {
            UpdatePasswordViewModel.SetPassword();
            Close();
        }
    }
}
