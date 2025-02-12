using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BCrypt.Net;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

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
            string query =
                @"
            SELECT e.passhash
            FROM employees e 
            WHERE e.username = @username";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@username", userName);
            MySqlDataReader reader = command.ExecuteReader();

            string hashedPassword = "";
            bool result = false;
            if (reader.Read())
            {
                hashedPassword = reader.GetString("passhash");
                result = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            CloseConnection();
            return result;
        }

        public Dictionary<int, string> GetProjectIds(string projectNo)
        {
            string query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            MySqlDataReader reader = command.ExecuteReader();

            Dictionary<int, string> projectIds = new Dictionary<int, string>();
            while (reader.Read())
            {
                projectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
            }
            reader.Close();

            if (!projectIds.Any())
            {
                // Project name does not exist, insert a new entry with a generated ID
                var id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                insertCommand.ExecuteNonQuery();
                projectIds.Add(1, id);
            }

            CloseConnection();
            projectIds = projectIds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return projectIds;
        }
        public Dictionary<int, string> AddProjectVersions(string projectNo, string projectId)
        {
            string query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            MySqlDataReader reader = command.ExecuteReader();

            Dictionary<int, string> projectIds = new Dictionary<int, string>();
            while (reader.Read())
            {
                projectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
            }
            reader.Close();

            if (!projectIds.Any())
            {
                // Project name does not exist, insert a new entry with a generated ID
                var id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                insertCommand.ExecuteNonQuery();
                projectIds.Add(1, id);
            }
            else
            {
                var id = Guid.NewGuid().ToString();
                projectIds = projectIds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                string insertQuery =
                   "INSERT INTO projects (id, gmep_project_no, version) VALUES (@id, @projectNo, @version)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                insertCommand.Parameters.AddWithValue("@version", projectIds.Last().Key + 1);
                insertCommand.ExecuteNonQuery();
                CloneElectricalProject(projectId, id);
                projectIds.Add(projectIds.Last().Key + 1, id);
            }

            CloseConnection();
            return projectIds;
        }
        public Dictionary<int, string> DeleteProjectVersions(string projectNo, string projectId)
        {
            OpenConnection();
            string[] tables = new string[] {
                "electrical_panels",
                "electrical_transformers",
                "electrical_equipment",
                "electrical_lighting_locations",
                "electrical_lighting",
                "electrical_services"
            };
            string query;
            MySqlCommand command;
            foreach (string table in tables)
            {
                query = $"DELETE FROM {table} WHERE project_id = @projectId";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@projectId", projectId);
                command.ExecuteNonQuery();
            }

            query = "DELETE FROM projects WHERE id = @projectId";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.ExecuteNonQuery();

            query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            Dictionary<int, string> projectIds = new Dictionary<int, string>();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                projectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
            }
            reader.Close();

            if (!projectIds.Any())
            {
                // Project name does not exist, insert a new entry with a generated ID
                var id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                insertCommand.ExecuteNonQuery();
                projectIds.Add(1, id);
            }
            CloseConnection();
            return projectIds;

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
            ObservableCollection<ElectricalLighting> lightings,
            ObservableCollection<Location> locations
        )
        {
            OpenConnection();

            UpdateServices(projectId, services);
            UpdatePanels(projectId, panels);
            UpdateEquipments(projectId, equipments);
            UpdateTransformers(projectId, transformers);
            UpdateLightings(projectId, lightings);
            UpdateLightingLocations(projectId, locations);

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

        private void UpdateLightingLocations(string projectId, ObservableCollection<Location> locations)
        {
            var existingLocationIds = GetExistingIds("electrical_lighting_locations", "project_id", projectId);

            foreach (var location in locations)
            {
                if (existingLocationIds.Contains(location.Id))
                {
                    UpdateLocation(location);
                    existingLocationIds.Remove(location.Id);
                }
                else
                {
                    InsertLocation(projectId, location);
                }
            }

            DeleteRemovedItems("electrical_lighting_locations", existingLocationIds);
        }


        private HashSet<string> GetExistingIds(
            string tableName,
            string columnName,
            string projectId
        )
        {
           
            var idType = "id";
            
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
                "UPDATE electrical_services SET name = @name, electrical_service_amp_rating_id = @amp, electrical_service_voltage_id = @type, electrical_service_meter_config_id = @config, color_code = @color_code, aic_rating = @aicRating WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@name", service.Name);
            command.Parameters.AddWithValue("@amp", service.Amp);
            command.Parameters.AddWithValue("@id", service.Id);
            command.Parameters.AddWithValue("@type", service.Type);
            command.Parameters.AddWithValue("@config", service.Config);
            command.Parameters.AddWithValue("@color_code", service.ColorCode);
            command.Parameters.AddWithValue("@aicRating", service.AicRating);
            command.ExecuteNonQuery();
        }

        private void InsertService(string projectId, ElectricalService service)
        {
            string query =
                "INSERT INTO electrical_services (id, project_id, name, electrical_service_amp_rating_id, electrical_service_voltage_id, electrical_service_meter_config_id, color_code, aic_rating) VALUES (@id, @projectId, @name, @amp, @type, @config, @color_code, @aicRating)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", service.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@name", service.Name);
            command.Parameters.AddWithValue("@amp", service.Amp);
            command.Parameters.AddWithValue("@type", service.Type);
            command.Parameters.AddWithValue("@config", service.Config);
            command.Parameters.AddWithValue("@color_code", service.ColorCode);
            command.Parameters.AddWithValue("@aicRating", service.AicRating);
            command.ExecuteNonQuery();
        }

        private void UpdatePanel(ElectricalPanel panel)
        {
            string query =
                "UPDATE electrical_panels SET bus_amp_rating_id = @bus, main_amp_rating_id = @main, is_distribution = @is_distribution, voltage_id = @type, num_breakers = @numBreakers, parent_distance = @distanceFromParent, aic_rating = @aicRating, name = @name, color_code = @color_code, parent_id = @parent_id, is_recessed = @is_recessed, is_mlo = @is_mlo, circuit_no = @circuit_no, is_hidden_on_plan = @is_hidden_on_plan, location = @location, notes = @notes WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@parent_id", panel.ParentId);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@aicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.Parameters.AddWithValue("@type", panel.Type);
            command.Parameters.AddWithValue("@is_recessed", panel.IsRecessed);
            command.Parameters.AddWithValue("@is_mlo", panel.IsMlo);
            command.Parameters.AddWithValue("@circuit_no", panel.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", panel.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@location", panel.Location);
            command.Parameters.AddWithValue("@notes", panel.Notes);
            command.ExecuteNonQuery();
        }

        private void InsertPanel(string projectId, ElectricalPanel panel)
        {
            string query =
                "INSERT INTO electrical_panels (id, project_id, bus_amp_rating_id, main_amp_rating_id, is_distribution, name, color_code, parent_id, num_breakers, parent_distance, aic_rating, voltage_id, is_recessed, is_mlo, circuit_no, is_hidden_on_plan, location, notes) VALUES (@id, @projectId, @bus, @main, @is_distribution, @name, @color_code, @parent_id, @numBreakers, @distanceFromParent, @AicRating, @type, @is_recessed, @is_mlo, @circuit_no, @is_hidden_on_plan, @location, @notes)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@parent_id", panel.ParentId);
            command.Parameters.AddWithValue("@AicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.Parameters.AddWithValue("@type", panel.Type);
            command.Parameters.AddWithValue("@is_recessed", panel.IsRecessed);
            command.Parameters.AddWithValue("@is_mlo", panel.IsMlo);
            command.Parameters.AddWithValue("@circuit_no", panel.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", panel.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@location", panel.Location);
            command.Parameters.AddWithValue("@notes", panel.Notes);
            command.ExecuteNonQuery();
        }

        private void UpdateEquipment(ElectricalEquipment equipment)
        {
            string query =
                "UPDATE electrical_equipment SET description = @description, equip_no = @equip_no, parent_id = @parent_id, owner_id = @owner, voltage_id = @voltage, fla = @fla, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, parent_distance=@distanceFromParent, category_id=@category, color_code = @color_code, connection_type_id = @connection, mca = @mca, hp = @hp, has_plug = @has_plug, locking_connector = @locking_connector, width=@width, depth=@depth, height=@height, circuit_no=@circuit_no, is_hidden_on_plan=@is_hidden_on_plan, load_type = @loadType WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", equipment.Id);
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
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@connection", equipment.Connection);
            command.Parameters.AddWithValue("@mca", equipment.McaId);
            command.Parameters.AddWithValue("@hp", equipment.Hp);
            command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
            command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
            command.Parameters.AddWithValue("@width", equipment.Width);
            command.Parameters.AddWithValue("@depth", equipment.Depth);
            command.Parameters.AddWithValue("@height", equipment.Height);
            command.Parameters.AddWithValue("@circuit_no", equipment.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", equipment.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@loadType", equipment.LoadType);
            command.ExecuteNonQuery();
        }

        private void InsertEquipment(string projectId, ElectricalEquipment equipment)
        {
            string query =
                "INSERT INTO electrical_equipment (id, project_id, equip_no, parent_id, owner_id, voltage_id, fla, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client, parent_distance, category_id, color_code, connection_type_id, description, mca, hp, has_plug, locking_connector, width, depth, height, circuit_no, is_hidden_on_plan, load_type) VALUES (@id, @projectId, @equip_no, @parent_id, @owner, @voltage, @fla, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @distanceFromParent, @category, @color_code, @connection, @description, @mca, @hp, @has_plug, @locking_connector, @width, @depth, @height, @circuit_no, @is_hidden_on_plan, @loadType)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", equipment.Id);
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
            command.Parameters.AddWithValue("@connection", equipment.Connection);
            command.Parameters.AddWithValue("@description", equipment.Description);
            command.Parameters.AddWithValue("@mca", equipment.McaId);
            command.Parameters.AddWithValue("@hp", equipment.Hp);
            command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
            command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
            command.Parameters.AddWithValue("@width", equipment.Width);
            command.Parameters.AddWithValue("@depth", equipment.Depth);
            command.Parameters.AddWithValue("@height", equipment.Height);
            command.Parameters.AddWithValue("@circuit_no", equipment.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", equipment.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@loadType", equipment.LoadType);
            command.ExecuteNonQuery();
        }

        private void UpdateLighting(ElectricalLighting lighting)
        {
            string query =
                "UPDATE electrical_lighting SET notes = @notes, model_no = @model_no, parent_id = @parent_id, voltage_id = @voltageId, color_code = @colorCode, mounting_type_id = @mountingType, occupancy=@occupancy, manufacturer = @manufacturer, wattage = @wattage, em_capable = @em_capable, tag = @tag, symbol_id = @symbolId, description=@description, driver_type_id = @driverTypeId, spec_sheet_from_client=@specFromClient, spec_sheet_id=@specSheetId, qty = @qty, has_photocell = @hasPhotoCell, location_id = @locationId WHERE id = @id";
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
            command.Parameters.AddWithValue("@driverTypeId", lighting.DriverTypeId);
            command.Parameters.AddWithValue("@specFromClient", lighting.SpecSheetFromClient);
            command.Parameters.AddWithValue("@specSheetId", lighting.SpecSheetId);
            command.Parameters.AddWithValue("@qty", lighting.Qty);
            command.Parameters.AddWithValue("@hasPhotoCell", lighting.HasPhotoCell);
            command.Parameters.AddWithValue("@locationId", lighting.LocationId);

            command.ExecuteNonQuery();
        }

        private void InsertLighting(string projectId, ElectricalLighting lighting)
        {
            string query =
                "INSERT INTO electrical_lighting (id, project_id, notes, model_no, parent_id, voltage_id, color_code, mounting_type_id, occupancy, manufacturer, wattage, em_capable, tag, symbol_id, description, driver_type_id, spec_sheet_from_client, spec_sheet_id, qty, has_photocell, location_id) VALUES (@id, @project_id, @notes, @model_no, @parent_id, @voltageId, @colorCode, @mountingType, @occupancy, @manufacturer, @wattage, @em_capable, @tag, @symbolId, @description, @driverTypeId, @specFromClient, @specSheetId, @qty, @hasPhotoCell, @locationId)";
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
            command.Parameters.AddWithValue("@driverTypeId", lighting.DriverTypeId);
            command.Parameters.AddWithValue("@specFromClient", lighting.SpecSheetFromClient);
            command.Parameters.AddWithValue("@specSheetId", lighting.SpecSheetId);
            command.Parameters.AddWithValue("@qty", lighting.Qty);
            command.Parameters.AddWithValue("@hasPhotoCell", lighting.HasPhotoCell);
            command.Parameters.AddWithValue("@locationId", lighting.LocationId);
            command.ExecuteNonQuery();
        }

        private void UpdateTransformer(ElectricalTransformer transformer)
        {
            string query =
                "UPDATE electrical_transformers SET parent_id = @parent_id, voltage_id = @voltage, project_id = @project_id, kva_id = @kva, parent_distance = @distanceFromParent, color_code = @color_code, name = @name, circuit_no = @circuitNo, is_hidden_on_plan = @is_hidden_on_plan, is_wall_mounted = @isWallMounted, aic_rating = @aicRating WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@project_id", transformer.ProjectId);
            command.Parameters.AddWithValue("@voltage", transformer.Voltage);
            command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
            command.Parameters.AddWithValue("@kva", transformer.Kva);
            command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
            command.Parameters.AddWithValue("@name", transformer.Name);
            command.Parameters.AddWithValue("@circuitNo", transformer.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", transformer.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@isWallMounted", transformer.IsWallMounted);
            command.Parameters.AddWithValue("@aicRating", transformer.AicRating);
            command.ExecuteNonQuery();
        }

        private void InsertTransformer(string projectId, ElectricalTransformer transformer)
        {
            string query =
                "INSERT INTO electrical_transformers (id, project_id, parent_id, voltage_id, parent_distance, color_code, kva_id, name, circuit_no, is_hidden_on_plan, is_wall_mounted, aic_rating) VALUES (@id, @project_id, @parent_id, @voltage, @distanceFromParent, @color_code, @kva, @name, @circuitNo, @isHiddenOnPlan, @isWallMounted, @aicRating)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@project_id", transformer.ProjectId);
            command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
            command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
            command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
            command.Parameters.AddWithValue("@kva", transformer.Kva);
            command.Parameters.AddWithValue("@name", transformer.Name);
            command.Parameters.AddWithValue("@voltage", transformer.Voltage);
            command.Parameters.AddWithValue("@circuitNo", transformer.CircuitNo);
            command.Parameters.AddWithValue("@isHiddenOnPlan", transformer.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@isWallMounted", transformer.IsWallMounted);
            command.Parameters.AddWithValue("@aicRating", transformer.AicRating);
            command.ExecuteNonQuery();
        }
        private void UpdateLocation(Location location)
        {
            string query =
                "UPDATE electrical_lighting_locations SET location = @locationDescription, outdoor = @isOutside WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", location.Id);
            command.Parameters.AddWithValue("@locationDescription", location.LocationDescription);
            command.Parameters.AddWithValue("@isOutside", location.IsOutside);

            command.ExecuteNonQuery();
        }

        private void InsertLocation(string projectId, Location location)
        {
            string query =
                "INSERT INTO electrical_lighting_locations (id, project_id, location, outdoor) VALUES (@id, @projectId, @locationDescription, @isOutside)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", location.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@locationDescription", location.LocationDescription);
            command.Parameters.AddWithValue("@isOutside", location.IsOutside);
           
            command.ExecuteNonQuery();
        }

        private void DeleteRemovedItems(string tableName, HashSet<string> ids)
        {
            var idType = "id";
            
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
                        reader.GetString("color_code"),
                        reader.GetInt32("aic_rating")
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
                        reader.IsDBNull(reader.GetOrdinal("bus_amp_rating_id")) ? 0 : reader.GetInt32("bus_amp_rating_id"),
                        reader.IsDBNull(reader.GetOrdinal("main_amp_rating_id")) ? 0 : reader.GetInt32("main_amp_rating_id"),
                        reader.GetBoolean("is_mlo"),
                        reader.GetBoolean("is_distribution"),
                        reader.GetString("name"),
                        reader.GetString("color_code"),
                        reader.GetString("parent_id"),
                        reader.IsDBNull(reader.GetOrdinal("num_breakers")) ? 0 : reader.GetInt32("num_breakers"),
                        reader.IsDBNull(reader.GetOrdinal("parent_distance")) ? 0 : reader.GetInt32("parent_distance"),
                        reader.IsDBNull(reader.GetOrdinal("aic_rating")) ? 0 : reader.GetInt32("aic_rating"),
                        0,
                        reader.IsDBNull(reader.GetOrdinal("voltage_id")) ? 0 : reader.GetInt32("voltage_id"),
                        false,
                        reader.GetBoolean("is_recessed"),
                        reader.IsDBNull(reader.GetOrdinal("circuit_no")) ? 0 : reader.GetInt32("circuit_no"),
                        reader.GetBoolean("is_hidden_on_plan"),
                        reader.GetString("location"),
                        reader.GetString("notes")
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
            while (reader.Read())

                equipments.Add(new ElectricalEquipment(
                    reader.GetString("id"),
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
                    reader.GetInt32("connection_type_id"),
                    reader.GetString("description"),
                    reader.GetInt32("mca"),
                    reader.GetString("hp"),
                    reader.GetBoolean("has_plug"),
                    reader.GetBoolean("locking_connector"),
                    reader.GetFloat("width"),
                    reader.GetFloat("depth"),
                    reader.GetFloat("height"),
                    reader.GetInt32("circuit_no"),
                    reader.GetBoolean("is_hidden_on_plan"),
                    reader.GetInt32("load_type")
                )
             );
           

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
                lightings.Add(
                    new ElectricalLighting(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("parent_id"),
                        reader.GetString("manufacturer"),
                        reader.GetString("model_no"),
                        reader.IsDBNull(reader.GetOrdinal("qty")) ? 0 : reader.GetInt32("qty"),
                        reader.IsDBNull(reader.GetOrdinal("occupancy")) ? false : reader.GetBoolean("occupancy"),
                        reader.IsDBNull(reader.GetOrdinal("wattage")) ? 0 : reader.GetInt32("wattage"),
                        reader.IsDBNull(reader.GetOrdinal("em_capable")) ? false : reader.GetBoolean("em_capable"),
                        reader.IsDBNull(reader.GetOrdinal("mounting_type_id")) ? 0 : reader.GetInt32("mounting_type_id"),
                        reader.GetString("tag"),
                        reader.GetString("notes"),
                        reader.IsDBNull(reader.GetOrdinal("voltage_id")) ? 0 : reader.GetInt32("voltage_id"),
                        reader.IsDBNull(reader.GetOrdinal("symbol_id")) ? 0 : reader.GetInt32("symbol_id"),
                        reader.GetString("color_code"),
                        false,
                        reader.GetString("description"),
                        reader.IsDBNull(reader.GetOrdinal("driver_type_id")) ? 0 : reader.GetInt32("driver_type_id"),
                        reader.IsDBNull(reader.GetOrdinal("spec_sheet_from_client")) ? false : reader.GetBoolean("spec_sheet_from_client"),
                        reader.GetString("spec_sheet_id"),
                        reader.IsDBNull(reader.GetOrdinal("has_photocell")) ? false : reader.GetBoolean("has_photocell"),
                        reader.GetString("location_id")
                    )
                );
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
                        false,
                       reader.GetInt32("circuit_no"),
                       reader.GetBoolean("is_hidden_on_plan"),
                       reader.GetBoolean("is_wall_mounted"),
                       reader.GetInt32("aic_rating")
                    )
                );
            }
            reader.Close();
            CloseConnection();
            return transformers;
        }
        public ObservableCollection<Location> GetLightingLocations(string projectId)
        {
            ObservableCollection<Location> locations =
                new ObservableCollection<Location>();
            string query = "SELECT * FROM electrical_lighting_locations WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var location = new Location();
                location.Id = reader.GetString("id");
                location.isOutside = reader.GetBoolean("outdoor");
                location.LocationDescription = reader.GetString("location");
                locations.Add(location);
            }
            reader.Close();
            CloseConnection();
            return locations;
        }
        public void CloneElectricalProject(string projectId, string newProjectId)
        {
            var services = GetProjectServices(projectId);
            var panels = GetProjectPanels(projectId);
            var equipments = GetProjectEquipment(projectId);
            var lightings = GetProjectLighting(projectId);
            var transformers = GetProjectTransformers(projectId);
            var locations = GetLightingLocations(projectId);

            Dictionary<string, string> parentIdSwitch = new Dictionary<string, string>();
            Dictionary<string, string> locationIdSwitch = new Dictionary<string, string>();

            foreach (var service in services)
            {
                string Id = Guid.NewGuid().ToString();
                parentIdSwitch.Add(service.Id, Id);
                service.Id = Id;
                service.ProjectId = newProjectId;
            }
            foreach (var panel in panels)
            {
                string Id = Guid.NewGuid().ToString();
                parentIdSwitch.Add(panel.Id, Id);
                panel.Id = Id;
                panel.ProjectId = newProjectId;
            }
            foreach (var transformer in transformers)
            {
                string Id = Guid.NewGuid().ToString();
                parentIdSwitch.Add(transformer.Id, Id);
                transformer.Id = Id;
                transformer.ProjectId = newProjectId;
            }
            foreach (var equipment in equipments)
            {
                string Id = Guid.NewGuid().ToString();
                equipment.Id = Id;
                equipment.ProjectId = newProjectId;
            }
            foreach (var location in locations)
            {
                string Id = Guid.NewGuid().ToString();
                locationIdSwitch.Add(location.Id, Id);
                location.Id = Id;
            }
            foreach (var lighting in lightings)
            {
                string Id = Guid.NewGuid().ToString();
                lighting.Id = Id;
                lighting.ProjectId = newProjectId;
                if (!string.IsNullOrEmpty(lighting.LocationId) && lighting.LocationId != "0")
                {
                    lighting.LocationId = locationIdSwitch[lighting.LocationId];
                }
            }
            foreach(var panel in panels)
            {
                if (!string.IsNullOrEmpty(panel.parentId))
                {
                    panel.ParentId = parentIdSwitch[panel.ParentId];
                }
            }
            foreach (var transformer in transformers)
            {
                if (!string.IsNullOrEmpty(transformer.parentId))
                {
                    transformer.ParentId = parentIdSwitch[transformer.ParentId];
                }
            }
            foreach (var equipment in equipments)
            {
                if (!string.IsNullOrEmpty(equipment.parentId))
                {
                    equipment.ParentId = parentIdSwitch[equipment.ParentId];
                }
            }
            UpdateProject(newProjectId, services, panels, equipments, transformers, lightings, locations);

        }
       
    }

    public class S3
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3()
        {
            _bucketName = Properties.Settings.Default.bucket_name;
            _s3Client = new AmazonS3Client(
                Properties.Settings.Default.aws_access_key_id,
                Properties.Settings.Default.aws_secret_access_key,
                RegionEndpoint.USEast1
            );
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
                    ContentType = "application/pdf",
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);
                Console.WriteLine("File uploaded successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    "Error encountered on server. Message:'{0}' when writing an object",
                    e.Message
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object",
                    e.Message
                );
            }
        }

        public async Task DownloadAndOpenFileAsync(string keyName, string downloadFilePath)
        {
            try
            {
                var getRequest = new GetObjectRequest { BucketName = _bucketName, Key = keyName };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(getRequest))
                using (Stream responseStream = response.ResponseStream)
                using (
                    FileStream fileStream = new FileStream(
                        downloadFilePath,
                        FileMode.Create,
                        FileAccess.Write
                    )
                )
                {
                    await responseStream.CopyToAsync(fileStream);
                    Console.WriteLine("File downloaded successfully.");
                }

                Process.Start(
                    new ProcessStartInfo { FileName = downloadFilePath, UseShellExecute = true }
                );
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    "Error encountered on server. Message:'{0}' when reading an object.",
                    e.Message
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when reading an object.",
                    e.Message
                );
            }
        }

        public async Task DeleteFileAsync(string keyName)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName,
                };

                DeleteObjectResponse response = await _s3Client.DeleteObjectAsync(deleteRequest);
                Console.WriteLine("File deleted successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    "Error encountered on server. Message:'{0}' when deleting an object",
                    e.Message
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when deleting an object",
                    e.Message
                );
            }
        }
    }
}
