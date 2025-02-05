using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace GMEPDesignTool
{
    public class ProjectControlViewModel : ViewModelBase
    {
        public string saveText;
        public string SaveText
        {
            get => saveText;
            set
            {
                if (saveText != value)
                {
                    saveText = value;
                    SetProperty(ref saveText, value);
                }
            }
        }

        public ProjectControlViewModel()
        {
            SaveText = "";
        }
    }
}
