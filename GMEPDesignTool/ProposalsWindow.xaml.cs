using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace GMEPDesignTool
{
  /// <summary>
  /// Interaction logic for ProposalsWindow.xaml
  /// </summary>
  ///

  public partial class ProposalsWindow : Window
  {
    public ProposalsViewModel ViewModel { get; set; }

    public ProposalsWindow(LoginResponse loginResponse)
    {
      ViewModel = new ProposalsViewModel(loginResponse);
      this.DataContext = ViewModel;
      InitializeComponent();
    }

    public void SaveClick(object sender, RoutedEventArgs e) { }

    public void ProposalYearListViewItem_Click(object sender, RoutedEventArgs e)
    {
      string year = (string)ProposalYearListView.SelectedItem;
      ViewModel.FilterDataGridByYear(year);
    }
  }
}
