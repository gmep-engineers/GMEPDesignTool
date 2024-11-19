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
  }
}
