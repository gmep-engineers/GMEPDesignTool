using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class SelectProposalTypeViewModel : INotifyPropertyChanged
    {
        private int typeId;
        public int TypeId
        {
            get { return typeId; }
            set
            {
                if (typeId != value)
                {
                    typeId = value;
                    OnPropertyChanged(nameof(TypeId));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
