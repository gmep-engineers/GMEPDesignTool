using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace GMEPDesignTool
{
    public class PhoneNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value != null)
            {
                Trace.WriteLine(value.ToString());
                string str = value
                    .ToString()
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("-", "")
                    .Replace(" ", "");
                Trace.WriteLine(str);
                if (!long.TryParse(str, out long d))
                {
                    Trace.WriteLine("do nothing");
                    return Binding.DoNothing;
                }
                Trace.WriteLine("d");
                Trace.WriteLine(d);
                return d;
            }
            return null;
        }
    }

    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value != null)
            {
                string month;
                string day;
                string year;

                string str = value.ToString();
                string[] strArr = str.Split('-');
                if (str.Contains('/'))
                {
                    strArr = str.Split("/");
                }
                if (strArr.Length == 3)
                {
                    month = strArr[0];
                    day = strArr[1];
                    year = strArr[2];
                    if (month.Length == 1)
                    {
                        month = "0" + month;
                    }
                    if (day.Length == 1)
                    {
                        day = "0" + day;
                    }
                    if (year.Length == 2)
                    {
                        year = "20" + year;
                    }
                    if (!DateTime.TryParse(year + "-" + month + "-" + day, out DateTime parsedDate))
                    {
                        return Binding.DoNothing;
                    }
                    return parsedDate;
                }
                else
                {
                    return Binding.DoNothing;
                }
            }
            return null;
        }
    }

    public class Employee : INotifyPropertyChanged
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

        private string _ContactId = String.Empty;
        public string ContactId
        {
            get => _ContactId;
        }

        private string _EntityId = String.Empty;
        public string EntityId
        {
            get => _EntityId;
        }
        private string _Id = String.Empty;
        public string Id
        {
            get => _Id;
        }
        private string _LastName = String.Empty;
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
        private string _FirstName = String.Empty;
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
        private int _TitleId;
        public int TitleId
        {
            get => _TitleId;
            set
            {
                if (_TitleId != value)
                {
                    _TitleId = value;
                    OnPropertyChanged(nameof(TitleId));
                    _Modified = true;
                }
            }
        }
        private int _DepartmentId;
        public int DepartmentId
        {
            get => _DepartmentId;
            set
            {
                if (_DepartmentId != value)
                {
                    _DepartmentId = value;
                    OnPropertyChanged(nameof(DepartmentId));
                    _Modified = true;
                }
            }
        }

        private string _EmailAddressId = String.Empty;
        public string EmailAddressId
        {
            get => _EmailAddressId;
            set => _EmailAddressId = value;
        }

        private string _EmailAddress = String.Empty;
        public string EmailAddress
        {
            get => _EmailAddress;
            set
            {
                if (_EmailAddress != value)
                {
                    if (String.IsNullOrEmpty(_EmailAddress))
                    {
                        _NewEmailAddress = true;
                    }
                    _EmailAddress = value;
                    OnPropertyChanged(nameof(EmailAddress));
                    _Modified = true;
                }
            }
        }
        private string _PhoneNumberId = String.Empty;
        public string PhoneNumberId
        {
            get => _PhoneNumberId;
            set => _PhoneNumberId = value;
        }

        private ulong? _PhoneNumber = null;
        public ulong? PhoneNumber
        {
            get => _PhoneNumber;
            set
            {
                if (_PhoneNumber != value)
                {
                    if (_PhoneNumber == 0)
                    {
                        _NewPhoneNumber = true;
                    }
                    _PhoneNumber = value;
                    OnPropertyChanged(nameof(PhoneNumber));
                    _Modified = true;
                }
            }
        }
        private uint? _Extension = null;
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
        private DateTime? _HireDate = null;
        public DateTime? HireDate
        {
            get => _HireDate;
            set
            {
                if (_HireDate != value)
                {
                    _HireDate = value;
                    OnPropertyChanged(nameof(HireDate));
                    _Modified = true;
                }
            }
        }
        private DateTime? _TerminationDate = null;
        public DateTime? TerminationDate
        {
            get => _TerminationDate;
            set
            {
                if (_TerminationDate != value)
                {
                    _TerminationDate = value;
                    OnPropertyChanged(nameof(TerminationDate));
                    _Modified = true;
                }
            }
        }
        private string _Username = String.Empty;
        public string Username
        {
            get => _Username;
            set
            {
                if (_Username != value)
                {
                    _Username = value;
                    OnPropertyChanged(nameof(Username));
                    _Modified = true;
                }
            }
        }

        public Employee(
            string id,
            string contactId,
            string entityId,
            string lastName,
            string firstName,
            int titleId,
            int deparmentId,
            string emailAddress,
            string emailAddressId,
            ulong? phoneNumber,
            string phoneNumberId,
            uint? extension,
            DateTime? hireDate,
            DateTime? terminationDate,
            string username
        )
        {
            _Id = id;
            _ContactId = contactId;
            _EntityId = entityId;
            _LastName = lastName;
            _FirstName = firstName;
            _TitleId = titleId;
            _DepartmentId = deparmentId;
            _EmailAddress = emailAddress;
            _EmailAddressId = emailAddressId;
            _PhoneNumber = phoneNumber;
            _PhoneNumberId = phoneNumberId;
            _Extension = extension;
            _HireDate = hireDate;
            _TerminationDate = terminationDate;
            _Username = username;
        }
    }

    public enum Departments
    {
        Structural,
        Mechanical,
        Electrical,
        Plumbing,
        Administration,
    }

    public enum Titles
    {
        Engineer,
        Manager,
    }

    class EmployeesViewModel : ViewModelBase
    {
        public List<Employee> Employees { get; set; }
        public Database.Database Database { get; set; }

        public Employee? SelectedEmployee { get; set; }

        public EmployeesViewModel(LoginResponse loginResponse)
        {
            Database = new Database.Database(loginResponse.SqlConnectionString);
            Employees = Database.GetEmployees();
        }

        public void OpenUpdatePasswordWindow()
        {
            if (SelectedEmployee != null)
            {
                UpdatePasswordWindow updatePasswordWindow = new UpdatePasswordWindow(
                    Database,
                    SelectedEmployee.Id
                );
                updatePasswordWindow.ShowDialog();
            }
        }

        public void Save()
        {
            foreach (Employee employee in Employees)
            {
                if (employee.Modified)
                {
                    Database.SaveEmployee(employee);
                    employee.Modified = false;
                }
            }
        }
    }
}
