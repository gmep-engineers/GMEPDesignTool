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
       
        public Database.Database database = new Database.Database();

        public ProjectControlViewModel(string projectNo)
        {
            projectIds = database.GetProjectIds(projectNo);
            selectedVersion = 1;
            _projectNo = projectNo;
        }

        private string saveText;
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

        private string _projectNo;
        public string ProjectNo
        {
            get { return _projectNo; }
            set
            {
                if (_projectNo != value)
                {
                    _projectNo = value;
                    OnPropertyChanged(nameof(ProjectNo));
                }
            }
        }

        public Dictionary<int, string> projectIds;
        public Dictionary<int, string> ProjectIds
        {
            get { return projectIds; }
            set
            {
                if (projectIds != value)
                {
                    projectIds = value;
                    OnPropertyChanged(nameof(ProjectIds));
                }
            }
        }

        public int selectedVersion;

        public int SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (selectedVersion != value)
                {
                    selectedVersion = value;
                    OnPropertyChanged(nameof(SelectedVersion));
                }
            }
        }

        private ElectricalProject activeElectricalProject;
        public ElectricalProject ActiveElectricalProject
        {
            get { return activeElectricalProject; }
            set
            {
                if (activeElectricalProject != value)
                {
                    activeElectricalProject = value;
                    OnPropertyChanged(nameof(ActiveElectricalProject));
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
