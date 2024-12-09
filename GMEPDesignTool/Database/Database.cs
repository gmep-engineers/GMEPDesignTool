﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace GMEPDesignTool.Database
{
    public class Database
    {
        public string ConnectionString { get; set; }
        public MySqlConnection Connection { get; set; }

        public Database()
        {
            ConnectionString = Properties.Settings.Default.ConnectionString;
            Connection = new MySqlConnection(ConnectionString);
        }

        public void OpenConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Closed)
            {
                Connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Open)
            {
                Connection.Close();
            }
        }

        public string GetProjectId(string projectNo)
        {
            string query = "SELECT id FROM projects WHERE gmep_project_no = @projectNo";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            MySqlDataReader reader = command.ExecuteReader();

            string id = null;
            if (reader.Read())
            {
                id = reader.GetString("id");
            }
            reader.Close();

            if (id == null)
            {
                // Project name does not exist, insert a new entry with a generated ID
                id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                insertCommand.ExecuteNonQuery();
            }

            CloseConnection();
            return id;
        }

        //Update Project Functions
        public void UpdateProject(
            string projectId,
            ObservableCollection<ElectricalService> services,
            ObservableCollection<ElectricalPanel> panels,
            ObservableCollection<ElectricalEquipment> equipments
        )
        {
            OpenConnection();

            UpdateServices(projectId, services);
            UpdatePanels(projectId, panels);
            UpdateEquipments(projectId, equipments);

            CloseConnection();
        }

        private void UpdateServices(
            string projectId,
            ObservableCollection<ElectricalService> services
        )
        {
            var existingServiceIds = GetExistingIds("electrical_services", "project_id", projectId);

            foreach (var service in services)
            {
                if (existingServiceIds.Contains(service.Id))
                {
                    UpdateService(service);
                    existingServiceIds.Remove(service.Id);
                }
                else
                {
                    InsertService(projectId, service);
                }
            }

            DeleteRemovedItems("electrical_services", existingServiceIds);
        }

        private void UpdatePanels(string projectId, ObservableCollection<ElectricalPanel> panels)
        {
            var existingPanelIds = GetExistingIds("electrical_panels", "project_id", projectId);

            foreach (var panel in panels)
            {
                if (existingPanelIds.Contains(panel.Id))
                {
                    UpdatePanel(panel);
                    existingPanelIds.Remove(panel.Id);
                }
                else
                {
                    InsertPanel(projectId, panel);
                }
            }

            DeleteRemovedItems("electrical_panels", existingPanelIds);
        }

        private void UpdateEquipments(
            string projectId,
            ObservableCollection<ElectricalEquipment> equipments
        )
        {
            var existingEquipmentIds = GetExistingIds(
                "electrical_equipment",
                "project_id",
                projectId
            );

            foreach (var equipment in equipments)
            {
                if (existingEquipmentIds.Contains(equipment.Id))
                {
                    SyncEquipmentQuantity(equipment);
                    UpdateEquipment(equipment);
                    existingEquipmentIds.Remove(equipment.Id);
                }
                else
                {
                    InsertEquipment(projectId, equipment);
                }
            }

            DeleteRemovedItems("electrical_equipment", existingEquipmentIds);
        }

        public void SyncEquipmentQuantity(ElectricalEquipment equipment)
        {
            // Get the current count of entries with the same group_id
            string countQuery =
                "SELECT COUNT(*) FROM electrical_equipment WHERE group_id = @groupId";
            MySqlCommand countCommand = new MySqlCommand(countQuery, Connection);
            countCommand.Parameters.AddWithValue("@groupId", equipment.Id);
            int currentCount = Convert.ToInt32(countCommand.ExecuteScalar());

            // If the current count is less than the qty, add new entries
            if (currentCount < equipment.Qty)
            {
                int entriesToAdd = equipment.Qty - currentCount;
                for (int i = 0; i < entriesToAdd; i++)
                {
                    InsertEquipment(equipment.ProjectId, equipment);
                }
            }
            // If the current count is more than the qty, remove excess entries
            else if (currentCount > equipment.Qty)
            {
                int entriesToRemove = currentCount - equipment.Qty;
                string deleteQuery =
                    "DELETE FROM electrical_equipment WHERE group_id = @groupId LIMIT @limit";
                MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, Connection);
                deleteCommand.Parameters.AddWithValue("@groupId", equipment.Id);
                deleteCommand.Parameters.AddWithValue("@limit", entriesToRemove);
                deleteCommand.ExecuteNonQuery();
            }
        }

        private HashSet<string> GetExistingIds(
            string tableName,
            string columnName,
            string projectId
        )
        {
            var idType = "";
            if (tableName == "electrical_equipment")
            {
                idType = "group_id";
            }
            else
            {
                idType = "id";
            }
            string query = $"SELECT {idType} FROM {tableName} WHERE {columnName} = @projectId";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            HashSet<string> ids = new HashSet<string>();
            while (reader.Read())
            {
                ids.Add(reader.GetString($"{idType}"));
            }
            reader.Close();
            return ids;
        }

        private void UpdateService(ElectricalService service)
        {
            string query =
                "UPDATE electrical_services SET name = @name, electrical_service_amp = @amp, electrical_service_type = @type, electrical_service_meter_config = @config, color_code = @color_code WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@name", service.Name);
            command.Parameters.AddWithValue("@amp", service.Amp);
            command.Parameters.AddWithValue("@id", service.Id);
            command.Parameters.AddWithValue("@type", service.Type);
            command.Parameters.AddWithValue("@config", service.Config);
            command.Parameters.AddWithValue("@color_code", service.ColorCode);
            command.ExecuteNonQuery();
        }

        private void InsertService(string projectId, ElectricalService service)
        {
            string query =
                "INSERT INTO electrical_services (id, project_id, name, electrical_service_amp, electrical_service_type, electrical_service_meter_config, color_code) VALUES (@id, @projectId, @name, @amp, @type, @config, @color_code)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", service.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@name", service.Name);
            command.Parameters.AddWithValue("@amp", service.Amp);
            command.Parameters.AddWithValue("@type", service.Type);
            command.Parameters.AddWithValue("@config", service.Config);
            command.Parameters.AddWithValue("@color_code", service.ColorCode);
            command.ExecuteNonQuery();
        }

        private void UpdatePanel(ElectricalPanel panel)
        {
            string query =
                "UPDATE electrical_panels SET bus = @bus, main = @main, is_distribution = @is_distribution, num_breakers = @numBreakers, distance_from_parent = @distanceFromParent, aic_rating = @aicRating, name = @name, color_code = @color_code, fed_from_id = @fed_from_id WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@fed_from_id", panel.FedFromId);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@aicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.ExecuteNonQuery();
        }

        private void InsertPanel(string projectId, ElectricalPanel panel)
        {
            string query =
                "INSERT INTO electrical_panels (id, project_id, bus, main, is_distribution, name, color_code, fed_from_id, num_breakers, distance_from_parent, aic_rating) VALUES (@id, @projectId, @bus, @main, @is_distribution, @name, @color_code, @fed_from_id, @numBreakers, @distanceFromParent, @AicRating)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@fed_from_id", panel.FedFromId);
            command.Parameters.AddWithValue("@AicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.ExecuteNonQuery();
        }

        private void UpdateEquipment(ElectricalEquipment equipment)
        {
            string query =
                "UPDATE electrical_equipment SET owner_id = @owner, equip_no = @equip_no, panel_id = @panel_id, voltage = @voltage, amp = @amp, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, distance_from_parent=@distanceFromParent, category=@category, color_code = @color_code WHERE group_id = @group_id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@panel_id", equipment.PanelId);
            command.Parameters.AddWithValue("@voltage", equipment.Voltage);
            command.Parameters.AddWithValue("@amp", equipment.Amp);
            command.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
            command.Parameters.AddWithValue("@spec_sheet_id", equipment.SpecSheetId);
            command.Parameters.AddWithValue("@aic_rating", equipment.AicRating);
            command.Parameters.AddWithValue(
                "@spec_sheet_from_client",
                equipment.SpecSheetFromClient
            );
            command.Parameters.AddWithValue("@distanceFromParent", equipment.DistanceFromParent);
            command.Parameters.AddWithValue("@category", equipment.Category);
            command.Parameters.AddWithValue("@color_code", equipment.ColorCode);
            command.Parameters.AddWithValue("@group_id", equipment.Id);
            command.ExecuteNonQuery();
        }

        private void InsertEquipment(string projectId, ElectricalEquipment equipment)
        {
            string query =
                "INSERT INTO electrical_equipment (id, group_id, project_id, owner_id, equip_no, panel_id, voltage, amp, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client, distance_from_parent, category, color_code) VALUES (@id, @group_id, @projectId, @owner, @equip_no, @panel_id, @voltage, @amp, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @distanceFromParent, @category, @color_code)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@group_id", equipment.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@panel_id", equipment.PanelId);
            command.Parameters.AddWithValue("@voltage", equipment.Voltage);
            command.Parameters.AddWithValue("@amp", equipment.Amp);
            command.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
            command.Parameters.AddWithValue("@spec_sheet_id", equipment.SpecSheetId);
            command.Parameters.AddWithValue("@aic_rating", equipment.AicRating);
            command.Parameters.AddWithValue(
                "@spec_sheet_from_client",
                equipment.SpecSheetFromClient
            );
            command.Parameters.AddWithValue("@distanceFromParent", equipment.DistanceFromParent);
            command.Parameters.AddWithValue("@category", equipment.Category);
            command.Parameters.AddWithValue("@color_code", equipment.ColorCode);
            command.ExecuteNonQuery();
        }

        private void DeleteRemovedItems(string tableName, HashSet<string> ids)
        {
            var idType = "";
            if (tableName == "electrical_equipment")
            {
                idType = "group_id";
            }
            else
            {
                idType = "id";
            }
            foreach (var id in ids)
            {
                string query = $"DELETE FROM {tableName} WHERE {idType} = @id";
                MySqlCommand command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        public ObservableCollection<ElectricalService> GetProjectServices(string projectId)
        {
            ObservableCollection<ElectricalService> services =
                new ObservableCollection<ElectricalService>();
            string query = "SELECT * FROM electrical_services WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                services.Add(
                    new ElectricalService(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("name"),
                        reader.GetInt32("electrical_service_type"),
                        reader.GetInt32("electrical_service_amp"),
                        reader.GetString("electrical_service_meter_config"),
                        reader.GetString("color_code")
                    )
                );
            }
            reader.Close();
            CloseConnection();
            return services;
        }

        public ObservableCollection<ElectricalPanel> GetProjectPanels(string projectId)
        {
            ObservableCollection<ElectricalPanel> panels =
                new ObservableCollection<ElectricalPanel>();
            string query = "SELECT * FROM electrical_panels WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                panels.Add(
                    new ElectricalPanel(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetInt32("bus"),
                        reader.GetInt32("main"),
                        false,
                        reader.GetBoolean("is_distribution"),
                        reader.GetString("name"),
                        reader.GetString("color_code"),
                        reader.GetString("fed_from_id"),
                        reader.GetInt32("num_breakers"),
                        reader.GetInt32("distance_from_parent"),
                        reader.GetInt32("aic_rating"),
                        0,
                        0
                    )
                );
            }
            reader.Close();
            CloseConnection();
            return panels;
        }

        public ObservableCollection<ElectricalEquipment> GetProjectEquipment(string projectId)
        {
            ObservableCollection<ElectricalEquipment> equipments =
                new ObservableCollection<ElectricalEquipment>();
            string query = "SELECT * FROM electrical_equipment WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            Dictionary<string, ElectricalEquipment> equipmentDict =
                new Dictionary<string, ElectricalEquipment>();
            Dictionary<string, int> qtyDict = new Dictionary<string, int>();

            while (reader.Read())
            {
                string groupId = reader.GetString("group_id");
                if (!equipmentDict.ContainsKey(groupId))
                {
                    var newEquip = new ElectricalEquipment(
                        groupId,
                        reader.GetString("project_id"),
                        reader.GetString("owner_id"),
                        reader.GetString("equip_no"),
                        0,
                        reader.GetString("panel_id"),
                        reader.GetInt32("voltage"),
                        reader.GetFloat("amp"),
                        reader.GetInt32("voltage") * reader.GetFloat("amp"),
                        reader.GetBoolean("is_three_phase"),
                        reader.GetString("spec_sheet_id"),
                        reader.GetInt32("aic_rating"),
                        reader.GetBoolean("spec_sheet_from_client"),
                        reader.GetInt32("distance_from_parent"),
                        reader.GetString("category"),
                        reader.GetString("color_code")
                    );
                    equipmentDict[groupId] = newEquip;
                    qtyDict[groupId] = 0;
                }
                qtyDict[groupId]++;
            }

            foreach (var pair in equipmentDict)
            {
                pair.Value.Qty = qtyDict[pair.Key];
                equipments.Add(pair.Value);
            }

            reader.Close();
            CloseConnection();
            return equipments;
        }
    }
}
