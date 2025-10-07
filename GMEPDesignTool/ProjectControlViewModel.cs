using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Mysqlx.Crud;

namespace GMEPDesignTool
{
  public class ProjectControlViewModel : INotifyPropertyChanged
  {
    public Database.Database database;

    public ProjectControlViewModel(string projectNo, LoginResponse loginResponse)
    {
      _projectNo = projectNo;
      database = new Database.Database(loginResponse.SqlConnectionString);
      if (loginResponse.AccessLevelId == 1)
      {
        ProjectVersionButtonsVisibility = Visibility.Visible;
      }
      //InitializeProjectControlViewModel(projectNo);
    }

    public async Task InitializeProjectControlViewModel()
    {
      projectIds = await database.GetProjectIds(ProjectNo);
      //database.SyncProjectDisciplineTable("electrical");
      electricalProjectIds = database.GetAllElectricalProjectVersionIds(ProjectNo);
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

    public List<string> electricalProjectIds;
    public List<string> ElectricalProjectIds
    {
      get { return electricalProjectIds; }
      set
      {
        if (electricalProjectIds != value)
        {
          electricalProjectIds = value;
          OnPropertyChanged(nameof(ElectricalProjectIds));
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

    private Visibility projectVersionButtonsVisibility = Visibility.Hidden;
    public Visibility ProjectVersionButtonsVisibility
    {
      get => projectVersionButtonsVisibility;
      set
      {
        if (projectVersionButtonsVisibility != value)
        {
          projectVersionButtonsVisibility = value;
          OnPropertyChanged(nameof(ProjectVersionButtonsVisibility));
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
