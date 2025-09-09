using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Google.Protobuf;

namespace GMEPDesignTool
{
  public class Client : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool _Modified = false;
    public bool Modified
    {
      get => _Modified;
      set => _Modified = value;
    }

    private bool _New = true;
    public bool New
    {
      get => _New;
      set => _New = value;
    }

    private bool _Delete = false;
    public bool Delete
    {
      get => _Delete;
      set
      {
        if (_Delete != value)
        {
          _Delete = value;
          OnPropertyChanged(nameof(Delete));
        }
      }
    }

    private bool _NewPhoneNumber = false;
    public bool NewPhoneNumber
    {
      get => _NewPhoneNumber;
      set => _NewPhoneNumber = value;
    }

    private bool _NewEmailAddress = false;
    public bool NewEmailAddress
    {
      get => _NewEmailAddress;
      set => _NewEmailAddress = value;
    }

    private bool _NewPrimaryContact = false;
    public bool NewPrimaryContact
    {
      get => _NewPrimaryContact;
      set => _NewPrimaryContact = value;
    }

    private string _CompanyId = string.Empty;
    public string CompanyId
    {
      get => _CompanyId;
    }

    private string _EntityId = String.Empty;
    public string EntityId
    {
      get => _EntityId;
    }

    private string _CompanyName;
    public string CompanyName
    {
      get => _CompanyName;
      set
      {
        if (_CompanyName != value)
        {
          _CompanyName = value;
          OnPropertyChanged(nameof(CompanyName));
          _Modified = true;
        }
      }
    }

    private int _LoyaltyTypeId;
    public int LoyaltyTypeId
    {
      get => _LoyaltyTypeId;
      set
      {
        if (_LoyaltyTypeId != value)
        {
          _LoyaltyTypeId = value;
          OnPropertyChanged(nameof(LoyaltyTypeId));
          _Modified = true;
        }
      }
    }

    private string _StreetAddress;
    public string StreetAddress
    {
      get => _StreetAddress;
      set
      {
        if (_StreetAddress != value)
        {
          _StreetAddress = value;
          OnPropertyChanged(nameof(StreetAddress));
          _Modified = true;
        }
      }
    }

    private string _City;
    public string City
    {
      get => _City;
      set
      {
        if (_City != value)
        {
          _City = value;
          OnPropertyChanged(nameof(City));
          _Modified = true;
        }
      }
    }

    private string _State;
    public string State
    {
      get => _State;
      set
      {
        if (_State != value)
        {
          _State = value;
          OnPropertyChanged(nameof(State));
          _Modified = true;
        }
      }
    }

    private string _PostalCode;
    public string PostalCode
    {
      get => _PostalCode;
      set
      {
        if (_PostalCode != value)
        {
          _PostalCode = value;
          OnPropertyChanged(nameof(PostalCode));
          _Modified = true;
        }
      }
    }

    private string _CompanyEmailId;
    public string CompanyEmailId
    {
      get => _CompanyEmailId;
      set => _CompanyEmailId = value;
    }

    private string _CompanyEmail;
    public string CompanyEmail
    {
      get => _CompanyEmail;
      set
      {
        if (_CompanyEmail != value)
        {
          _CompanyEmail = value;
          OnPropertyChanged(nameof(CompanyEmail));
          _Modified = true;
          _NewEmailAddress = true;
        }
      }
    }

    private string _CompanyPhoneId;
    public string CompanyPhoneId
    {
      get => _CompanyPhoneId;
      set => _CompanyPhoneId = value;
    }

    private ulong? _CompanyPhone;
    public ulong? CompanyPhone
    {
      get => _CompanyPhone;
      set
      {
        if (_CompanyPhone != value)
        {
          _CompanyPhone = value;
          OnPropertyChanged(nameof(CompanyPhone));
          _Modified = true;
          _NewPhoneNumber = true;
        }
      }
    }

    private uint? _CompanyExtension;
    public uint? CompanyExtension
    {
      get => _CompanyExtension;
      set
      {
        if (_CompanyExtension != value)
        {
          _CompanyExtension = value;
          OnPropertyChanged(nameof(CompanyExtension));
          _Modified = true;
          _NewPhoneNumber = true;
        }
      }
    }

    private string _PrimaryContactId;

    public string PrimaryContactId
    {
      get => _PrimaryContactId;
      set => _PrimaryContactId = value;
    }

    private string _PrimaryContactName;
    public string PrimaryContactName
    {
      get => _PrimaryContactName;
      set => _PrimaryContactName = value;
    }

    public Client(
      string id,
      string entityId,
      string name,
      int loyaltyTypeId,
      string streetAddress,
      string city,
      string state,
      string postalCode,
      string emailAddressId,
      string emailAddress,
      string phoneNumberId,
      ulong? phoneNumber,
      uint? extension,
      string primaryContactId,
      string primaryContactFirstName,
      string primaryContactLastName
    )
    {
      _CompanyId = id;
      _EntityId = entityId;
      _CompanyName = name;
      _LoyaltyTypeId = loyaltyTypeId;
      _StreetAddress = streetAddress;
      _City = city;
      _State = state;
      _PostalCode = postalCode;
      _CompanyEmailId = emailAddressId;
      _CompanyEmail = emailAddress;
      _CompanyPhoneId = phoneNumberId;
      _CompanyPhone = phoneNumber;
      _CompanyExtension = extension;
      _PrimaryContactId = primaryContactId;
      if (string.IsNullOrEmpty(_PrimaryContactId))
      {
        _PrimaryContactName = "Assign";
      }
      else
      {
        _PrimaryContactName = primaryContactFirstName + " " + primaryContactLastName;
      }
    }

    public Client()
    {
      _CompanyId = Guid.NewGuid().ToString();
      _EntityId = Guid.NewGuid().ToString();
      _CompanyName = string.Empty;
      _LoyaltyTypeId = 3;
      _StreetAddress = string.Empty;
      _City = string.Empty;
      _State = string.Empty;
      _PostalCode = string.Empty;
      _CompanyEmailId = Guid.NewGuid().ToString();
      _CompanyEmail = string.Empty;
      _CompanyPhoneId = Guid.NewGuid().ToString();
      _CompanyPhone = 0;
      _CompanyExtension = 0;
      _PrimaryContactId = string.Empty;
      _NewPhoneNumber = true;
      _NewEmailAddress = true;
    }
  }

  public class Contact : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool _Modified = false;
    public bool Modified
    {
      get => _Modified;
      set => _Modified = value;
    }

    private bool _New = true;
    public bool New
    {
      get => _New;
      set => _New = value;
    }

    private bool _Delete = false;
    public bool Delete
    {
      get => _Delete;
      set
      {
        if (_Delete != value)
        {
          _Delete = value;
          OnPropertyChanged(nameof(Delete));
        }
      }
    }

    private string _Id = string.Empty;
    public string Id
    {
      get => _Id;
      set => _Id = value;
    }

    private string _EntityId = string.Empty;
    public string EntityId
    {
      get => _EntityId;
      set => _EntityId = value;
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
          OnPropertyChanged(nameof(FirstName));
          _Modified = true;
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
          OnPropertyChanged(nameof(LastName));
          _Modified = true;
        }
      }
    }

    private string _CompanyId = string.Empty;
    public string CompanyId
    {
      get => _CompanyId;
      set => _CompanyId = value;
    }

    private string _CompanyName = string.Empty;
    public string CompanyName
    {
      get => _CompanyName;
      set => _CompanyName = value;
    }

    private string _EmailAddressId = string.Empty;
    public string EmailAddressId
    {
      get => _EmailAddressId;
      set => _EmailAddressId = value;
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
          OnPropertyChanged(nameof(EmailAddress));
          _Modified = true;
        }
      }
    }

    private string _PhoneNumberId = string.Empty;
    public string PhoneNumberId
    {
      get => _PhoneNumberId;
      set => _PhoneNumberId = value;
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
          OnPropertyChanged(nameof(PhoneNumber));
          _Modified = true;
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
          OnPropertyChanged(nameof(Extension));
          _Modified = true;
        }
      }
    }

    public Contact(
      string id,
      string entityId,
      string firstName,
      string lastName,
      string companyId,
      string companyName,
      string emailAddressId,
      string emailAddress,
      string phoneNumberId,
      ulong? phoneNumber,
      uint? extension
    )
    {
      _Id = id;
      _EntityId = entityId;
      _FirstName = firstName;
      _LastName = lastName;
      _CompanyId = companyId;
      _CompanyName = companyName;
      _EmailAddressId = emailAddressId;
      _EmailAddress = emailAddress;
      _PhoneNumberId = phoneNumberId;
      _PhoneNumber = phoneNumber;
      _Extension = extension;
    }

    public Contact()
    {
      _Id = Guid.NewGuid().ToString();
      _EntityId = Guid.NewGuid().ToString();
      _FirstName = string.Empty;
      _LastName = string.Empty;
      _CompanyId = string.Empty;
      _EmailAddressId = Guid.NewGuid().ToString();
      _EmailAddress = string.Empty;
      _PhoneNumberId = Guid.NewGuid().ToString();
      _PhoneNumber = 0;
      _Extension = 0;
    }
  }

  class ClientsViewModel : ViewModelBase
  {
    public List<Client> Clients { get; set; }
    public Database.Database Database { get; set; }
    public Client? SelectedClient { get; set; }

    public Contact? SelectedContact { get; set; }

    public ObservableCollection<Client> AllClients { get; set; }

    public ObservableCollection<Contact> AllContacts { get; set; }

    public ClientsViewModel(LoginResponse loginResponse)
    {
      Database = new Database.Database(loginResponse.SqlConnectionString);
      AllClients = new ObservableCollection<Client>(Database.GetClients());
      AllContacts = new ObservableCollection<Contact>(Database.GetContacts());
      foreach (Client client in AllClients)
      {
        client.New = false;
      }
    }

    public void Save()
    {
      List<Client> deletedClients = new List<Client>();
      foreach (Client client in AllClients)
      {
        if (client.Delete)
        {
          Database.DeleteClient(client);
          deletedClients.Add(client);
        }
        else if (client.Modified)
        {
          Database.SaveClient(client);
          client.Modified = false;
        }
      }
      foreach (Client client in deletedClients)
      {
        AllClients.Remove(client);
      }
    }

    public void SaveClientOnEnter()
    {
      foreach (Client client in AllClients)
      {
        if (client.Modified)
        {
          Database.SaveClient(client);
          client.Modified = false;
        }
      }
    }

    public void SaveContactOnEnter()
    {
      foreach (Contact contact in AllContacts)
      {
        if (contact.Modified)
        {
          Database.SaveContact(contact);
          contact.Modified = false;
        }
      }
    }

    public void FlagClientForDeletion()
    {
      if (SelectedClient != null)
      {
        SelectedClient.Delete = !SelectedClient.Delete;
      }
    }

    public void FlagContactForDeletion()
    {
      if (SelectedContact != null)
      {
        SelectedContact.Delete = !SelectedContact.Delete;
      }
    }
  }
}
