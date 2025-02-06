using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace GMEPDesignTool
{
    public class ProjectControlViewModel : INotifyPropertyChanged
    {
        private string saveText;
        public Database.Database database = new Database.Database();
        public string SaveText
        {
            get { return saveText; }
            set
            {
                if (saveText != value)
                {
                    saveText = value;
                    OnPropertyChanged(nameof(SaveText));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
