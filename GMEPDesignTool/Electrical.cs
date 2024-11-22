using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class Electrical
  {
    public List<ElectricalService> ServiceList;
    public List<ElectricalPanel> PanelList;
    public List<ElectricalEquipment> EquipmentList;
    public DateTime LastSaved;
    public List<string> sqlActions;
    public Electrical()
    {
      ServiceList = new List<ElectricalService>();
      PanelList = new List<ElectricalPanel>();
      EquipmentList = new List<ElectricalEquipment>();
      sqlActions = new List<string>();
    }
    public void AddService(ElectricalService electricalService)
    {
      if (!electricalService.Verify()) { return; }
      ServiceList.Add(electricalService);
      sqlActions.Add($"INSERT INTO electrical_services VALUES('{electricalService.Id}', '{electricalService.Name}', '{electricalService.ProjectId}')");
    }
    public void UpdateService(int serviceIndex, ElectricalService updatedElectricalService)
    {
      if (!updatedElectricalService.Verify()) { return; }
      ServiceList[serviceIndex] = updatedElectricalService;
    }
    public void RemoveService(int serviceIndex)
    {
      ServiceList.RemoveAt(serviceIndex);
    }
    public void AddPanel(ElectricalPanel panel)
    {
      if (!panel.Verify()) { return; }
      PanelList.Add(panel);
    }
    public void UpdatePanel(int panelIndex, ElectricalPanel updatedPanel)
    {
      if (!updatedPanel.Verify()) { return; }
      PanelList[panelIndex] = updatedPanel;
    }
    public void RemovePanel(int panelIndex)
    {
      PanelList.RemoveAt(panelIndex);
    }
    public void AddEquipment(ElectricalEquipment equipment)
    {
      if (!equipment.Verify()) { return; }
      EquipmentList.Add(equipment);
    }
    public void UpdateEquipment(int equipmentIndex, ElectricalEquipment updatedEquipment)
    {
      if (!updatedEquipment.Verify()) { return; }
      EquipmentList[equipmentIndex] = updatedEquipment;
    }
    public void RemoveEquipment(int equipmentIndex)
    {
      EquipmentList.RemoveAt(equipmentIndex);
    }

    public void FlushActions()
    {
      foreach (string action in sqlActions)
      {
        // just printing the actions for now
        // later on, this will be a function to perform actions in sql
        Trace.WriteLine(action);
      }
      sqlActions = new List<string>();
      LastSaved = DateTime.Now;
    }
  }
}
