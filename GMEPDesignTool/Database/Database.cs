using System;
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

        public Dictionary<string, string> getOwners()
        {
            var owners = new Dictionary<string, string>();

            try
            {
                OpenConnection();

                string query = "SELECT id, name FROM owners";

                using (MySqlCommand cmd = new MySqlCommand(query, Connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader.GetString("id");
                            string name = reader.GetString("name");
                            owners.Add(id, name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return owners;
        }

        //Update Project Functions
        public void UpdateProject(
            string projectId,
            ObservableCollection<ElectricalService> services,
            ObservableCollection<ElectricalPanel> panels,
            ObservableCollection<ElectricalEquipment> equipments,
            ObservableCollection<ElectricalTransformer> transformers
        )
        {
            OpenConnection();

            UpdateServices(projectId, services);
            UpdatePanels(projectId, panels);
            UpdateEquipments(projectId, equipments);
            UpdateTransformers(projectId, transformers);

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

        private void UpdateTransformers(
            string projectId,
            ObservableCollection<ElectricalTransformer> transformers
        )
        {
            var existingTransformerIds = GetExistingIds(
                "electrical_transformers",
                "project_id",
                projectId
            );

            foreach (var transformer in transformers)
            {
                if (existingTransformerIds.Contains(transformer.Id))
                {
                    UpdateTransformer(transformer);
                    existingTransformerIds.Remove(transformer.Id);
                }
                else
                {
                    InsertTransformer(projectId, transformer);
                }
            }

            DeleteRemovedItems("electrical_transformers", existingTransformerIds);
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
                "UPDATE electrical_services SET name = @name, electrical_service_amp_rating_id = @amp, electrical_service_voltage_id = @type, electrical_service_meter_config_id = @config, color_code = @color_code WHERE id = @id";
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
                "INSERT INTO electrical_services (id, project_id, name, electrical_service_amp_rating_id, electrical_service_voltage_id, electrical_service_meter_config_id, color_code) VALUES (@id, @projectId, @name, @amp, @type, @config, @color_code)";
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
                "UPDATE electrical_panels SET bus_amp_rating_id = @bus, main_amp_rating_id = @main, is_distribution = @is_distribution, voltage_id = @type, num_breakers = @numBreakers, parent_distance = @distanceFromParent, aic_rating = @aicRating, name = @name, color_code = @color_code, parent_id = @parent_id WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@parent_id", panel.FedFromId);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@aicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.Parameters.AddWithValue("@type", panel.Type);
            command.ExecuteNonQuery();
        }

        private void InsertPanel(string projectId, ElectricalPanel panel)
        {
            string query =
                "INSERT INTO electrical_panels (id, project_id, bus_amp_rating_id, main_amp_rating_id, is_distribution, name, color_code, parent_id, num_breakers, parent_distance, aic_rating, voltage_id) VALUES (@id, @projectId, @bus, @main, @is_distribution, @name, @color_code, @parent_id, @numBreakers, @distanceFromParent, @AicRating, @type)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@parent_id", panel.FedFromId);
            command.Parameters.AddWithValue("@AicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.Parameters.AddWithValue("@type", panel.Type);
            command.ExecuteNonQuery();
        }

        private void UpdateEquipment(ElectricalEquipment equipment)
        {
            string query =
                "UPDATE electrical_equipment SET equip_no = @equip_no, parent_id = @parent_id, owner_id = @owner, voltage_id = @voltage, amp = @amp, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, parent_distance=@distanceFromParent, category_id=@category, color_code = @color_code, mounting_type_id = @mounting WHERE group_id = @group_id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
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
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@mounting", equipment.Mounting);
            command.ExecuteNonQuery();
        }

        private void InsertEquipment(string projectId, ElectricalEquipment equipment)
        {
            string query =
                "INSERT INTO electrical_equipment (id, group_id, project_id, equip_no, parent_id, owner_id, voltage_id, amp, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client, parent_distance, category_id, color_code, mounting_type_id) VALUES (@id, @group_id, @projectId, @equip_no, @parent_id, @owner, @voltage, @amp, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @distanceFromParent, @category, @color_code, @mounting)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@group_id", equipment.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
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
            command.Parameters.AddWithValue("@mounting", equipment.Mounting);
            command.ExecuteNonQuery();
        }

        private void UpdateTransformer(ElectricalTransformer transformer)
        {
            string query =
                "UPDATE electrical_transformers SET parent_id = @parent_id, voltage_id = @voltage, project_id = @project_id, kva_id = @kva, parent_distance = @distanceFromParent, color_code = @color_code, name = @name WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@project_id", transformer.ProjectId);
            command.Parameters.AddWithValue("@voltage", transformer.Voltage);
            command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
            command.Parameters.AddWithValue("@kva", transformer.Kva);
            command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
            command.Parameters.AddWithValue("@name", transformer.Name);
            command.ExecuteNonQuery();
        }

        private void InsertTransformer(string projectId, ElectricalTransformer transformer)
        {
            string query =
                "INSERT INTO electrical_transformers (id, project_id, parent_id, voltage_id, parent_distance, color_code, kva_id, name) VALUES (@id, @project_id, @parent_id, @voltage, @distanceFromParent, @color_code, @kva, @name)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@project_id", transformer.ProjectId);
            command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
            command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
            command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
            command.Parameters.AddWithValue("@kva", transformer.Kva);
            command.Parameters.AddWithValue("@name", transformer.Name);
            command.Parameters.AddWithValue("@voltage", transformer.Voltage);
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
                        reader.GetInt32("electrical_service_voltage_id"),
                        reader.GetInt32("electrical_service_amp_rating_id"),
                        reader.GetInt32("electrical_service_meter_config_id"),
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
                        reader.GetInt32("bus_amp_rating_id"),
                        reader.GetInt32("main_amp_rating_id"),
                        false,
                        reader.GetBoolean("is_distribution"),
                        reader.GetString("name"),
                        reader.GetString("color_code"),
                        reader.GetString("parent_id"),
                        reader.GetInt32("num_breakers"),
                        reader.GetInt32("parent_distance"),
                        reader.GetInt32("aic_rating"),
                        0,
                        0,
                        reader.GetInt32("voltage_id"),
                        false
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
                        reader.GetString("parent_id"),
                        reader.GetInt32("voltage_id"),
                        reader.GetFloat("amp"),
                        idToVoltage(reader.GetInt32("voltage_id")) * reader.GetFloat("amp"),
                        reader.GetBoolean("is_three_phase"),
                        reader.GetString("spec_sheet_id"),
                        reader.GetInt32("aic_rating"),
                        reader.GetBoolean("spec_sheet_from_client"),
                        reader.GetInt32("parent_distance"),
                        reader.GetInt32("category_id"),
                        reader.GetString("color_code"),
                        false,
                        reader.GetInt32("mounting_type_id")
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

            float idToVoltage(int voltageId)
            {
                int voltage = 0;
                switch (voltageId)
                {
                    case (1):
                        voltage = 115;
                        break;
                    case (2):
                        voltage = 120;
                        break;
                    case (3):
                        voltage = 208;
                        break;
                    case (4):
                        voltage = 230;
                        break;
                    case (5):
                        voltage = 240;
                        break;
                    case (6):
                        voltage = 277;
                        break;
                    case (7):
                        voltage = 460;
                        break;
                    case (8):
                        voltage = 480;
                        break;
                }
                return voltage;
            }
        }

        public ObservableCollection<ElectricalTransformer> GetProjectTransformers(string projectId)
        {
            ObservableCollection<ElectricalTransformer> transformers =
                new ObservableCollection<ElectricalTransformer>();
            string query = "SELECT * FROM electrical_transformers WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                transformers.Add(
                    new ElectricalTransformer(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("parent_id"),
                        reader.GetInt32("parent_distance"),
                        reader.GetString("color_code"),
                        reader.GetInt32("voltage_id"),
                        reader.GetString("name"),
                        reader.GetInt32("kva_id"),
                        false
                    )
                );
            }
            reader.Close();
            CloseConnection();
            return transformers;
        }
    }
}
