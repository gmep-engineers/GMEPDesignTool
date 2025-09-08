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
  /// Interaction logic for AddEditContactWindow.xaml
  /// </summary>
  public partial class AddEditContactWindow : Window
  {
    AddEditContactViewModel ViewModel { get; set; }

    public AddEditContactWindow(Client client)
    {
      InitializeComponent();
      ViewModel = new AddEditContactViewModel(
        client.CompanyName,
        client.ClientLoyaltyTypeId,
        client.StreetAddress,
        client.City,
        client.State,
        client.PostalCode,
        client.CompanyEmail,
        client.CompanyPhone,
        client.CompanyExtension,
        client.PrimaryContactName
      );
      this.DataContext = ViewModel;
    }
  }
}
