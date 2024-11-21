﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class Electrical
  {
    private List<ElectricalService> serviceList;
    private List<ElectricalPanel> panelList;
    private List<ElectricalEquipment> equipmentList;
    private DateTime LastSaved;
    private List<string> sqlActions;
    public Electrical()
    {
      serviceList = new List<ElectricalService>();
      panelList = new List<ElectricalPanel>();
      equipmentList = new List<ElectricalEquipment>();
      sqlActions = new List<string>();
    }
    public void AddService(ElectricalService electricalService)
    {
      if (!electricalService.Verify()) { return; }
      serviceList.Add(electricalService);
      sqlActions.Add($"INSERT INTO electrical_services VALUES('{electricalService.Id}', '{electricalService.Name}', '{electricalService.ProjectId}')");
    }
    public void UpdateService(int serviceIndex, ElectricalService updatedElectricalService)
    {
      if (!updatedElectricalService.Verify()) { return; }
      serviceList[serviceIndex] = updatedElectricalService;
    }
    public void RemoveService(int serviceIndex)
    {
      serviceList.RemoveAt(serviceIndex);
    }
    public void AddPanel(ElectricalPanel panel)
    {
      if (!panel.Verify()) { return; }
      panelList.Add(panel);
    }
    public void UpdatePanel(int panelIndex, ElectricalPanel updatedPanel)
    {
      if (!updatedPanel.Verify()) { return; }
      panelList[panelIndex] = updatedPanel;
    }
    public void RemovePanel(int panelIndex)
    {
      panelList.RemoveAt(panelIndex);
    }
    public void AddEquipment(ElectricalEquipment equipment)
    {
      if (!equipment.Verify()) { return; }
      equipmentList.Add(equipment);
    }
    public void UpdateEquipment(int equipmentIndex, ElectricalEquipment updatedEquipment)
    {
      if (!updatedEquipment.Verify()) { return; }
      equipmentList[equipmentIndex] = updatedEquipment;
    }
    public void RemoveEquipment(int equipmentIndex)
    {
      equipmentList.RemoveAt(equipmentIndex);
    }

    public void FlushActions()
    {
      foreach (string action in sqlActions)
      {
        // just printing the actions for now
        // later on, this will be a function to perform actions in sql
        Console.WriteLine(action);
      }
      sqlActions = new List<string>();
      LastSaved = DateTime.Now;
    }
  }
}
