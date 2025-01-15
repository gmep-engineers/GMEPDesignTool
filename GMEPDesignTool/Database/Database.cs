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
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics;
using BCrypt.Net;

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
        public bool LoginUser(string userName, string password)
        {
            if (userName == "" || password == "")
            {
                return false;
            }
                string query = @"
            SELECT e.password
            FROM email_addresses ea
            JOIN employees e ON e.email_id = ea.id
            WHERE ea.email_address = @employeeEmail";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@employeeEmail", userName + "@gmepe.com");
            MySqlDataReader reader = command.ExecuteReader();
            

            string hashedPassword = "";
            if (reader.Read())
            {
                hashedPassword = reader.GetString("password");
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);

            }
            CloseConnection();
            return true;
            


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
            ObservableCollection<ElectricalTransformer> transformers,
            ObservableCollection<ElectricalLighting> lightings
        )
        {
            OpenConnection();

            UpdateServices(projectId, services);
            UpdatePanels(projectId, panels);
            UpdateEquipments(projectId, equipments);
            UpdateTransformers(projectId, transformers);
            UpdateLightings(projectId, lightings);

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

        private void UpdateLightings(
            string projectId,
            ObservableCollection<ElectricalLighting> lightings
        )
        {
            var existingLightingIds = GetExistingIds(
                "electrical_lighting",
                "project_id",
                projectId
            );

            foreach (var lighting in lightings)
            {
                if (existingLightingIds.Contains(lighting.Id))
                {
                    UpdateLighting(lighting);
                    existingLightingIds.Remove(lighting.Id);
                }
                else
                {
                    InsertLighting(projectId, lighting);
                }
            }

            DeleteRemovedItems("electrical_lighting", existingLightingIds);
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
                "UPDATE electrical_panels SET bus_amp_rating_id = @bus, main_amp_rating_id = @main, is_distribution = @is_distribution, voltage_id = @type, num_breakers = @numBreakers, parent_distance = @distanceFromParent, aic_rating = @aicRating, name = @name, color_code = @color_code, parent_id = @parent_id, is_recessed = @is_recessed WHERE id = @id";
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
            command.Parameters.AddWithValue("@is_recessed", panel.IsRecessed);
            command.ExecuteNonQuery();
        }

        private void InsertPanel(string projectId, ElectricalPanel panel)
        {
            string query =
                "INSERT INTO electrical_panels (id, project_id, bus_amp_rating_id, main_amp_rating_id, is_distribution, name, color_code, parent_id, num_breakers, parent_distance, aic_rating, voltage_id, is_recessed) VALUES (@id, @projectId, @bus, @main, @is_distribution, @name, @color_code, @parent_id, @numBreakers, @distanceFromParent, @AicRating, @type, @is_recessed)";
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
            command.Parameters.AddWithValue("@is_recessed", panel.IsRecessed);
            command.ExecuteNonQuery();
        }

        private void UpdateEquipment(ElectricalEquipment equipment)
        {
            string query =
                "UPDATE electrical_equipment SET description = @description, equip_no = @equip_no, parent_id = @parent_id, owner_id = @owner, voltage_id = @voltage, fla = @fla, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, parent_distance=@distanceFromParent, category_id=@category, color_code = @color_code, mounting_type_id = @mounting, mca_id = @mca_id, hp = @hp, has_plug = @has_plug, locking_connector = @locking_connector WHERE group_id = @group_id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
            command.Parameters.AddWithValue("@voltage", equipment.Voltage);
            command.Parameters.AddWithValue("@fla", equipment.Fla);
            command.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
            command.Parameters.AddWithValue("@spec_sheet_id", equipment.SpecSheetId);
            command.Parameters.AddWithValue("@aic_rating", equipment.AicRating);
            command.Parameters.AddWithValue("@description", equipment.Description);
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
            command.Parameters.AddWithValue("@mca_id", equipment.McaId);
            command.Parameters.AddWithValue("@hp", equipment.Hp);
            command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
            command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
            command.ExecuteNonQuery();
        }

        private void InsertEquipment(string projectId, ElectricalEquipment equipment)
        {
            string query =
                "INSERT INTO electrical_equipment (id, group_id, project_id, equip_no, parent_id, owner_id, voltage_id, fla, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client, parent_distance, category_id, color_code, mounting_type_id, description, mca_id, hp, has_plug, locking_connector) VALUES (@id, @group_id, @projectId, @equip_no, @parent_id, @owner, @voltage, @fla, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @distanceFromParent, @category, @color_code, @mounting, @description, @mca_id, @hp, @has_plug, @locking_connector)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@group_id", equipment.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
            command.Parameters.AddWithValue("@voltage", equipment.Voltage);
            command.Parameters.AddWithValue("@fla", equipment.Fla);
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
            command.Parameters.AddWithValue("@description", equipment.Description);
            command.Parameters.AddWithValue("@mca_id", equipment.McaId);
            command.Parameters.AddWithValue("@hp", equipment.Hp);
            command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
            command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
            command.ExecuteNonQuery();
        }

        private void UpdateLighting(ElectricalLighting lighting)
        {
            string query =
                "UPDATE electrical_lighting SET notes = @notes, model_no = @model_no, parent_id = @parent_id, voltage_id = @voltageId, color_code = @colorCode, mounting_type_id = @mountingType, occupancy=@occupancy, manufacturer_id = @manufacturer, wattage = @wattage, em_capable = @em_capable, tag = @tag, symbol_id = @symbolId, description=@description, control_type_id = @controlTypeId, spec_sheet_from_client=@specFromClient, spec_sheet_id=@specSheetId, qty = @qty WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@model_no", lighting.ModelNo);
            command.Parameters.AddWithValue("@parent_id", lighting.ParentId);
            command.Parameters.AddWithValue("@id", lighting.Id);
            command.Parameters.AddWithValue("@manufacturer", lighting.Manufacturer);
            command.Parameters.AddWithValue("@occupancy", lighting.Occupancy);
            command.Parameters.AddWithValue("@wattage", lighting.Wattage);
            command.Parameters.AddWithValue("@em_capable", lighting.EmCapable);
            command.Parameters.AddWithValue("@mountingType", lighting.MountingType);
            command.Parameters.AddWithValue("@tag", lighting.Tag);
            command.Parameters.AddWithValue("@notes", lighting.Notes);
            command.Parameters.AddWithValue("@voltageId", lighting.VoltageId);
            command.Parameters.AddWithValue("@symbolId", lighting.SymbolId);
            command.Parameters.AddWithValue("@colorCode", lighting.colorCode);
            command.Parameters.AddWithValue("@description", lighting.Description);
            command.Parameters.AddWithValue("@controlTypeId", lighting.ControlTypeId);
            command.Parameters.AddWithValue("@specFromClient", lighting.SpecSheetFromClient);
            command.Parameters.AddWithValue("@specSheetId", lighting.SpecSheetId);
            command.Parameters.AddWithValue("@qty", lighting.Qty);

            command.ExecuteNonQuery();
        }

        private void InsertLighting(string projectId, ElectricalLighting lighting)
        {
            string query =
                "INSERT INTO electrical_lighting (id, project_id, notes, model_no, parent_id, voltage_id, color_code, mounting_type_id, occupancy, manufacturer_id, wattage, em_capable, tag, symbol_id, description, control_type_id, spec_sheet_from_client, spec_sheet_id, qty) VALUES (@id, @project_id, @notes, @model_no, @parent_id, @voltageId, @colorCode, @mountingType, @occupancy, @manufacturer, @wattage, @em_capable, @tag, @symbolId, @description, @controlTypeId, @specFromClient, @specSheetId, @qty)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", lighting.Id);
            command.Parameters.AddWithValue("@project_id", projectId);
            command.Parameters.AddWithValue("@model_no", lighting.ModelNo);
            command.Parameters.AddWithValue("@parent_id", lighting.ParentId);
            command.Parameters.AddWithValue("@manufacturer", lighting.Manufacturer);
            command.Parameters.AddWithValue("@occupancy", lighting.Occupancy);
            command.Parameters.AddWithValue("@wattage", lighting.Wattage);
            command.Parameters.AddWithValue("@em_capable", lighting.EmCapable);
            command.Parameters.AddWithValue("@mountingType", lighting.MountingType);
            command.Parameters.AddWithValue("@tag", lighting.Tag);
            command.Parameters.AddWithValue("@notes", lighting.Notes);
            command.Parameters.AddWithValue("@voltageId", lighting.VoltageId);
            command.Parameters.AddWithValue("@symbolId", lighting.SymbolId);
            command.Parameters.AddWithValue("@colorCode", lighting.colorCode);
            command.Parameters.AddWithValue("@description", lighting.Description);
            command.Parameters.AddWithValue("@controlTypeId", lighting.ControlTypeId);
            command.Parameters.AddWithValue("@specFromClient", lighting.SpecSheetFromClient);
            command.Parameters.AddWithValue("@specSheetId", lighting.SpecSheetId);
            command.Parameters.AddWithValue("@qty", lighting.Qty);
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
                        false,
                        reader.GetBoolean("is_recessed")
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
                        reader.GetFloat("fla"),
                        idToVoltage(reader.GetInt32("voltage_id")) * reader.GetFloat("fla"),
                        reader.GetBoolean("is_three_phase"),
                        reader.GetString("spec_sheet_id"),
                        reader.GetInt32("aic_rating"),
                        reader.GetBoolean("spec_sheet_from_client"),
                        reader.GetInt32("parent_distance"),
                        reader.GetInt32("category_id"),
                        reader.GetString("color_code"),
                        false,
                        reader.GetInt32("mounting_type_id"),
                        reader.GetString("description"),
                        reader.GetInt32("mca_id"),
                        reader.GetString("hp"),
                        reader.GetBoolean("has_plug"),
                        reader.GetBoolean("locking_connector")
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

        public ObservableCollection<ElectricalLighting> GetProjectLighting(string projectId)
        {
            ObservableCollection<ElectricalLighting> lightings =
                new ObservableCollection<ElectricalLighting>();
            string query = "SELECT * FROM electrical_lighting WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                lightings.Add(new ElectricalLighting(
                    reader.GetString("id"),
                    reader.GetString("project_id"),
                    reader.GetString("parent_id"),
                    reader.GetString("manufacturer_id"),
                    reader.GetString("model_no"),
                    reader.GetInt32("qty"),
                    reader.GetBoolean("occupancy"),
                    reader.GetInt32("wattage"),
                    reader.GetBoolean("em_capable"),
                    reader.GetInt32("mounting_type_id"),
                    reader.GetString("tag"),
                    reader.GetString("notes"),
                    reader.GetInt32("voltage_id"),
                    reader.GetInt32("symbol_id"),
                    reader.GetString("color_code"),
                    false,
                    reader.GetString("description"),
                    reader.GetInt32("control_type_id"),
                    reader.GetBoolean("spec_sheet_from_client"),
                    reader.GetString("spec_sheet_id")
                ));
            }

            reader.Close();
            CloseConnection();
            return lightings;
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
    public class S3
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3()
        {
            _bucketName = Properties.Settings.Default.bucket_name;
            _s3Client = new AmazonS3Client(Properties.Settings.Default.aws_access_key_id, Properties.Settings.Default.aws_secret_access_key, RegionEndpoint.USEast1);
        }

        public async Task UploadFileAsync(string keyName, string filePath)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = "application/pdf"
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);
                Console.WriteLine("File uploaded successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        public async Task DownloadAndOpenFileAsync(string keyName, string downloadFilePath)
        {
            try
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(getRequest))
                using (Stream responseStream = response.ResponseStream)
                using (FileStream fileStream = new FileStream(downloadFilePath, FileMode.Create, FileAccess.Write))
                {
                    await responseStream.CopyToAsync(fileStream);
                    Console.WriteLine("File downloaded successfully.");
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadFilePath,
                    UseShellExecute = true
                });
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when reading an object.", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading an object.", e.Message);
            }
        }

        public async Task DeleteFileAsync(string keyName)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                DeleteObjectResponse response = await _s3Client.DeleteObjectAsync(deleteRequest);
                Console.WriteLine("File deleted successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when deleting an object", e.Message);
            }
        }
    }

}
