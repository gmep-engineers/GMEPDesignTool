using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace GMEPDesignTool
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  /// 
  public partial class MainWindow
  {
    public ViewModel MainWindowViewModel { get; set; }
    public MainWindow()
    {
      MainWindowViewModel = new ViewModel();
      DataContext = MainWindowViewModel;
      InitializeComponent();
    }

    public void MainWindowOpenProject(object sender, MouseButtonEventArgs e)
    {
      string projectName = (string)ProjectList.SelectedItem;
      MainWindowViewModel.OpenProject(projectName);
    }

    public void MainWindowCloseProject(object sender, RoutedEventArgs e)
    {
      TabItem projectTab = (TabItem)ProjectTabs.SelectedItem;
      MainWindowViewModel.CloseProject(projectTab);
    }
  }
}
