using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class AddEditContactViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _CompanyName;
    public string CompanyName
    {
      get => _CompanyName;
    }

    private int _ClientLoyaltyTypeId;
    public int ClientLoyaltyTypeId
    {
      get => _ClientLoyaltyTypeId;
    }

    private string _StreetAddress;
    public string StreetAddres
    {
      get => _StreetAddress;
    }

    private string _City;
    public string City
    {
      get => _City;
    }

    private string _State;
    public string State
    {
      get => _State;
    }

    private string _PostalCode;
    public string PostalCode
    {
      get => _PostalCode;
    }

    private string _CompanyEmailAddress;
    public string CompanyEmailAddress
    {
      get => _CompanyEmailAddress;
    }

    private ulong? _CompanyPhone;
    public ulong? CompanyPhone
    {
      get => _CompanyPhone;
    }

    private uint? _CompanyExtension;
    public uint? CompanyExtension
    {
      get => _CompanyExtension;
    }

    private string _PrimaryContact;
    public string PrimaryContact
    {
      get => _PrimaryContact;
    }

    private string _SearchText = string.Empty;
    public string SearchText
    {
      get => _SearchText;
      set
      {
        if (_SearchText != value)
        {
          _SearchText = value;
          OnPropertyChanged(nameof(SearchText));
        }
      }
    }

    private string _FirstName = string.Empty;
    public string FirstName
    {
      get => _FirstName;
      set
      {
        if (_FirstName != value)
        {
          _FirstName = value;
          OnPropertyChanged(nameof(_FirstName));
        }
      }
    }

    private string _LastName = string.Empty;
    public string LastName
    {
      get => _LastName;
      set
      {
        if (_LastName != value)
        {
          _LastName = value;
          OnPropertyChanged(nameof(_LastName));
        }
      }
    }

    private string _EmailAddress = string.Empty;
    public string EmailAddress
    {
      get => _EmailAddress;
      set
      {
        if (_EmailAddress != value)
        {
          _EmailAddress = value;
          OnPropertyChanged(nameof(_EmailAddress));
        }
      }
    }

    private ulong? _PhoneNumber = 0;
    public ulong? PhoneNumber
    {
      get => _PhoneNumber;
      set
      {
        if (_PhoneNumber != value)
        {
          _PhoneNumber = value;
          OnPropertyChanged(nameof(_PhoneNumber));
        }
      }
    }

    private uint? _Extension = 0;
    public uint? Extension
    {
      get => _Extension;
      set
      {
        if (_Extension != value)
        {
          _Extension = value;
          OnPropertyChanged(nameof(_Extension));
        }
      }
    }

    public AddEditContactViewModel(
      string companyName,
      int clientLoyaltyTypeId,
      string streetAddress,
      string city,
      string state,
      string postalCode,
      string companyEmailAddress,
      ulong? companyPhone,
      uint? companyExtension,
      string primaryContact
    )
    {
      _CompanyName = companyName;
      _ClientLoyaltyTypeId = clientLoyaltyTypeId;
      _StreetAddress = streetAddress;
      _City = city;
      _State = state;
      _PostalCode = postalCode;
      _CompanyEmailAddress = companyEmailAddress;
      _CompanyPhone = companyPhone;
      _CompanyExtension = companyExtension;
      _PrimaryContact = primaryContact;
    }
  }
}
