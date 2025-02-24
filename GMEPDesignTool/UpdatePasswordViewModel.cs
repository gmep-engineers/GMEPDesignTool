using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    class UpdatePasswordViewModel : ViewModelBase
    {
        private string _Password = String.Empty;
        public string Password
        {
            get => _Password;
            set => _Password = value;
        }

        private Database.Database Database { get; set; }

        private string EmployeeId { get; set; }

        public UpdatePasswordViewModel(Database.Database database, string employeeId)
        {
            EmployeeId = employeeId;
            Database = database;
        }

        public void SetPassword()
        {
            Database.SetEmployeePassword(EmployeeId, Password);
        }
    }
}
