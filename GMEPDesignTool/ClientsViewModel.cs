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
using MySqlX.XDevAPI;

namespace GMEPDesignTool
{
  public class Company : INotifyPropertyChanged
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

          if (Client != null)
          {
            Client.Delete = value;
          }

          if (Architect != null)
          {
            Architect.Delete = value;
          }
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
      set => _CompanyId = value;
    }

    private string _EntityId = String.Empty;
    public string EntityId
    {
      get => _EntityId;
      set => _EntityId = value;
    }

    private string _CompanyName = string.Empty;
    public string CompanyName
    {
      get => _CompanyName;
      set
      {
        if (_CompanyName != value && value.Length <= 255)
        {
          _CompanyName = value;
          OnPropertyChanged(nameof(CompanyName));
          _Modified = true;

          if (Client != null)
          {
            Client.CompanyName = value;
          }

          if (Architect != null)
          {
            Architect.CompanyName = value;
          }
        }
      }
    }

    private string _StreetAddress = string.Empty;
    public string StreetAddress
    {
      get => _StreetAddress;
      set
      {
        if (_StreetAddress != value && value.Length <= 255)
        {
          _StreetAddress = value;
          OnPropertyChanged(nameof(StreetAddress));
          _Modified = true;

          if (Client != null)
          {
            Client.StreetAddress = value;
          }

          if (Architect != null)
          {
            Architect.StreetAddress = value;
          }
        }
      }
    }

    private string _City = string.Empty;
    public string City
    {
      get => _City;
      set
      {
        if (_City != value && value.Length <= 255)
        {
          _City = value;
          OnPropertyChanged(nameof(City));
          _Modified = true;

          if (Client != null)
          {
            Client.City = value;
          }

          if (Architect != null)
          {
            Architect.City = value;
          }
        }
      }
    }

    private string _State = string.Empty;
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

          if (Client != null)
          {
            Client.State = value;
          }

          if (Architect != null)
          {
            Architect.State = value;
          }
        }
      }
    }

    private string _PostalCode = string.Empty;
    public string PostalCode
    {
      get => _PostalCode;
      set
      {
        if (_PostalCode != value && value.Length <= 15)
        {
          _PostalCode = value;
          OnPropertyChanged(nameof(PostalCode));
          _Modified = true;

          if (Client != null)
          {
            Client.PostalCode = value;
          }

          if (Architect != null)
          {
            Architect.PostalCode = value;
          }
        }
      }
    }

    private string _CompanyEmailId = string.Empty;
    public string CompanyEmailId
    {
      get => _CompanyEmailId;
      set => _CompanyEmailId = value;
    }

    private string _CompanyEmail = string.Empty;
    public string CompanyEmail
    {
      get => _CompanyEmail;
      set
      {
        if (_CompanyEmail != value && value.Length <= 63)
        {
          _CompanyEmail = value;
          OnPropertyChanged(nameof(CompanyEmail));
          _Modified = true;
          _NewEmailAddress = true;

          if (Client != null)
          {
            Client.CompanyEmail = value;
          }

          if (Architect != null)
          {
            Architect.CompanyEmail = value;
          }
        }
      }
    }

    private string _CompanyPhoneId = string.Empty;
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

          if (Client != null)
          {
            Client.CompanyPhone = value;
          }

          if (Architect != null)
          {
            Architect.CompanyPhone = value;
          }
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

          if (Client != null)
          {
            Client.CompanyExtension = value;
          }

          if (Architect != null)
          {
            Architect.CompanyExtension = value;
          }
        }
      }
    }

    private string _PrimaryContactId = string.Empty;

    public string PrimaryContactId
    {
      get => _PrimaryContactId;
      set => _PrimaryContactId = value;
    }

    private string _PrimaryContactName = string.Empty;
    public string PrimaryContactName
    {
      get => _PrimaryContactName;
      set
      {
        if (_PrimaryContactName != value)
        {
          _PrimaryContactName = value;
          OnPropertyChanged(nameof(PrimaryContactName));
        }
      }
    }

    private Architect? _Architect;
    public Architect? Architect
    {
      get => _Architect;
      set => _Architect = value;
    }

    private Client? _Client;
    public Client? Client
    {
      get => _Client;
      set => _Client = value;
    }

    private bool _IsArchitect;
    public bool IsArchitect
    {
      get => _IsArchitect;
      set
      {
        if (_IsArchitect != value)
        {
          _IsArchitect = value;
          OnPropertyChanged(nameof(IsArchitect));
          if (_IsArchitect) { }
          Modified = true;
        }
      }
    }

    private bool _IsClient;
    public bool IsClient
    {
      get => _IsClient;
      set
      {
        if (_IsClient != value)
        {
          _IsClient = value;
          OnPropertyChanged(nameof(IsClient));
          Modified = true;
        }
      }
    }
  }

  public class Client : Company
  {
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
          Modified = true;
        }
      }
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
      CompanyId = id;
      EntityId = entityId;
      CompanyName = name;
      _LoyaltyTypeId = loyaltyTypeId;
      StreetAddress = streetAddress;
      City = city;
      State = state;
      PostalCode = postalCode;
      CompanyEmailId = emailAddressId;
      CompanyEmail = emailAddress;
      CompanyPhoneId = phoneNumberId;
      CompanyPhone = phoneNumber;
      CompanyExtension = extension;
      PrimaryContactId = primaryContactId;
      if (string.IsNullOrEmpty(PrimaryContactId))
      {
        PrimaryContactName = "Assign";
      }
      else
      {
        PrimaryContactName = primaryContactFirstName + " " + primaryContactLastName;
      }
    }

    public Client()
    {
      CompanyId = Guid.NewGuid().ToString();
      EntityId = Guid.NewGuid().ToString();
      CompanyName = string.Empty;
      LoyaltyTypeId = 3;
      StreetAddress = string.Empty;
      City = string.Empty;
      State = string.Empty;
      PostalCode = string.Empty;
      CompanyEmailId = Guid.NewGuid().ToString();
      CompanyEmail = string.Empty;
      CompanyPhoneId = Guid.NewGuid().ToString();
      CompanyPhone = 0;
      CompanyExtension = 0;
      PrimaryContactId = string.Empty;
      NewPhoneNumber = true;
      NewEmailAddress = true;
    }
  }

  public class Architect : Company
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Architect(
      string id,
      string entityId,
      string name,
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
      CompanyId = id;
      EntityId = entityId;
      CompanyName = name;
      StreetAddress = streetAddress;
      City = city;
      State = state;
      PostalCode = postalCode;
      CompanyEmailId = emailAddressId;
      CompanyEmail = emailAddress;
      CompanyPhoneId = phoneNumberId;
      CompanyPhone = phoneNumber;
      CompanyExtension = extension;
      PrimaryContactId = primaryContactId;
      if (string.IsNullOrEmpty(PrimaryContactId))
      {
        PrimaryContactName = "Assign";
      }
      else
      {
        PrimaryContactName = primaryContactFirstName + " " + primaryContactLastName;
      }
    }

    public Architect()
    {
      CompanyId = Guid.NewGuid().ToString();
      EntityId = Guid.NewGuid().ToString();
      CompanyName = string.Empty;
      StreetAddress = string.Empty;
      City = string.Empty;
      State = string.Empty;
      PostalCode = string.Empty;
      CompanyEmailId = Guid.NewGuid().ToString();
      CompanyEmail = string.Empty;
      CompanyPhoneId = Guid.NewGuid().ToString();
      CompanyPhone = 0;
      CompanyExtension = 0;
      PrimaryContactId = string.Empty;
      NewPhoneNumber = true;
      NewEmailAddress = true;
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
        if (_FirstName != value && value.Length <= 31)
        {
          _FirstName = value;
          OnPropertyChanged(nameof(FirstName));
          _Modified = true;
          if (Company != null)
            Company.PrimaryContactName = _FirstName + " " + _LastName;
        }
      }
    }

    private string _LastName = string.Empty;
    public string LastName
    {
      get => _LastName;
      set
      {
        if (_LastName != value && value.Length <= 31)
        {
          _LastName = value;
          OnPropertyChanged(nameof(LastName));
          _Modified = true;
          if (Company != null)
            Company.PrimaryContactName = _FirstName + " " + _LastName;
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
        if (_EmailAddress != value && value.Length <= 63)
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

    private Company? _Company;
    public Company? Company
    {
      get => _Company;
      set => _Company = value;
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
    public Architect? SelectedArchitect { get; set; }

    public Contact? SelectedContact { get; set; }

    public ObservableCollection<Client> AllClients { get; set; }
    public ObservableCollection<Architect> AllArchitects { get; set; }

    public ObservableCollection<Contact> AllContacts { get; set; }

    public ClientsViewModel(LoginResponse loginResponse)
    {
      Database = new Database.Database(loginResponse.SqlConnectionString);

      AllClients = new ObservableCollection<Client>(Database.GetClients());
      AllArchitects = new ObservableCollection<Architect>(Database.GetArchitects());
      AllContacts = new ObservableCollection<Contact>(Database.GetContacts());
      foreach (Client client in AllClients)
      {
        foreach (Architect architect in AllArchitects)
        {
          if (client.CompanyId == architect.CompanyId)
          {
            client.IsArchitect = true;
            client.Architect = architect;
          }
        }
        client.New = false;
      }
      foreach (Architect architect in AllArchitects)
      {
        foreach (Client client in AllClients)
        {
          if (client.CompanyId == architect.CompanyId)
          {
            architect.IsClient = true;
            architect.Client = client;
          }
        }
        architect.New = false;
      }
      foreach (Contact contact in AllContacts)
      {
        foreach (Architect architect in AllArchitects)
        {
          if (contact.CompanyId == architect.CompanyId)
          {
            contact.Company = architect;
          }
        }
        foreach (Client client in AllClients)
        {
          if (contact.CompanyId == client.CompanyId)
          {
            contact.Company = client;
          }
        }
        contact.New = false;
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

      List<Architect> deletedArchitects = new List<Architect>();
      foreach (Architect architect in AllArchitects)
      {
        if (architect.Delete)
        {
          Database.DeleteArchitect(architect);
          deletedArchitects.Add(architect);
        }
        else if (architect.Modified)
        {
          Database.SaveArchitect(architect);
          architect.Modified = false;
        }
      }
      foreach (Architect architect in deletedArchitects)
      {
        AllArchitects.Remove(architect);
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

    public void FlagArchitectForDeletion()
    {
      if (SelectedArchitect != null)
      {
        SelectedArchitect.Delete = !SelectedArchitect.Delete;
      }
    }

    public void FlagContactForDeletion()
    {
      if (SelectedContact != null)
      {
        SelectedContact.Delete = !SelectedContact.Delete;
      }
    }

    public void SetPrimaryContact() { }

    public void AddClientToArchitects()
    {
      if (SelectedClient == null)
        return;
      Architect? architect = Database.GetArchitect(SelectedClient.CompanyId);
      if (architect == null)
      {
        architect = new Architect
        {
          CompanyId = SelectedClient.CompanyId,
          EntityId = SelectedClient.EntityId,
          CompanyName = SelectedClient.CompanyName,
          StreetAddress = SelectedClient.StreetAddress,
          City = SelectedClient.City,
          State = SelectedClient.State,
          PostalCode = SelectedClient.PostalCode,
          CompanyEmailId = SelectedClient.CompanyEmailId,
          CompanyEmail = SelectedClient.CompanyEmail,
          CompanyPhoneId = SelectedClient.CompanyPhoneId,
          CompanyPhone = SelectedClient.CompanyPhone,
          CompanyExtension = SelectedClient.CompanyExtension,
          PrimaryContactId = SelectedClient.PrimaryContactId,
          IsClient = true,
          Client = SelectedClient,
        };
      }
      AllArchitects.Add(architect);
    }

    public void RemoveClientFromArchitects()
    {
      if (SelectedClient == null)
        return;
      Architect? architect = AllArchitects.FirstOrDefault(a =>
        a.CompanyId == SelectedClient.CompanyId
      );
      if (architect != null)
        AllArchitects.Remove(architect);
    }

    public void AddArchitectToClients()
    {
      if (SelectedArchitect == null)
        return;
      Client? client = Database.GetClient(SelectedArchitect.CompanyId);
      if (client == null)
      {
        client = new Client
        {
          CompanyId = SelectedArchitect.CompanyId,
          EntityId = SelectedArchitect.EntityId,
          CompanyName = SelectedArchitect.CompanyName,
          StreetAddress = SelectedArchitect.StreetAddress,
          City = SelectedArchitect.City,
          State = SelectedArchitect.State,
          PostalCode = SelectedArchitect.PostalCode,
          CompanyEmailId = SelectedArchitect.CompanyEmailId,
          CompanyEmail = SelectedArchitect.CompanyEmail,
          CompanyPhoneId = SelectedArchitect.CompanyPhoneId,
          CompanyPhone = SelectedArchitect.CompanyPhone,
          CompanyExtension = SelectedArchitect.CompanyExtension,
          PrimaryContactId = SelectedArchitect.PrimaryContactId,
          IsArchitect = true,
          Architect = SelectedArchitect,
        };
      }
      AllClients.Add(client);
    }

    public void RemoveArchitectFromClients()
    {
      if (SelectedArchitect == null)
        return;
      Client? client = AllClients.FirstOrDefault(c => c.CompanyId == SelectedArchitect.CompanyId);
      if (client != null)
        AllClients.Remove(client);
    }
  }
}
