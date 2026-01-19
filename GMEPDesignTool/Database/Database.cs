using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BCrypt.Net;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using MySqlX.XDevAPI;
using Org.BouncyCastle.Crypto.Generators;
using RtfPipe.Tokens;

namespace GMEPDesignTool.Database
{
  public class Database
  {
    public string ConnectionString { get; set; }
    public MySqlConnection Connection { get; set; }

    public MySqlConnection SessionConnection { get; set; }

    public Database(string sqlConnectionString)
    {
      ConnectionString = sqlConnectionString;
      Connection = new MySqlConnection(ConnectionString);
      SessionConnection = new MySqlConnection(ConnectionString);
    }

    public void OpenConnection(MySqlConnection conn)
    {
      if (conn.State == System.Data.ConnectionState.Closed)
      {
        Trace.WriteLine(ConnectionString);
        conn.Open();
      }
    }

    public void CloseConnection(MySqlConnection conn)
    {
      if (conn.State == System.Data.ConnectionState.Open)
      {
        conn.Close();
      }
    }

    public async Task OpenConnectionAsync(MySqlConnection conn)
    {
      if (conn.State == System.Data.ConnectionState.Closed)
      {
        await conn.OpenAsync();
      }
    }

    public async Task CloseConnectionAsync(MySqlConnection conn)
    {
      if (conn.State == System.Data.ConnectionState.Open)
      {
        await conn.CloseAsync();
      }
    }

    string GetSafeString(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetString(index);
      }
      return string.Empty;
    }

    char GetSafeChar(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetChar(index);
      }
      return char.MinValue;
    }

    DateTime? GetUnsafeDate(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetDateTime(index);
      }
      return null;
    }

    int GetSafeInt(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetInt32(index);
      }
      return 0;
    }

    uint? GetUnsafeUInt(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetUInt32(index);
      }
      return null;
    }

    ulong? GetUnsafeULong(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetUInt64(index);
      }
      return null;
    }

    float GetSafeFloat(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetFloat(index);
      }
      return 0;
    }

    bool GetSafeBoolean(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetBoolean(index);
      }
      return false;
    }

    DateTime GetSafeDateTime(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetDateTime(index);
      }
      return DateTime.MinValue;
    }

    DateTime? GetUnsafeDateTime(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        if (reader.GetDateTime(index) == DateTime.MinValue)
        {
          return null;
        }
        return reader.GetDateTime(index);
      }
      return null;
    }

    public void SetProposalPdf(string proposalId, string pdfName)
    {
      string query =
        @"
            UPDATE proposals
            SET proposals.pdf_name = @pdf_name
            WHERE id = @id";
      MySqlConnection Connection2 = new MySqlConnection(ConnectionString);
      OpenConnection(Connection2);
      MySqlCommand command = new MySqlCommand(query, Connection2);
      command.Parameters.AddWithValue("@pdf_name", pdfName);
      command.Parameters.AddWithValue("@id", proposalId);
      command.ExecuteNonQuery();
      CloseConnection(Connection2);
    }

    public void SetProposalData(string proposalId, string data)
    {
      string query =
        @"
        UPDATE proposals SET
        proposals.data = @data
        WHERE id = @id
        ";
      MySqlConnection Connection2 = new MySqlConnection(ConnectionString);
      OpenConnection(Connection2);
      MySqlCommand command = new MySqlCommand(query, Connection2);
      command.Parameters.AddWithValue("@data", data);
      command.Parameters.AddWithValue("@id", proposalId);
      command.ExecuteNonQuery();
      CloseConnection(Connection2);
    }

    public async Task<ObservableCollection<Proposal>> GetProposals(string projectId)
    {
      ObservableCollection<Proposal> proposals = new ObservableCollection<Proposal>();
      string query =
        @"
                        SELECT 
                            proposals.id,
                            proposals.pdf_name,
                            proposals.project_id,
                            proposals.date_created AS date_created,
                            proposals.status_id,
                            proposals.type_id,
                            proposal_types.type AS type,
                            proposal_statuses.status,
                            employees.username AS username            
                        FROM proposals
                        LEFT JOIN proposal_types ON proposals.type_id = proposal_types.id
                        LEFT JOIN employees ON proposals.employee_id = employees.id
                        LEFT JOIN proposal_statuses ON proposal_statuses.id = proposals.status_id
                        where proposals.project_id = @projectId
                        order by date_created DESC";
      await OpenConnectionAsync(Connection);

      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        proposals.Add(
          new Proposal
          {
            Id = GetSafeString(reader, "id"),
            ProjectId = GetSafeString(reader, "project_id"),
            DateCreated = GetSafeDateTime(reader, "date_created"),
            Type = GetSafeString(reader, "type"),
            TypeId = GetSafeInt(reader, "type_id"),
            EmployeeUsername = GetSafeString(reader, "username"),
            PdfName = GetSafeString(reader, "pdf_name"),
            Status = GetSafeString(reader, "status"),
            StatusId = GetSafeInt(reader, "status_id"),
          }
        );
      }

      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return proposals;
    }

    public async Task<Proposal?> GetProposalById(string proposalId)
    {
      Proposal? proposal = null;
      string query =
        @"
                        SELECT 
                            proposals.id,
                            proposals.pdf_name,
                            proposals.project_id,
                            proposals.date_created AS date_created,
                            proposals.data,
                            proposals.type_id,
                            proposal_types.type AS type,
                            proposals.status_id,
                            employees.username AS username            
                        FROM proposals
                        LEFT JOIN proposal_types ON proposals.type_id = proposal_types.id
                        LEFT JOIN employees ON proposals.employee_id = employees.id
                        where proposals.id = @proposalId";
      await OpenConnectionAsync(Connection);

      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@proposalId", proposalId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        string dataString = GetSafeString(reader, "data");
        ProposalData? proposalData = null;
        try
        {
          proposalData = JsonSerializer.Deserialize<ProposalData>(dataString);
        }
        catch (Exception ex) { }
        proposal = new Proposal
        {
          Id = GetSafeString(reader, "id"),
          ProjectId = GetSafeString(reader, "project_id"),
          DateCreated = GetSafeDateTime(reader, "date_created"),
          Type = GetSafeString(reader, "type"),
          TypeId = GetSafeInt(reader, "type_id"),
          Data = proposalData,
          EmployeeUsername = GetSafeString(reader, "username"),
          PdfName = GetSafeString(reader, "pdf_name"),
          StatusId = GetSafeInt(reader, "status_id"),
        };
      }

      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return proposal;
    }

    public List<ProposalListItem> GetProposalsByStatusId(int statusId)
    {
      List<ProposalListItem> proposals = new List<ProposalListItem>();
      string query =
        @"
        SELECT
        proposals.id,
        proposals.project_id,
        projects.gmep_project_name,
        projects.gmep_project_no,
        proposal_types.type
        FROM proposals
        LEFT JOIN projects ON projects.id = proposals.project_id
        LEFT JOIN proposal_types ON proposal_types.id = proposals.type_id
        WHERE proposals.status_id = @statusId
        GROUP BY proposals.project_id
        ORDER BY proposals.date_created DESC
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@statusId", statusId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        proposals.Add(
          new ProposalListItem(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "gmep_project_name"),
            GetSafeString(reader, "project_id"),
            GetSafeString(reader, "gmep_project_no"),
            GetSafeString(reader, "type")
          )
        );
      }
      reader.Close();
      CloseConnection(Connection);
      return proposals;
    }

    public void SaveProposal(Proposal proposal)
    {
      string query =
        @"
        UPDATE proposals SET
        rfp_date = @rfp_date,
        project_id = @project_id,
        proposal_date = @proposal_date,
        type_id = @type_id,
        status_id = @status_id,
        employee_id = @employee_id,
        pdf_name = @pdf_name,
        is_estimate = @is_estimate,
        notes = @notes,
        last_follow_up_date = @last_follow_up_date,
        followed_up_by_employee_id = @followed_up_by_employee_id,
        fees = @fees,
        s_drive_path = @s_drive_path
        WHERE id = @id
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@rfp_date", proposal.RfpDate);
      command.Parameters.AddWithValue("@project_id", proposal.ProjectId);
      command.Parameters.AddWithValue("@proposal_date", proposal.ProposalDate);
      command.Parameters.AddWithValue("@type_id", proposal.TypeId);
      command.Parameters.AddWithValue("@status_id", proposal.StatusId);
      command.Parameters.AddWithValue("@employee_id", proposal.SentByEmployeeId);
      command.Parameters.AddWithValue("@pdf_name", proposal.PdfName);
      command.Parameters.AddWithValue("@is_estimate", proposal.IsEstimate);
      command.Parameters.AddWithValue("@notes", proposal.Notes);
      command.Parameters.AddWithValue("@last_follow_up_date", proposal.LastFollowUpDate);
      command.Parameters.AddWithValue(
        "@followed_up_by_employee_id",
        proposal.FollowedUpByEmployeeId
      );
      command.Parameters.AddWithValue("@fees", proposal.Fees);
      command.Parameters.AddWithValue("@s_drive_path", proposal.SDrivePath);
      command.Parameters.AddWithValue("@id", proposal.Id);
      command.ExecuteNonQuery();

      query =
        @"
        UPDATE projects SET
        region_id = @region_id,
        client_company_id = @client_company_id
        WHERE id = @id
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@region_id", proposal.RegionId);
      command.Parameters.AddWithValue("@client_company_id", proposal.ClientCompanyId);
      command.Parameters.AddWithValue("@id", proposal.ProjectId);
      command.ExecuteNonQuery();
      Connection.Close();
    }

    public void SetProposalWindowProjectValues(Proposal p)
    {
      string query =
        @"
        UPDATE projects SET
        client_company_id = @clientCompanyId,
        gmep_project_name = @projectName,
        gmep_project_no = @projectNo
        WHERE id = @projectId
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@clientCompanyId", p.ClientCompanyId);
      command.Parameters.AddWithValue("@projectName", p.ProjectName);
      command.Parameters.AddWithValue("@projectNo", p.ProjectNo);
      command.Parameters.AddWithValue("@projectId", p.ProjectId);
      command.ExecuteNonQuery();
      Connection.Close();
    }

    public async Task<AdminModel> GetAdminByProjectId(string projectId)
    {
      AdminModel adminModel = null;
      string query =
        @"
                        SELECT 
                        gmep_project_no,
                        gmep_project_name,
                        client_companies.name as client,
                        projects.client_company_id,
                        architect_companies.name as architect,
                        projects.architect_company_id as architect_company_id,
                        projects.street_address,
                        projects.city,
                        projects.state,
                        projects.postal_code, 
                        projects.directory,
                        s,
                        m,
                        e,
                        p,
                        descriptions
                        FROM projects
                        LEFT JOIN companies AS client_companies ON client_companies.id = projects.client_company_id
                        LEFT JOIN companies AS architect_companies ON architect_companies.id = projects.architect_company_id
                        WHERE projects.id = @projectId
      ";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);

      using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
      {
        if (await reader.ReadAsync())
        {
          adminModel = new AdminModel
          {
            ProjectNo = GetSafeString(reader, "gmep_project_no"),
            ProjectName = GetSafeString(reader, "gmep_project_name"),
            Client = GetSafeString(reader, "client"),
            ClientCompanyId = GetSafeString(reader, "client_company_id"),
            Architect = GetSafeString(reader, "architect"),
            ArchitectCompanyId = GetSafeString(reader, "architect_company_id"),
            StreetAddress = GetSafeString(reader, "street_address"),
            City = GetSafeString(reader, "city"),
            State = GetSafeString(reader, "state"),
            PostalCode = GetSafeString(reader, "postal_code"),
            Directory = GetSafeString(reader, "directory"),
            IsCheckedS = GetSafeBoolean(reader, "s"),
            IsCheckedM = GetSafeBoolean(reader, "m"),
            IsCheckedE = GetSafeBoolean(reader, "e"),
            IsCheckedP = GetSafeBoolean(reader, "p"),
            Descriptions = GetSafeString(reader, "descriptions"),
          };
        }
      }

      await CloseConnectionAsync(Connection);
      return adminModel;
    }

    public async Task<string> DuplicateProposal(string id, string employeeId)
    {
      Proposal? proposal = await GetProposalById(id);
      if (proposal == null || proposal.Data == null)
      {
        return id;
      }
      string newId = Guid.NewGuid().ToString();
      string query =
        @"
        INSERT INTO proposals
        ( id,  project_id,  type_id,  status_id,  employee_id) VALUES
        (@id, @project_id, @type_id, @status_id, @employee_id)
        ";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", newId);
      command.Parameters.AddWithValue("@project_id", proposal.ProjectId);
      command.Parameters.AddWithValue("@type_id", proposal.TypeId);
      command.Parameters.AddWithValue("@status_id", proposal.StatusId);
      command.Parameters.AddWithValue("@employee_id", employeeId);
      await command.ExecuteNonQueryAsync();
      await CloseConnectionAsync(Connection);
      return newId;
    }

    public ObservableCollection<Proposal> GetProposalsByYear(string year)
    {
      ObservableCollection<Proposal> proposals = new ObservableCollection<Proposal>();

      string query =
        @"
        SELECT 
        proposals.rfp_date,
        proposals.proposal_date,
        proposals.date_created,
        sender_employees.id as sender_employee_id,
        company_contacts.first_name as company_contact_first_name,
        company_contacts.last_name as company_contact_last_name,
        companies.id as company_id,
        companies.name as company_name,
        projects.gmep_project_name,
        proposals.is_estimate,
        projects.gmep_project_no,
        projects.id as project_id,
        proposals.data,
        proposals.fees,
        proposals.notes,
        proposals.type_id,
        proposals.last_follow_up_date,
        proposals.id as proposal_id,
        follow_up_employees.id as follow_up_employee_id,
        proposals.status_id,
        projects.region_id,
        proposals.s_drive_path
        FROM proposals
        LEFT JOIN employees AS sender_employees ON sender_employees.id = proposals.employee_id
        LEFT JOIN contacts AS sender_employee_contacts ON sender_employee_contacts.id = sender_employees.contact_id
        LEFT JOIN projects ON projects.id = proposals.project_id
        LEFT JOIN employees AS follow_up_employees ON follow_up_employees.id = proposals.employee_id
        LEFT JOIN companies ON companies.id = projects.client_company_id
        LEFT JOIN contacts AS company_contacts ON company_contacts.id = companies.primary_contact_id
        WHERE proposals.rfp_date BETWEEN @yearStart AND @yearEnd
        ORDER BY proposals.date_created DESC
        ";
      string yearStart = $"{year}-01-01";
      string yearEnd = $"{year}-12-31";

      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@yearStart", yearStart);
      command.Parameters.AddWithValue("@yearEnd", yearEnd);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        string projectId = GetSafeString(reader, "project_id");
        Proposal? proposal = proposals.FirstOrDefault((p) => p.ProjectId == projectId);
        if (proposal == null)
        {
          string dataString = GetSafeString(reader, "data");
          ProposalData? proposalData = null;
          int totalPrice = 0;
          try
          {
            proposalData = JsonSerializer.Deserialize<ProposalData>(dataString);
            if (proposalData != null)
            {
              Int32.TryParse(proposalData.TotalPrice, out totalPrice);
            }
          }
          catch (Exception ex) { }
          proposals.Add(
            new Proposal
            {
              Id = GetSafeString(reader, "proposal_id"),
              RfpDate = GetSafeDateTime(reader, "rfp_date"),
              ProposalDate = GetUnsafeDateTime(reader, "proposal_date"),
              SentByEmployeeId = GetSafeString(reader, "sender_employee_id"),
              ContactName =
                GetSafeString(reader, "company_contact_first_name")
                + " "
                + GetSafeString(reader, "company_contact_last_name"),
              CompanyName = GetSafeString(reader, "company_name"),
              ClientCompanyId = GetSafeString(reader, "company_id"),
              ProjectName = GetSafeString(reader, "gmep_project_name"),
              IsEstimate = GetSafeBoolean(reader, "is_estimate"),
              ProjectNo = GetSafeString(reader, "gmep_project_no"),
              Fees = GetSafeInt(reader, "fees") > 0 ? GetSafeInt(reader, "fees") : totalPrice,
              Notes = GetSafeString(reader, "notes"),
              LastFollowUpDate = GetUnsafeDateTime(reader, "last_follow_up_date"),
              StatusId = GetSafeInt(reader, "status_id"),
              FollowedUpByEmployeeId = GetSafeString(reader, "follow_up_employee_id"),
              RegionId = GetSafeInt(reader, "region_id"),
              SDrivePath = GetSafeString(reader, "s_drive_path"),
              ProjectId = GetSafeString(reader, "project_id"),
              TypeId = GetSafeInt(reader, "type_id"),
              Data = proposalData,
              db = new Database(ConnectionString),
            }
          );
        }
      }
      reader.Close();
      CloseConnection(Connection);
      return proposals;
    }

    public async Task UpdateAdminProject(AdminModel model, string projectId)
    {
      string query =
        @"
            UPDATE projects
            SET gmep_project_name = @name,
                gmep_project_no = @projectNo,
                street_address = @address,
                client_company_id = @clientCompanyId,
                architect_company_id = @architectCompanyId,
                city = @city,
                state = @state,
                postal_code = @postalCode,
                directory  = @directory,
                s = @s,
                m = @m,
                e = @e,
                p = @p,
                descriptions = @descriptions
            WHERE id = @projectId";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      command.Parameters.AddWithValue("@name", model.ProjectName);
      command.Parameters.AddWithValue("@projectNo", model.ProjectNo);
      command.Parameters.AddWithValue("@clientCompanyId", model.ClientCompanyId);
      command.Parameters.AddWithValue("@architectCompanyId", model.ArchitectCompanyId);
      command.Parameters.AddWithValue("@address", model.StreetAddress);
      command.Parameters.AddWithValue("@city", model.City);
      command.Parameters.AddWithValue("@state", model.State);
      command.Parameters.AddWithValue("@postalCode", model.PostalCode);
      command.Parameters.AddWithValue("@directory", model.Directory);
      command.Parameters.AddWithValue("@s", model.IsCheckedS);
      command.Parameters.AddWithValue("@m", model.IsCheckedM);
      command.Parameters.AddWithValue("@e", model.IsCheckedE);
      command.Parameters.AddWithValue("@p", model.IsCheckedP);
      command.Parameters.AddWithValue("@descriptions", model.Descriptions);

      await command.ExecuteNonQueryAsync();
      command.Dispose();
      await CloseConnectionAsync(Connection);
    }

    public async Task<ObservableCollection<PlumbingModel>> GetPlumbingModelByProjectId(
      string projectId
    )
    {
      ObservableCollection<PlumbingModel> fixtures = new ObservableCollection<PlumbingModel>();
      string query =
        @"
        SELECT 
            plumbing_fixture_types.abbreviation,
            plumbing_fixtures.number,
            plumbing_fixture_catalog.description,
            plumbing_fixture_types.name,
            plumbing_fixture_catalog.make,
            plumbing_fixture_catalog.model,
            plumbing_fixture_catalog.trap,
            plumbing_fixture_catalog.waste,
            plumbing_fixture_catalog.vent,
            plumbing_fixture_catalog.cold_water,
            plumbing_fixture_catalog.hot_water,
            plumbing_fixture_catalog.fixture_demand,
            plumbing_fixture_catalog.hot_demand,
            plumbing_fixture_catalog.dfu
        FROM plumbing_fixtures
        LEFT JOIN plumbing_fixture_catalog ON plumbing_fixture_catalog.id = plumbing_fixtures.catalog_id
        LEFT JOIN plumbing_fixture_types ON plumbing_fixture_types.id = plumbing_fixture_catalog.type_id
        WHERE plumbing_fixtures.project_id = @projectId";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        fixtures.Add(
          new PlumbingModel
          {
            Abbreviation = GetSafeString(reader, "abbreviation"),
            Number = GetSafeInt(reader, "number"),
            Description = GetSafeString(reader, "description"),
            Name = GetSafeString(reader, "name"),
            Make = GetSafeString(reader, "make"),
            Model = GetSafeString(reader, "model"),
            Trap = GetSafeFloat(reader, "trap"),
            Waste = GetSafeFloat(reader, "waste"),
            Vent = GetSafeFloat(reader, "vent"),
            ColdWater = GetSafeFloat(reader, "cold_water"),
            HotWater = GetSafeFloat(reader, "hot_water"),
            FixtureDemand = GetSafeFloat(reader, "fixture_demand"),
            HotDemand = GetSafeFloat(reader, "hot_demand"),
            DFU = GetSafeInt(reader, "dfu"),
          }
        );
      }

      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return fixtures;
    }

    public bool LoginUser(string userName, string password)
    {
      string query =
        @"
            SELECT e.passhash
            FROM employees e 
            WHERE e.username = @username";
      OpenConnection(Connection);
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
      reader.Close();
      CloseConnection(Connection);
      return result;
    }

    public List<Employee> GetAdminEmployeesByYear(string year)
    {
      List<Employee> employees = new List<Employee>();
      string query =
        @"
        SELECT 
        employees.id as employee_id,
        contacts.id as contact_id,
        entities.id as entity_id,
        last_name,
        first_name,
        email_address,
        email_addresses.id as email_address_id,
        phone_number,
        phone_numbers.id as phone_number_id,
        extension,
        hire_date,
        termination_date,
        employee_department_id,
        employee_title_id,
        employee_access_level_id,
        username
        FROM employees
        LEFT JOIN contacts ON contacts.id = employees.contact_id
        LEFT JOIN entities ON contacts.entity_id = entities.id
        LEFT JOIN email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
        LEFT JOIN email_addresses ON email_addr_entity_rel.email_address_id = email_addresses.id
        LEFT JOIN phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
        LEFT JOIN phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
        WHERE (
          employees.termination_date IS NULL 
          OR
          ( employees.termination_date >= @yearStart AND employees.hire_date <= @yearEnd )
         )
        AND ( employees.employee_access_level_id = 1 OR employees.employee_access_level_id = 2 )
        ORDER BY last_name ASC
        ";
      string yearEnd = $"{year}-12-31";
      string yearStart = $"{year}-01-01";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@yearStart", yearStart);
      command.Parameters.AddWithValue("@yearEnd", yearEnd);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        employees.Add(
          new Employee(
            GetSafeString(reader, "employee_id"),
            GetSafeString(reader, "contact_id"),
            GetSafeString(reader, "entity_id"),
            GetSafeString(reader, "last_name"),
            GetSafeString(reader, "first_name"),
            GetSafeInt(reader, "employee_title_id"),
            GetSafeInt(reader, "employee_department_id"),
            GetSafeString(reader, "email_address"),
            GetSafeString(reader, "email_address_id"),
            GetUnsafeULong(reader, "phone_number"),
            GetSafeString(reader, "phone_number_id"),
            GetUnsafeUInt(reader, "extension"),
            GetUnsafeDate(reader, "hire_date"),
            GetUnsafeDate(reader, "termination_date"),
            GetSafeString(reader, "username")
          )
        );
      }
      reader.Close();
      CloseConnection(Connection);
      return employees;
    }

    public List<Employee> GetEmployees()
    {
      List<Employee> employees = new List<Employee>();
      string query =
        @"
                SELECT 
                employees.id as employee_id,
                contacts.id as contact_id,
                entities.id as entity_id,
                last_name,
                first_name,
                email_address,
                email_addresses.id as email_address_id,
                phone_number,
                phone_numbers.id as phone_number_id,
                extension,
                hire_date,
                termination_date,
                employee_department_id,
                employee_title_id,
                username
                FROM employees
                LEFT JOIN contacts ON contacts.id = employees.contact_id
                LEFT JOIN entities ON contacts.entity_id = entities.id
                LEFT JOIN email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
                LEFT JOIN email_addresses ON email_addr_entity_rel.email_address_id = email_addresses.id
                LEFT JOIN phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
                LEFT JOIN phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
                ORDER BY last_name ASC
                ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        employees.Add(
          new Employee(
            GetSafeString(reader, "employee_id"),
            GetSafeString(reader, "contact_id"),
            GetSafeString(reader, "entity_id"),
            GetSafeString(reader, "last_name"),
            GetSafeString(reader, "first_name"),
            GetSafeInt(reader, "employee_title_id"),
            GetSafeInt(reader, "employee_department_id"),
            GetSafeString(reader, "email_address"),
            GetSafeString(reader, "email_address_id"),
            GetUnsafeULong(reader, "phone_number"),
            GetSafeString(reader, "phone_number_id"),
            GetUnsafeUInt(reader, "extension"),
            GetUnsafeDate(reader, "hire_date"),
            GetUnsafeDate(reader, "termination_date"),
            GetSafeString(reader, "username")
          )
        );
      }
      reader.Close();
      CloseConnection(Connection);
      return employees;
    }

    public void SaveEmployee(Employee employee)
    {
      OpenConnection(Connection);
      var query =
        @"
                    UPDATE employees SET
                    employee_title_id = @titleId,
                    employee_department_id = @departmentId,
                    hire_date = @hireDate,
                    termination_date = @terminationDate,
                    username = @username
                    WHERE id = @id
                    ";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@titleId", employee.TitleId);
      command.Parameters.AddWithValue("@departmentId", employee.DepartmentId);
      command.Parameters.AddWithValue("@hireDate", employee.HireDate);
      if (employee.TerminationDate == DateTime.MinValue)
      {
        command.Parameters.AddWithValue("@terminationDate", null);
      }
      else
      {
        command.Parameters.AddWithValue("@terminationDate", employee.TerminationDate);
      }
      command.Parameters.AddWithValue("@username", employee.Username);
      command.Parameters.AddWithValue("@id", employee.Id);
      command.ExecuteNonQuery();
      if (employee.NewEmailAddress)
      {
        string emailAddressId = Guid.NewGuid().ToString();
        string emailAddressRelId = Guid.NewGuid().ToString();
        query =
          @"
                    INSERT INTO email_addresses (id, email_address)
                    VALUES (@id, @emailAddress)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", emailAddressId);
        command.Parameters.AddWithValue("@emailAddress", employee.EmailAddress);
        command.ExecuteNonQuery();
        query =
          @"
                    INSERT INTO email_addresses_entity_rel (id, email_address_id, entity_id, is_primary)
                    VALUES (@id, @emailAddressId, @entityId, 1)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", emailAddressRelId);
        command.Parameters.AddWithValue("@emailAddressId", emailAddressId);
        command.Parameters.AddWithValue("@entityId", employee.EntityId);
        command.ExecuteNonQuery();
        employee.EmailAddressId = emailAddressId;
        employee.NewEmailAddress = false;
      }
      else
      {
        query =
          @"
                    UPDATE email_addresses
                    SET email_address = @emailAddress
                    WHERE id = @emailAddressId
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@emailAddress", employee.EmailAddress);
        command.Parameters.AddWithValue("@emailAddressId", employee.EmailAddressId);
        command.ExecuteNonQuery();
      }
      if (employee.NewPhoneNumber)
      {
        string phoneNumberId = Guid.NewGuid().ToString();
        string phoneNumberRelId = Guid.NewGuid().ToString();
        query =
          @"
                    INSERT INTO phone_numbers (id, phone_number, extension, calling_code)
                    VALUES (@id, @phoneNumber, @extension, 1)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", phoneNumberId);
        command.Parameters.AddWithValue("@phoneNumber", employee.PhoneNumber);
        command.Parameters.AddWithValue(
          "@extension",
          employee.Extension == 0 ? null : employee.Extension
        );
        command.ExecuteNonQuery();
        query =
          @"
                    INSERT INTO phone_number_entity_rel (id, phone_number_id, entity_id, is_primary)
                    VALUES (@id, @phoneNumberId, @entityId, 1)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", phoneNumberRelId);
        command.Parameters.AddWithValue("@phoneNumberId", phoneNumberId);
        command.Parameters.AddWithValue("@entityId", employee.EntityId);
        command.ExecuteNonQuery();
        employee.PhoneNumberId = phoneNumberId;
        employee.NewPhoneNumber = false;
      }
      else
      {
        query =
          @"
                    UPDATE phone_numbers
                    SET phone_number = @phoneNumber
                    WHERE id = @phoneNumberId
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@phoneNumber", employee.PhoneNumber);
        command.Parameters.AddWithValue("@phoneNumberId", employee.PhoneNumberId);
        command.ExecuteNonQuery();
      }
      query =
        @"
                    UPDATE contacts
                    SET
                    first_name = @firstName,
                    last_name = @lastName
                    WHERE id = @contactId
                    ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@firstName", employee.FirstName);
      command.Parameters.AddWithValue("@lastName", employee.LastName);
      command.Parameters.AddWithValue("@contactId", employee.ContactId);
      command.ExecuteNonQuery();

      CloseConnection(Connection);
    }

    public void SetEmployeePassword(string employeeId, string password)
    {
      string query = "UPDATE employees SET passhash = @passhash WHERE id = @id";
      string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
      string passhash = BCrypt.Net.BCrypt.HashPassword(password, salt);
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@passhash", passhash);
      command.Parameters.AddWithValue("@id", employeeId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public void CreateCompany(Company company)
    {
      string query =
        @"
        INSERT INTO entities
        ( id ) VALUES
        (@id)
        ";

      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", company.EntityId);
      try
      {
        command.ExecuteNonQuery();
      }
      catch (Exception ex)
      {
        CloseConnection(Connection);
        return;
      }

      query =
        @"
        INSERT INTO email_addresses
        ( id,  email_address) VALUES
        (@id, @email_address)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", company.CompanyEmailId);
      command.Parameters.AddWithValue("@email_address", company.CompanyEmail);
      command.ExecuteNonQuery();

      query =
        @"
        INSERT INTO email_addr_entity_rel
        ( id,  email_address_id,  entity_id,  is_primary) VALUES
        (@id, @email_address_id, @entity_id, @is_primary)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
      command.Parameters.AddWithValue("@email_address_id", company.CompanyEmailId);
      command.Parameters.AddWithValue("@entity_id", company.EntityId);
      command.Parameters.AddWithValue("@is_primary", 1);
      command.ExecuteNonQuery();

      query =
        @"
        INSERT INTO phone_numbers
        ( id,  phone_number,  extension) VALUES
        (@id, @phone_number, @extension)
        ";

      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", company.CompanyPhoneId);
      command.Parameters.AddWithValue("@phone_number", company.CompanyPhone);
      command.Parameters.AddWithValue("@extension", company.CompanyExtension);
      command.ExecuteNonQuery();

      query =
        @"
        INSERT INTO phone_number_entity_rel
        ( id,  phone_number_id,  entity_id,  is_primary) VALUES
        (@id, @phone_number_id, @entity_id, @is_primary)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
      command.Parameters.AddWithValue("@phone_number_id", company.CompanyPhoneId);
      command.Parameters.AddWithValue("@entity_id", company.EntityId);
      command.Parameters.AddWithValue("@is_primary", 1);
      command.ExecuteNonQuery();

      query =
        @"
          INSERT INTO companies
          ( id,  entity_id,  name,  street_address,  city,  state,  postal_code) VALUES
          (@id, @entity_id, @name, @street_address, @city, @state, @postal_code)
         ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@name", company.CompanyName);
      command.Parameters.AddWithValue("@street_address", company.StreetAddress);
      command.Parameters.AddWithValue("@city", company.City);
      command.Parameters.AddWithValue("@state", company.State);
      command.Parameters.AddWithValue("@postal_code", company.PostalCode);
      command.Parameters.AddWithValue("@entity_id", company.EntityId);
      command.Parameters.AddWithValue("@id", company.CompanyId);
      command.ExecuteNonQuery();

      company.New = false;
    }

    public void CreateClient(Client client)
    {
      string query = "SELECT id FROM clients WHERE company_id = @company_id";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@company_id", client.CompanyId);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        reader.Close();
        CloseConnection(Connection);
        return;
      }
      reader.Close();
      query =
        @"
        INSERT INTO clients
        ( id,  company_id,  loyalty_type_id) VALUES
        (@id, @company_id, @loyalty_type_id)
        ";
      OpenConnection(Connection);
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
      command.Parameters.AddWithValue("@company_id", client.CompanyId);
      command.Parameters.AddWithValue("@loyalty_type_id", client.LoyaltyTypeId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public void CreateArchitect(Architect architect)
    {
      string query = "SELECT id FROM architects WHERE company_id = @company_id";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@company_id", architect.CompanyId);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        CloseConnection(Connection);
        return;
      }
      query =
        @"
        INSERT INTO architects
        ( id,  company_id ) VALUES
        (@id, @company_id )
        ";
      OpenConnection(Connection);
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
      command.Parameters.AddWithValue("@company_id", architect.CompanyId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public List<Client> GetClients()
    {
      List<Client> clients = new List<Client>();
      string query =
        @"
        SELECT 
        companies.id,
        companies.entity_id,
        companies.name,
        companies.street_address,
        companies.city,
        companies.state,
        companies.postal_code,
        companies.primary_contact_id,
        phone_numbers.phone_number,
        phone_numbers.extension,
        phone_numbers.id as phone_number_id,
        email_addresses.email_address,
        email_addresses.id as email_address_id,
        contacts.id as primary_contact_id,
        contacts.first_name,
        contacts.last_name,
        clients.loyalty_type_id
        FROM companies
        LEFT JOIN
        entities ON entities.id = companies.entity_id
        LEFT JOIN
        phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
        LEFT JOIN
        phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
        LEFT JOIN
        email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
        LEFT JOIN
        email_addresses ON email_addresses.id = email_addr_entity_rel.email_address_id
        LEFT JOIN
        contacts ON contacts.id = companies.primary_contact_id
        LEFT JOIN
        clients ON clients.company_id = companies.id
        WHERE ( email_addr_entity_rel.is_primary OR email_addr_entity_rel.is_primary IS NULL )
        AND ( phone_number_entity_rel.is_primary OR phone_number_entity_rel.is_primary IS NULL )
        AND clients.company_id IS NOT NULL
        AND clients.date_deleted IS NULL
        GROUP BY companies.id
        ORDER BY companies.name
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        clients.Add(
          new Client(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "entity_id"),
            GetSafeString(reader, "name"),
            GetSafeInt(reader, "loyalty_type_id"),
            GetSafeString(reader, "street_address"),
            GetSafeString(reader, "city"),
            GetSafeString(reader, "state"),
            GetSafeString(reader, "postal_code"),
            GetSafeString(reader, "email_address_id"),
            GetSafeString(reader, "email_address"),
            GetSafeString(reader, "phone_number_id"),
            GetUnsafeULong(reader, "phone_number"),
            GetUnsafeUInt(reader, "extension"),
            GetSafeString(reader, "primary_contact_id"),
            GetSafeString(reader, "first_name"),
            GetSafeString(reader, "last_name")
          )
        );
      }
      reader.Close();
      CloseConnection(Connection);
      return clients;
    }

    public Client? GetClient(string companyId)
    {
      Client? client = null;
      string query =
        @"
        SELECT 
        companies.id,
        companies.entity_id,
        companies.name,
        companies.street_address,
        companies.city,
        companies.state,
        companies.postal_code,
        companies.primary_contact_id,
        phone_numbers.phone_number,
        phone_numbers.extension,
        phone_numbers.id as phone_number_id,
        email_addresses.email_address,
        email_addresses.id as email_address_id,
        contacts.id as primary_contact_id,
        contacts.first_name,
        contacts.last_name,
        clients.loyalty_type_id
        FROM companies
        LEFT JOIN
        entities ON entities.id = companies.entity_id
        LEFT JOIN
        phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
        LEFT JOIN
        phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
        LEFT JOIN
        email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
        LEFT JOIN
        email_addresses ON email_addresses.id = email_addr_entity_rel.email_address_id
        LEFT JOIN
        contacts ON contacts.id = companies.primary_contact_id
        LEFT JOIN
        clients ON clients.company_id = companies.id
        WHERE ( email_addr_entity_rel.is_primary OR email_addr_entity_rel.is_primary IS NULL )
        AND ( phone_number_entity_rel.is_primary OR phone_number_entity_rel.is_primary IS NULL )
        AND clients.company_id IS NOT NULL
        AND clients.date_deleted IS NULL
        AND companies.id = @companyId
        GROUP BY companies.id
        ORDER BY companies.name
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@companyId", companyId);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        client = new Client(
          GetSafeString(reader, "id"),
          GetSafeString(reader, "entity_id"),
          GetSafeString(reader, "name"),
          GetSafeInt(reader, "loyalty_type_id"),
          GetSafeString(reader, "street_address"),
          GetSafeString(reader, "city"),
          GetSafeString(reader, "state"),
          GetSafeString(reader, "postal_code"),
          GetSafeString(reader, "email_address_id"),
          GetSafeString(reader, "email_address"),
          GetSafeString(reader, "phone_number_id"),
          GetUnsafeULong(reader, "phone_number"),
          GetUnsafeUInt(reader, "extension"),
          GetSafeString(reader, "primary_contact_id"),
          GetSafeString(reader, "first_name"),
          GetSafeString(reader, "last_name")
        );
      }
      reader.Close();
      CloseConnection(Connection);
      return client;
    }

    public string GetCompanyPrimaryContactName(string companyId)
    {
      string query =
        @"
        SELECT first_name, last_name FROM contacts
        LEFT JOIN companies ON companies.id = contacts.company_id
        WHERE contacts.company_id = @companyId
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@companyId", companyId);
      MySqlDataReader reader = command.ExecuteReader();
      string name = "";
      if (reader.Read())
      {
        name = GetSafeString(reader, "first_name") + " " + GetSafeString(reader, "last_name");
      }
      reader.Close();
      CloseConnection(Connection);
      return name;
    }

    public string GetCompanyName(string companyId)
    {
      string query =
        @"
        SELECT name FROM companies        
        WHERE id = @companyId
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@companyId", companyId);
      MySqlDataReader reader = command.ExecuteReader();
      string name = "";
      if (reader.Read())
      {
        name = GetSafeString(reader, "name");
      }
      reader.Close();
      CloseConnection(Connection);
      return name;
    }

    public void SaveCompany(Company company)
    {
      string query =
        @"
        UPDATE companies SET
        name = @name,
        street_address = @streetAddress,
        city = @city,
        state = @state,
        postal_code = @postalCode
        WHERE id = @id
        ";
      ;
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@name", company.CompanyName);
      command.Parameters.AddWithValue("@streetAddress", company.StreetAddress);
      command.Parameters.AddWithValue("@city", company.City);
      command.Parameters.AddWithValue("@state", company.State);
      command.Parameters.AddWithValue("@postalCode", company.PostalCode);
      command.Parameters.AddWithValue("@id", company.CompanyId);
      command.ExecuteNonQuery();

      if (company.NewEmailAddress)
      {
        string emailAddressId = Guid.NewGuid().ToString();
        string emailAddressRelId = Guid.NewGuid().ToString();
        query =
          @"
                    INSERT INTO email_addresses (id, email_address)
                    VALUES (@id, @emailAddress)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", emailAddressId);
        command.Parameters.AddWithValue("@emailAddress", company.CompanyEmail);
        command.ExecuteNonQuery();

        query =
          @"
          UPDATE email_addr_entity_rel SET
          is_primary = 0 WHERE entity_id = @id
          ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", company.EntityId);
        command.ExecuteNonQuery();

        query =
          @"
                    INSERT INTO email_addr_entity_rel (id, email_address_id, entity_id, is_primary)
                    VALUES (@id, @emailAddressId, @entityId, 1)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", emailAddressRelId);
        command.Parameters.AddWithValue("@emailAddressId", emailAddressId);
        command.Parameters.AddWithValue("@entityId", company.EntityId);
        command.ExecuteNonQuery();
        company.CompanyEmailId = emailAddressId;
        company.NewEmailAddress = false;
      }
      else
      {
        query =
          @"
                    UPDATE email_addresses
                    SET email_address = @emailAddress
                    WHERE id = @emailAddressId
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@emailAddress", company.CompanyEmail);
        command.Parameters.AddWithValue("@emailAddressId", company.CompanyEmailId);
        command.ExecuteNonQuery();
      }
      if (company.NewPhoneNumber)
      {
        string phoneNumberId = Guid.NewGuid().ToString();
        string phoneNumberRelId = Guid.NewGuid().ToString();
        query =
          @"
                    INSERT INTO phone_numbers (id, phone_number, extension, calling_code)
                    VALUES (@id, @phoneNumber, @extension, 1)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", phoneNumberId);
        command.Parameters.AddWithValue("@phoneNumber", company.CompanyPhone);
        command.Parameters.AddWithValue(
          "@extension",
          company.CompanyExtension == 0 ? null : company.CompanyExtension
        );
        command.ExecuteNonQuery();

        query =
          @"
          UPDATE phone_number_entity_rel SET
          is_primary = 0 WHERE entity_id = @id
          ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", company.EntityId);
        command.ExecuteNonQuery();

        query =
          @"
                    INSERT INTO phone_number_entity_rel (id, phone_number_id, entity_id, is_primary)
                    VALUES (@id, @phoneNumberId, @entityId, 1)
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", phoneNumberRelId);
        command.Parameters.AddWithValue("@phoneNumberId", phoneNumberId);
        command.Parameters.AddWithValue("@entityId", company.EntityId);
        command.ExecuteNonQuery();
        company.CompanyPhoneId = phoneNumberId;
        company.NewPhoneNumber = false;
      }
      else
      {
        query =
          @"
                    UPDATE phone_numbers
                    SET phone_number = @phoneNumber
                    WHERE id = @phoneNumberId
                    ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@phoneNumber", company.CompanyPhone);
        command.Parameters.AddWithValue("@phoneNumberId", company.CompanyPhoneId);
        command.ExecuteNonQuery();
      }
    }

    public void SaveClient(Client client)
    {
      if (client.New)
      {
        CreateCompany(client);
        CreateClient(client);
      }
      else
      {
        SaveCompany(client);
      }

      string query =
        @"
        UPDATE clients SET
        loyalty_type_id = @loyalty_type_id WHERE company_id = @company_id
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@company_id", client.CompanyId);
      command.Parameters.AddWithValue("@loyalty_type_id", client.LoyaltyTypeId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public void DeleteClient(Client client)
    {
      OpenConnection(Connection);
      string query =
        @"
        UPDATE clients SET date_deleted = current_timestamp() WHERE company_id = @company_id
        ";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@company_id", client.CompanyId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public void DeleteArchitect(Architect architect)
    {
      OpenConnection(Connection);
      string query =
        @"
        UPDATE architects SET date_deleted = current_timestamp() WHERE company_id = @company_id
        ";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@company_id", architect.CompanyId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public List<Architect> GetArchitects()
    {
      List<Architect> architects = new List<Architect>();
      string query =
        @"
        SELECT 
        companies.id,
        companies.entity_id,
        companies.name,
        companies.street_address,
        companies.city,
        companies.state,
        companies.postal_code,
        companies.primary_contact_id,
        phone_numbers.phone_number,
        phone_numbers.extension,
        phone_numbers.id as phone_number_id,
        email_addresses.email_address,
        email_addresses.id as email_address_id,
        contacts.id as primary_contact_id,
        contacts.first_name,
        contacts.last_name
        FROM companies
        LEFT JOIN
        entities ON entities.id = companies.entity_id
        LEFT JOIN
        phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
        LEFT JOIN
        phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
        LEFT JOIN
        email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
        LEFT JOIN
        email_addresses ON email_addresses.id = email_addr_entity_rel.email_address_id
        LEFT JOIN
        contacts ON contacts.id = companies.primary_contact_id
        LEFT JOIN
        architects ON architects.company_id = companies.id
        WHERE ( email_addr_entity_rel.is_primary OR email_addr_entity_rel.is_primary IS NULL )
        AND ( phone_number_entity_rel.is_primary OR phone_number_entity_rel.is_primary IS NULL )
        AND architects.company_id IS NOT NULL
        AND architects.date_deleted IS NULL
        GROUP BY companies.id
        ORDER BY companies.name
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        architects.Add(
          new Architect(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "entity_id"),
            GetSafeString(reader, "name"),
            GetSafeString(reader, "street_address"),
            GetSafeString(reader, "city"),
            GetSafeString(reader, "state"),
            GetSafeString(reader, "postal_code"),
            GetSafeString(reader, "email_address_id"),
            GetSafeString(reader, "email_address"),
            GetSafeString(reader, "phone_number_id"),
            GetUnsafeULong(reader, "phone_number"),
            GetUnsafeUInt(reader, "extension"),
            GetSafeString(reader, "primary_contact_id"),
            GetSafeString(reader, "first_name"),
            GetSafeString(reader, "last_name")
          )
        );
      }
      reader.Close();
      CloseConnection(Connection);
      return architects;
    }

    public Architect? GetArchitect(string companyId)
    {
      Architect? architect = null;
      string query =
        @"
        SELECT 
        companies.id,
        companies.entity_id,
        companies.name,
        companies.street_address,
        companies.city,
        companies.state,
        companies.postal_code,
        companies.primary_contact_id,
        phone_numbers.phone_number,
        phone_numbers.extension,
        phone_numbers.id as phone_number_id,
        email_addresses.email_address,
        email_addresses.id as email_address_id,
        contacts.id as primary_contact_id,
        contacts.first_name,
        contacts.last_name
        FROM companies
        LEFT JOIN
        entities ON entities.id = companies.entity_id
        LEFT JOIN
        phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
        LEFT JOIN
        phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
        LEFT JOIN
        email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
        LEFT JOIN
        email_addresses ON email_addresses.id = email_addr_entity_rel.email_address_id
        LEFT JOIN
        contacts ON contacts.id = companies.primary_contact_id
        LEFT JOIN
        architects ON architects.company_id = companies.id
        WHERE ( email_addr_entity_rel.is_primary OR email_addr_entity_rel.is_primary IS NULL )
        AND ( phone_number_entity_rel.is_primary OR phone_number_entity_rel.is_primary IS NULL )
        AND architects.company_id IS NOT NULL
        AND architects.date_deleted IS NULL
        AND companies.id = @companyId
        GROUP BY companies.id
        ORDER BY companies.name
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@companyId", companyId);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        architect = new Architect(
          GetSafeString(reader, "id"),
          GetSafeString(reader, "entity_id"),
          GetSafeString(reader, "name"),
          GetSafeString(reader, "street_address"),
          GetSafeString(reader, "city"),
          GetSafeString(reader, "state"),
          GetSafeString(reader, "postal_code"),
          GetSafeString(reader, "email_address_id"),
          GetSafeString(reader, "email_address"),
          GetSafeString(reader, "phone_number_id"),
          GetUnsafeULong(reader, "phone_number"),
          GetUnsafeUInt(reader, "extension"),
          GetSafeString(reader, "primary_contact_id"),
          GetSafeString(reader, "first_name"),
          GetSafeString(reader, "last_name")
        );
      }
      reader.Close();
      CloseConnection(Connection);
      return architect;
    }

    public void SaveArchitect(Architect architect)
    {
      if (architect.New)
      {
        CreateCompany(architect);
        CreateArchitect(architect);
      }
      else
      {
        SaveCompany(architect);
      }
    }

    public List<Contact> GetContacts(string companyId = "")
    {
      List<Contact> contacts = new List<Contact>();
      string query =
        @"
        SELECT 
        contacts.id,
        contacts.entity_id,
        contacts.first_name,
        contacts.last_name,
        contacts.company_id,
        phone_numbers.phone_number,
        phone_numbers.extension,
        phone_numbers.id as phone_number_id,
        email_addresses.email_address,
        email_addresses.id as email_address_id,
        companies.name
        FROM contacts
        LEFT JOIN
        entities ON entities.id = contacts.entity_id
        LEFT JOIN
        phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
        LEFT JOIN
        phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
        LEFT JOIN
        email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
        LEFT JOIN
        email_addresses ON email_addresses.id = email_addr_entity_rel.email_address_id
        LEFT JOIN
        companies ON companies.id = contacts.company_id
        LEFT JOIN
        clients ON clients.company_id = companies.id
        LEFT JOIN
        architects ON architects.company_id = companies.id
        WHERE ( email_addr_entity_rel.is_primary OR email_addr_entity_rel.is_primary IS NULL )
        AND ( phone_number_entity_rel.is_primary OR phone_number_entity_rel.is_primary IS NULL )
        AND ( clients.date_deleted IS NULL OR architects.date_deleted IS NULL )
        AND contacts.date_deleted IS NULL
        AND ( clients.company_id IS NOT NULL OR architects.company_id IS NOT NULL )
      ";
      if (!String.IsNullOrEmpty(companyId))
      {
        query += "AND contacts.company_id = @company_id";
      }
      query +=
        @"
        GROUP BY contacts.id
        ORDER BY contacts.last_name
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      if (!String.IsNullOrEmpty(companyId))
      {
        command.Parameters.AddWithValue("@company_id", companyId);
      }
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        contacts.Add(
          new Contact(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "entity_id"),
            GetSafeString(reader, "first_name"),
            GetSafeString(reader, "last_name"),
            GetSafeString(reader, "company_id"),
            GetSafeString(reader, "name"),
            GetSafeString(reader, "email_address_id"),
            GetSafeString(reader, "email_address"),
            GetSafeString(reader, "phone_number_id"),
            GetUnsafeULong(reader, "phone_number"),
            GetUnsafeUInt(reader, "extension")
          )
        );
      }
      reader.Close();
      CloseConnection(Connection);

      return contacts;
    }

    public string GetContactCompanyIdByEmail(string email)
    {
      string query =
        @"
        SELECT
        contacts.company_id
        FROM contacts
        LEFT JOIN entities ON entities.id = contacts.entity_id
        LEFT JOIN email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
        LEFT JOIN email_addresses ON email_addresses.id = email_addr_entity_rel.email_address_id
        WHERE email_addresses.email_address = @email
        ";
      string companyId = string.Empty;
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@email", email);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        companyId = GetSafeString(reader, "company_id");
      }
      reader.Close();
      CloseConnection(Connection);
      return companyId;
    }

    public void CreateContact(Contact contact)
    {
      string query = @"INSERT INTO entities ( id ) VALUES ( @entityId )";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@entityId", contact.EntityId);
      command.ExecuteNonQuery();
      query =
        @"
        INSERT INTO contacts
        ( id,  entity_id,  first_name,  last_name,  company_id) VALUES
        (@id, @entity_id, @first_name, @last_name, @company_id)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", contact.Id);
      command.Parameters.AddWithValue("@entity_id", contact.EntityId);
      command.Parameters.AddWithValue("@first_name", contact.FirstName);
      command.Parameters.AddWithValue("@last_name", contact.LastName);
      command.Parameters.AddWithValue("@company_id", contact.CompanyId);
      command.ExecuteNonQuery();

      string emailAddressRelId = Guid.NewGuid().ToString();
      query =
        @"
        INSERT INTO email_addresses (id, email_address)
        VALUES (@id, @emailAddress)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", contact.EmailAddressId);
      command.Parameters.AddWithValue("@emailAddress", contact.EmailAddress);
      command.ExecuteNonQuery();

      query =
        @"
        UPDATE email_addr_entity_rel SET
        is_primary = 0 WHERE entity_id = @id
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", contact.EntityId);
      command.ExecuteNonQuery();

      query =
        @"
        INSERT INTO email_addr_entity_rel (id, email_address_id, entity_id, is_primary)
        VALUES (@id, @emailAddressId, @entityId, 1)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", emailAddressRelId);
      command.Parameters.AddWithValue("@emailAddressId", contact.EmailAddressId);
      command.Parameters.AddWithValue("@entityId", contact.EntityId);
      command.ExecuteNonQuery();

      string phoneNumberRelId = Guid.NewGuid().ToString();
      query =
        @"
        INSERT INTO phone_numbers (id, phone_number, extension, calling_code)
        VALUES (@id, @phoneNumber, @extension, 1)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", contact.PhoneNumberId);
      command.Parameters.AddWithValue("@phoneNumber", contact.PhoneNumber);
      command.Parameters.AddWithValue(
        "@extension",
        contact.Extension == 0 ? null : contact.Extension
      );
      command.ExecuteNonQuery();

      query =
        @"
        UPDATE phone_number_entity_rel SET
        is_primary = 0 WHERE entity_id = @id
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", contact.EntityId);
      command.ExecuteNonQuery();

      query =
        @"
        INSERT INTO phone_number_entity_rel (id, phone_number_id, entity_id, is_primary)
        VALUES (@id, @phoneNumberId, @entityId, 1)
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", phoneNumberRelId);
      command.Parameters.AddWithValue("@phoneNumberId", contact.PhoneNumberId);
      command.Parameters.AddWithValue("@entityId", contact.EntityId);
      command.ExecuteNonQuery();

      CloseConnection(Connection);
    }

    public void SaveContact(Contact contact)
    {
      if (contact.New)
      {
        CreateContact(contact);
        return;
      }
      string query =
        @"
        UPDATE contacts SET
        first_name = @first_name,
        last_name = @last_name
        WHERE id = @id
        ";

      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@first_name", contact.FirstName);
      command.Parameters.AddWithValue("@last_name", contact.LastName);
      command.Parameters.AddWithValue("@id", contact.Id);
      command.ExecuteNonQuery();

      query =
        @"
        UPDATE email_addresses SET
        email_address = @email_address
        WHERE id = @id
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@email_address", contact.EmailAddress);
      command.Parameters.AddWithValue("@id", contact.EmailAddressId);
      command.ExecuteNonQuery();

      query =
        @"
        UPDATE phone_numbers SET
        phone_number = @phone_number,
        extension = @extension
        WHERE id = @id
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@phone_number", contact.PhoneNumber);
      command.Parameters.AddWithValue("@extension", contact.Extension);
      command.Parameters.AddWithValue("@id", contact.PhoneNumberId);
      command.ExecuteNonQuery();

      CloseConnection(Connection);
    }

    public void DeleteContact(Contact contact)
    {
      string query =
        @"
        UPDATE contacts SET date_deleted = current_timestamp() WHERE id = @id
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", contact.Id);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public void SetPrimaryContact(Contact contact)
    {
      if (String.IsNullOrEmpty(contact.CompanyId))
      {
        return;
      }
      string query =
        @"
        UPDATE companies SET
        primary_contact_id = @primary_contact_id
        WHERE id = @id
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@primary_contact_id", contact.Id);
      command.Parameters.AddWithValue("@id", contact.CompanyId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public string CreateBlankProject()
    {
      var id = Guid.NewGuid().ToString();
      var projectNo = "n" + id.Substring(0, 6);
      OpenConnection(Connection);
      string query = "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", id);
      command.Parameters.AddWithValue("@projectNo", projectNo);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
      return id;
    }

    public async Task<Dictionary<int, string>> GetProjectIds(string projectNo)
    {
      string query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectNo", projectNo);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

      Dictionary<int, string> projectIds = new Dictionary<int, string>();
      while (reader.Read())
      {
        projectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
      }
      await reader.CloseAsync();
      try
      {
        if (!projectIds.Any())
        {
          // Project name does not exist, insert a new entry with a generated ID
          var id = Guid.NewGuid().ToString();
          string insertQuery =
            "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
          MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
          insertCommand.Parameters.AddWithValue("@id", id);
          insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
          await insertCommand.ExecuteNonQueryAsync();

          insertQuery = "INSERT INTO electrical_projects (id, project_id) VALUES (@id, @projectId)";
          insertCommand = new MySqlCommand(insertQuery, Connection);
          insertCommand.Parameters.AddWithValue("@id", id);
          insertCommand.Parameters.AddWithValue("@projectId", id);
          await insertCommand.ExecuteNonQueryAsync();
          projectIds.Add(1, id);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Insert failed: {ex.Message}");
        return projectIds;
      }

      await CloseConnectionAsync(Connection);
      projectIds = projectIds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
      return projectIds;
    }

    public string GetComparableProject(
      string street_address,
      string postal_code,
      string compareWithProjectNo
    )
    {
      if (string.IsNullOrEmpty(street_address) || string.IsNullOrEmpty(postal_code))
      {
        return string.Empty;
      }
      string gmep_project_no = string.Empty;
      string streetAddressSubstring = street_address;
      if (street_address.Length > 4)
      {
        streetAddressSubstring = street_address.Substring(0, 4);
      }
      string query =
        @"SELECT projects.gmep_project_no FROM projects
        LEFT JOIN proposals ON proposals.project_id = projects.id
        WHERE street_address LIKE @streetAddressSubstring
        AND proposals.status_id = 1
        AND postal_code = @postal_code
        AND projects.gmep_project_no <> @compareWithProjectNo
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@streetAddressSubstring", streetAddressSubstring + "%");
      command.Parameters.AddWithValue("@postal_code", postal_code);
      command.Parameters.AddWithValue("@compareWithProjectNo", compareWithProjectNo);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        gmep_project_no = GetSafeString(reader, "gmep_project_no");
      }
      reader.Close();
      CloseConnection(Connection);
      return gmep_project_no;
    }

    public string GetLatestElectricalProjectId(string projectId)
    {
      string query =
        @"
      SELECT id FROM electrical_projects WHERE project_id = @project_id ORDER BY version DESC LIMIT 1 
      ";
      string id = string.Empty;
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@project_id", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        id = GetSafeString(reader, "id");
      }
      reader.Close();
      CloseConnection(Connection);
      return id;
    }

    public string GetActiveElectricalProjectId(string projectId, int version)
    {
      string query =
        @"
      SELECT id FROM electrical_projects WHERE project_id = @project_id AND version = @version ORDER BY version DESC LIMIT 1 
      ";
      string id = string.Empty;
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@version", version);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        id = GetSafeString(reader, "id");
      }
      reader.Close();
      CloseConnection(Connection);
      return id;
    }

    public void CreateElectricalProject(string projectId)
    {
      string query =
        @"
        INSERT IGNORE INTO electrical_projects
        ( id,  project_id) VALUES
        (@id, @project_id)
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", projectId);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public async Task<Dictionary<int, string>> AddElectricalProjectVersions(string projectId)
    {
      string query = "SELECT id, version FROM electrical_projects WHERE project_id = @projectId";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

      Dictionary<int, string> electricalProjectIds = new Dictionary<int, string>();
      while (await reader.ReadAsync())
      {
        electricalProjectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
      }
      await reader.CloseAsync();

      if (!electricalProjectIds.Any())
      {
        // Project name does not exist, insert a new entry with a generated ID
        var id = Guid.NewGuid().ToString();
        string insertQuery =
          "INSERT INTO electrical_projects (id, project_id) VALUES (@id, @projectId)";
        MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
        insertCommand.Parameters.AddWithValue("@id", id);
        insertCommand.Parameters.AddWithValue("@projectId", projectId);
        await insertCommand.ExecuteNonQueryAsync();
        electricalProjectIds.Add(1, id);
      }
      else
      {
        var id = Guid.NewGuid().ToString();
        electricalProjectIds = electricalProjectIds
          .OrderBy(x => x.Key)
          .ToDictionary(x => x.Key, x => x.Value);
        string insertQuery =
          "INSERT INTO electrical_projects (id, project_id, version) VALUES (@id, @projectId, @version)";
        MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
        insertCommand.Parameters.AddWithValue("@id", id);
        insertCommand.Parameters.AddWithValue("@projectId", projectId);
        insertCommand.Parameters.AddWithValue("@version", electricalProjectIds.Last().Key + 1);
        await insertCommand.ExecuteNonQueryAsync();
        await CloneElectricalProject(projectId, electricalProjectIds.Last().Value, id);
        electricalProjectIds.Add(electricalProjectIds.Last().Key + 1, id);
      }

      CloseConnection(Connection);
      return electricalProjectIds;
    }

    public async Task<Dictionary<int, string>> DeleteElectricalProjectVersions(
      string electricalProjectId,
      string projectId
    )
    {
      await OpenConnectionAsync(Connection);
      List<string> tables = GetElectricalTables();
      string query;
      MySqlCommand command;
      foreach (string table in tables)
      {
        query = $"DELETE FROM {table} WHERE electrical_project_id = @electricalProjectId";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
        await command.ExecuteNonQueryAsync();
      }

      query = "DELETE FROM electrical_projects WHERE id = @electricalProjectId";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      await command.ExecuteNonQueryAsync();

      query = "SELECT id, version FROM electrical_projects WHERE project_id = @project_id";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@project_id", projectId);
      Dictionary<int, string> electricalProjectIds = new Dictionary<int, string>();
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (reader.Read())
      {
        electricalProjectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
      }
      await reader.CloseAsync();

      if (!electricalProjectIds.Any())
      {
        // Project name does not exist, insert a new entry with a generated ID
        var id = Guid.NewGuid().ToString();
        string insertQuery =
          "INSERT INTO electrical_projects (id, project_id) VALUES (@id, @projectId)";
        MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
        insertCommand.Parameters.AddWithValue("@id", id);
        insertCommand.Parameters.AddWithValue("@projectId", projectId);
        await insertCommand.ExecuteNonQueryAsync();
        electricalProjectIds.Add(1, id);
      }
      await CloseConnectionAsync(Connection);
      return electricalProjectIds;
    }

    public async Task<Dictionary<string, string>> getOwners()
    {
      var owners = new Dictionary<string, string>();

      try
      {
        await OpenConnectionAsync(Connection);

        string query = "SELECT id, name FROM owners";

        using (MySqlCommand cmd = new MySqlCommand(query, Connection))
        {
          using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
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
        await CloseConnectionAsync(Connection);
      }

      return owners;
    }

    //Update Project Functions
    public async Task UpdateProject(
      string projectId,
      string electricalProjectId,
      ObservableCollection<ElectricalService> services,
      ObservableCollection<ElectricalPanel> panels,
      ObservableCollection<ElectricalEquipment> equipments,
      ObservableCollection<ElectricalTransformer> transformers,
      ObservableCollection<ElectricalLighting> lightings,
      ObservableCollection<ElectricalLightingControl> lightingControls,
      ObservableCollection<Location> locations,
      ObservableCollection<ElectricalPanelNote> electricalPanelNotes,
      ObservableCollection<ElectricalPanelNoteRel> electricalPanelNoteRels,
      ObservableCollection<Circuit> customCircuits,
      ObservableCollection<TimeClock> timeClocks
    )
    {
      await OpenConnectionAsync(Connection);
      await UpdateServices(projectId, electricalProjectId, services);
      await UpdatePanels(projectId, electricalProjectId, panels);
      await UpdateEquipments(projectId, electricalProjectId, equipments);
      await UpdateTransformers(projectId, electricalProjectId, transformers);
      await UpdateLightings(projectId, electricalProjectId, lightings);
      await UpdateLightingControls(projectId, electricalProjectId, lightingControls);
      await UpdateLightingLocations(projectId, electricalProjectId, locations);
      await UpdateElectricalPanelNotes(electricalProjectId, electricalPanelNotes);
      await UpdateElectricalPanelNoteRels(electricalProjectId, electricalPanelNoteRels);
      await UpdateCustomCircuits(projectId, electricalProjectId, customCircuits);
      await UpdateTimeClocks(projectId, electricalProjectId, timeClocks);

      await CloseConnectionAsync(Connection);
    }

    private async Task UpdateServices(
      string projectId,
      string electricalProjectId,
      ObservableCollection<ElectricalService> services
    )
    {
      var existingServiceIds = await GetExistingIds(
        "electrical_services",
        "electrical_project_id",
        electricalProjectId
      );

      foreach (var service in services)
      {
        if (existingServiceIds.Contains(service.Id))
        {
          await UpdateService(service);
          existingServiceIds.Remove(service.Id);
        }
        else
        {
          await InsertService(projectId, electricalProjectId, service);
        }
      }

      await DeleteRemovedItems("electrical_services", existingServiceIds);
    }

    private async Task UpdatePanels(
      string projectId,
      string electricalProjectId,
      ObservableCollection<ElectricalPanel> panels
    )
    {
      var existingPanelIds = await GetExistingIds(
        "electrical_panels",
        "electrical_project_id",
        electricalProjectId
      );

      foreach (var panel in panels)
      {
        if (existingPanelIds.Contains(panel.Id))
        {
          await UpdatePanel(panel);
          existingPanelIds.Remove(panel.Id);
        }
        else
        {
          await InsertPanel(projectId, electricalProjectId, panel);
        }
      }

      await DeleteRemovedItems("electrical_panels", existingPanelIds);
    }

    private async Task UpdateElectricalPanelNotes(
      string electricalProjectId,
      ObservableCollection<ElectricalPanelNote> panelNotes
    )
    {
      var existingPanelIds = await GetExistingIds(
        "electrical_panel_notes",
        "electrical_project_id",
        electricalProjectId
      );

      var panelNotesCopy = panelNotes.ToList(); // Create a copy of the collection

      foreach (var note in panelNotesCopy)
      {
        if (existingPanelIds.Contains(note.Id))
        {
          await UpdateElectricalPanelNote(note);
          existingPanelIds.Remove(note.Id);
        }
        else
        {
          await InsertElectricalPanelNote(note);
        }
      }

      await DeleteRemovedItems("electrical_panel_notes", existingPanelIds);
    }

    private async Task UpdateElectricalPanelNoteRels(
      string electricalProjectId,
      ObservableCollection<ElectricalPanelNoteRel> noteRels
    )
    {
      var existingNoteRelIds = await GetExistingIds(
        "electrical_panel_note_panel_rel",
        "electrical_project_id",
        electricalProjectId
      );
      var noteRelsCopy = noteRels.ToList(); // Create a copy of the collection

      foreach (var note in noteRelsCopy)
      {
        if (existingNoteRelIds.Contains(note.Id))
        {
          await UpdateElectricalPanelNoteRel(note);
          existingNoteRelIds.Remove(note.Id);
        }
        else
        {
          await InsertElectricalPanelNoteRel(note);
        }
      }
      await DeleteRemovedItems("electrical_panel_note_panel_rel", existingNoteRelIds);
    }

    private async Task UpdateCustomCircuits(
      string projectId,
      string electricalProjectId,
      ObservableCollection<Circuit> customCircuits
    )
    {
      var existingPanelIds = await GetExistingIds(
        "electrical_panel_custom_circuits",
        "electrical_project_id",
        electricalProjectId
      );

      var customCircuitsCopy = customCircuits.ToList(); // Create a copy of the collection

      foreach (var circuit in customCircuitsCopy)
      {
        if (existingPanelIds.Contains(circuit.Id))
        {
          await UpdateCustomCircuit(circuit);
          existingPanelIds.Remove(circuit.Id);
        }
        else
        {
          await InsertCustomCircuit(projectId, electricalProjectId, circuit);
        }
      }

      await DeleteRemovedItems("electrical_panel_custom_circuits", existingPanelIds);
    }

    private async Task UpdateTransformers(
      string projectId,
      string electricalProjectId,
      ObservableCollection<ElectricalTransformer> transformers
    )
    {
      var existingTransformerIds = await GetExistingIds(
        "electrical_transformers",
        "electrical_project_id",
        electricalProjectId
      );

      foreach (var transformer in transformers)
      {
        if (existingTransformerIds.Contains(transformer.Id))
        {
          await UpdateTransformer(transformer);
          existingTransformerIds.Remove(transformer.Id);
        }
        else
        {
          await InsertTransformer(projectId, electricalProjectId, transformer);
        }
      }

      await DeleteRemovedItems("electrical_transformers", existingTransformerIds);
    }

    private async Task UpdateEquipments(
      string projectId,
      string electricalProjectId,
      ObservableCollection<ElectricalEquipment> equipments
    )
    {
      var existingEquipmentIds = await GetExistingIds(
        "electrical_equipment",
        "electrical_project_id",
        electricalProjectId
      );

      foreach (var equipment in equipments)
      {
        if (existingEquipmentIds.Contains(equipment.Id))
        {
          await UpdateEquipment(equipment);
          existingEquipmentIds.Remove(equipment.Id);
        }
        else
        {
          await InsertEquipment(projectId, electricalProjectId, equipment);
        }
      }

      await DeleteRemovedItems("electrical_equipment", existingEquipmentIds);
    }

    private async Task UpdateLightings(
      string projectId,
      string electricalProjectId,
      ObservableCollection<ElectricalLighting> lightings
    )
    {
      var existingLightingIds = await GetExistingIds(
        "electrical_lighting",
        "electrical_project_id",
        electricalProjectId
      );

      foreach (var lighting in lightings)
      {
        if (existingLightingIds.Contains(lighting.Id))
        {
          await UpdateLighting(lighting);
          existingLightingIds.Remove(lighting.Id);
        }
        else
        {
          await InsertLighting(projectId, electricalProjectId, lighting);
        }
      }

      await DeleteRemovedItems("electrical_lighting", existingLightingIds);
    }

    private async Task UpdateLightingControls(
      string projectId,
      string electricalProjectId,
      ObservableCollection<ElectricalLightingControl> controls
    )
    {
      var existingLightingControlIds = await GetExistingIds(
        "electrical_lighting_controls",
        "electrical_project_id",
        electricalProjectId
      );

      foreach (var control in controls)
      {
        if (existingLightingControlIds.Contains(control.Id))
        {
          await UpdateLightingControl(control);
          existingLightingControlIds.Remove(control.Id);
        }
        else
        {
          await InsertLightingControl(projectId, electricalProjectId, control);
        }
      }

      await DeleteRemovedItems("electrical_lighting_controls", existingLightingControlIds);
    }

    private async Task UpdateLightingLocations(
      string projectId,
      string electricalProjectId,
      ObservableCollection<Location> locations
    )
    {
      var existingLocationIds = await GetExistingIds(
        "electrical_lighting_locations",
        "electrical_project_id",
        electricalProjectId
      );

      foreach (var location in locations)
      {
        if (existingLocationIds.Contains(location.Id))
        {
          await UpdateLocation(location);
          existingLocationIds.Remove(location.Id);
        }
        else
        {
          await InsertLocation(projectId, electricalProjectId, location);
        }
      }

      await DeleteRemovedItems("electrical_lighting_locations", existingLocationIds);
    }

    private async Task UpdateTimeClocks(
      string projectId,
      string electricalProjectId,
      ObservableCollection<TimeClock> clocks
    )
    {
      var existingLocationIds = await GetExistingIds(
        "electrical_lighting_timeclocks",
        "electrical_project_id",
        electricalProjectId
      );
      foreach (var clock in clocks)
      {
        if (existingLocationIds.Contains(clock.Id))
        {
          await UpdateClock(clock);
          existingLocationIds.Remove(clock.Id);
        }
        else
        {
          await InsertClock(projectId, electricalProjectId, clock);
        }
      }
      await DeleteRemovedItems("electrical_lighting_timeclocks", existingLocationIds);
    }

    private async Task<HashSet<string>> GetExistingIds(
      string tableName,
      string columnName,
      string disciplineProjectId
    )
    {
      var idType = "id";

      string query = $"SELECT {idType} FROM {tableName} WHERE {columnName} = @disciplineProjectId";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@disciplineProjectId", disciplineProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      HashSet<string> ids = new HashSet<string>();
      while (await reader.ReadAsync())
      {
        ids.Add(reader.GetString($"{idType}"));
      }
      await reader.CloseAsync();
      return ids;
    }

    private async Task UpdateService(ElectricalService service)
    {
      string query =
        "UPDATE electrical_services SET name = @name, order_no = @order_no, electrical_service_amp_rating_id = @amp, electrical_service_voltage_id = @type, electrical_service_meter_config_id = @config, color_code = @color_code, aic_rating = @aicRating, parent_id = @parentId WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@name", service.Name);
      command.Parameters.AddWithValue("@amp", service.Amp);
      command.Parameters.AddWithValue("@id", service.Id);
      command.Parameters.AddWithValue("@type", service.Type);
      command.Parameters.AddWithValue("@config", service.Config);
      command.Parameters.AddWithValue("@color_code", service.ColorCode);
      command.Parameters.AddWithValue("@aicRating", service.AicRating);
      command.Parameters.AddWithValue("@parentId", service.ParentId);
      command.Parameters.AddWithValue("@order_no", service.OrderNo);
      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertService(
      string projectId,
      string electricalProjectId,
      ElectricalService service
    )
    {
      string query =
        @"
INSERT INTO electrical_services
( id,  project_id,  electrical_project_id,  name,  electrical_service_amp_rating_id,  electrical_service_voltage_id,  electrical_service_meter_config_id,  color_code,  aic_rating,  parent_id,  order_no) VALUES
(@id, @project_id, @electrical_project_id, @name, @electrical_service_amp_rating_id, @electrical_service_voltage_id, @electrical_service_meter_config_id, @color_code, @aic_rating, @parent_id, @order_no)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", service.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@name", service.Name);
      command.Parameters.AddWithValue("@electrical_service_amp_rating", service.Amp);
      command.Parameters.AddWithValue("@electrical_service_voltage_id", service.Type);
      command.Parameters.AddWithValue("@electrical_service_meter_config", service.Config);
      command.Parameters.AddWithValue("@color_code", service.ColorCode);
      command.Parameters.AddWithValue("@aic_rating", service.AicRating);
      command.Parameters.AddWithValue("@parent_id", service.ParentId);
      command.Parameters.AddWithValue("@order_no", service.OrderNo);
      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdatePanel(ElectricalPanel panel)
    {
      string query =
        "UPDATE electrical_panels SET bus_amp_rating_id = @bus, main_amp_rating_id = @main, order_no = @order_no, is_distribution = @is_distribution, voltage_id = @type, num_breakers = @numBreakers, parent_distance = @distanceFromParent, aic_rating = @aicRating, name = @name, color_code = @color_code, parent_id = @parent_id, is_recessed = @is_recessed, is_mlo = @is_mlo, circuit_no = @circuit_no, is_hidden_on_plan = @is_hidden_on_plan, location = @location, high_leg_phase = @highLegPhase, load_amperage = @amp, kva = @kva, status_id = @statusId WHERE id = @id";
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
      command.Parameters.AddWithValue("@highLegPhase", panel.HighLegPhase);
      command.Parameters.AddWithValue("@amp", panel.Amp);
      command.Parameters.AddWithValue("@kva", panel.Kva);
      command.Parameters.AddWithValue("@order_no", panel.OrderNo);
      command.Parameters.AddWithValue("@statusId", panel.StatusId);
      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertPanel(
      string projectId,
      string electricalProjectId,
      ElectricalPanel panel
    )
    {
      string query =
        @"INSERT INTO electrical_panels
( id,  project_id,  electrical_project_id,  bus_amp_rating_id,  main_amp_rating_id,  is_distribution,  name,  color_code,  parent_id,  num_breakers,  parent_distance,  aic_rating,  voltage_id,  is_recessed,  is_mlo,  circuit_no,  is_hidden_on_plan,  location,  high_leg_phase,  load_amperage,  kva,  order_no) VALUES
(@id, @project_id, @electrical_project_id, @bus_amp_rating_id, @main_amp_rating_id, @is_distribution, @name, @color_code, @parent_id, @num_breakers, @parent_distance, @aic_rating, @voltage_id, @is_recessed, @is_mlo, @circuit_no, @is_hidden_on_plan, @location, @high_leg_phase, @load_amperage, @kva, @order_no)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", panel.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@bus_amp_rating_id", panel.BusSize);
      command.Parameters.AddWithValue("@main_amp_rating_id", panel.MainSize);
      command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
      command.Parameters.AddWithValue("@name", panel.Name);
      command.Parameters.AddWithValue("@color_code", panel.ColorCode);
      command.Parameters.AddWithValue("@parent_id", panel.ParentId);
      command.Parameters.AddWithValue("@aic_ating", panel.AicRating);
      command.Parameters.AddWithValue("@parent_distance", panel.DistanceFromParent);
      command.Parameters.AddWithValue("@num_breakers", panel.NumBreakers);
      command.Parameters.AddWithValue("@voltage_id", panel.Type);
      command.Parameters.AddWithValue("@is_recessed", panel.IsRecessed);
      command.Parameters.AddWithValue("@is_mlo", panel.IsMlo);
      command.Parameters.AddWithValue("@circuit_no", panel.CircuitNo);
      command.Parameters.AddWithValue("@is_hidden_on_plan", panel.IsHiddenOnPlan);
      command.Parameters.AddWithValue("@location", panel.Location);
      command.Parameters.AddWithValue("@high_leg_phase", panel.HighLegPhase);
      command.Parameters.AddWithValue("@load_amperage", panel.Amp);
      command.Parameters.AddWithValue("@kva", panel.Kva);
      command.Parameters.AddWithValue("@order_no", panel.OrderNo);
      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateElectricalPanelNote(ElectricalPanelNote note)
    {
      string query = "UPDATE electrical_panel_notes SET note = @note WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, Connection);

      command.Parameters.AddWithValue("@note", note.Note);
      command.Parameters.AddWithValue("@id", note.Id);

      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateElectricalPanelNoteRel(ElectricalPanelNoteRel noteRel)
    {
      string query =
        "UPDATE electrical_panel_note_panel_rel SET panel_id = @panelId, note_id = @noteId, circuit_no = @circuitNo, length = @length, stack = @stack WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, Connection);

      command.Parameters.AddWithValue("@id", noteRel.Id);
      command.Parameters.AddWithValue("@panelId", noteRel.PanelId);
      command.Parameters.AddWithValue("@noteId", noteRel.NoteId);
      command.Parameters.AddWithValue("@circuitNo", noteRel.CircuitNo);
      command.Parameters.AddWithValue("@length", noteRel.Length);
      command.Parameters.AddWithValue("@stack", noteRel.Stack);

      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertElectricalPanelNote(ElectricalPanelNote note)
    {
      if (String.IsNullOrEmpty(note.Note))
      {
        return;
      }
      string query =
        @"
        INSERT IGNORE INTO electrical_panel_notes
        ( id,  project_id,  electrical_project_id,  note,  date) VALUES
        (@id, @project_id, @electrical_project_id, @note, @date)
        ";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", note.Id);
      command.Parameters.AddWithValue("@project_id", note.ProjectId);
      command.Parameters.AddWithValue("@electrical_project_id", note.ElectricalProjectId);
      command.Parameters.AddWithValue("@note", note.Note);
      command.Parameters.AddWithValue("@date", note.DateCreated);
      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertElectricalPanelNoteRel(ElectricalPanelNoteRel noteRel)
    {
      string query =
        @"
        INSERT IGNORE INTO electrical_panel_note_panel_rel
        ( id,  project_id,  electrical_project_id,  panel_id,  note_id,  circuit_no,  length, stack) VALUES
        (@id, @project_id, @electrical_project_id, @panel_id, @note_id, @circuit_no, @length, @stack)
        ";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", noteRel.Id);
      command.Parameters.AddWithValue("@project_id", noteRel.ProjectId);
      command.Parameters.AddWithValue("@electrical_project_id", noteRel.ElectricalProjectId);
      command.Parameters.AddWithValue("@panel_id", noteRel.PanelId);
      command.Parameters.AddWithValue("@note_id", noteRel.NoteId);
      command.Parameters.AddWithValue("@circuit_no", noteRel.CircuitNo);
      command.Parameters.AddWithValue("@length", noteRel.Length);
      command.Parameters.AddWithValue("@stack", noteRel.Stack);

      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateCustomCircuit(Circuit customCircuit)
    {
      string query =
        "UPDATE electrical_panel_custom_circuits SET number = @number, equip_id = @equipId, breaker_size = @breakerSize, description = @description, load_category = @loadCategory, va = @va, originalVa = @originalVa, custom_breaker_size = @customBreakerSize, custom_description = @customDescription WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, Connection);

      command.Parameters.AddWithValue("@id", customCircuit.Id);
      command.Parameters.AddWithValue("@number", customCircuit.Number);
      command.Parameters.AddWithValue("@equipId", customCircuit.EquipId);
      command.Parameters.AddWithValue("@breakerSize", customCircuit.BreakerSize);
      command.Parameters.AddWithValue("@loadCategory", customCircuit.LoadCategory);
      command.Parameters.AddWithValue("@description", customCircuit.Description);
      command.Parameters.AddWithValue("@va", customCircuit.Va);
      command.Parameters.AddWithValue("@customBreakerSize", customCircuit.CustomBreakerSize);
      command.Parameters.AddWithValue("@customDescription", customCircuit.CustomDescription);

      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateClock(TimeClock clock)
    {
      string query =
        "UPDATE electrical_lighting_timeclocks SET name = @name, bypass_switch_name = @bypassSwitchName, bypass_switch_location = @bypassSwitchLocation, voltage_id = @voltageId, adjacent_panel_id = @adjacentPanelId WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", clock.Id);
      command.Parameters.AddWithValue("@name", clock.Name);
      command.Parameters.AddWithValue("@bypassSwitchName", clock.BypassSwitchName);
      command.Parameters.AddWithValue("@bypassSwitchLocation", clock.BypassSwitchLocation);
      command.Parameters.AddWithValue("@voltageId", clock.VoltageId);
      command.Parameters.AddWithValue("@adjacentPanelId", clock.AdjacentPanelId);

      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertCustomCircuit(
      string projectId,
      string electricalProjectId,
      Circuit customCircuit
    )
    {
      string query =
        @"
INSERT INTO electrical_panel_custom_circuits
( id,  panel_id,  project_id,  electrical_project_id,  equip_id,  number,  breaker_size,  description,  load_category,  va,  original_va,  custom_breaker_size,  custom_description) VALUES
(@id, @panel_id, @project_id, @electrical_project_id, @equip_id, @number, @breaker_size, @description, @load_category, @va, @original_va, @custom_breaker_size, @custom_description)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", customCircuit.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@panel_id", customCircuit.PanelId);
      command.Parameters.AddWithValue("@equip_id", customCircuit.EquipId);
      command.Parameters.AddWithValue("@number", customCircuit.Number);
      command.Parameters.AddWithValue("@breaker_size", customCircuit.BreakerSize);
      command.Parameters.AddWithValue("@load_category", customCircuit.LoadCategory);
      command.Parameters.AddWithValue("@description", customCircuit.Description);
      command.Parameters.AddWithValue("@va", customCircuit.Va);
      command.Parameters.AddWithValue("@custom_breaker_size", customCircuit.CustomBreakerSize);
      command.Parameters.AddWithValue("@custom_description", customCircuit.CustomDescription);
      await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateEquipment(ElectricalEquipment equipment, MySqlConnection conn = null)
    {
      if (conn == null)
      {
        conn = Connection;
      }
      string query =
        "UPDATE electrical_equipment SET description = @description, equip_no = @equip_no, parent_id = @parent_id, owner_id = @owner, voltage_id = @voltage, fla = @fla, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, parent_distance=@distanceFromParent, category_id=@category, color_code = @color_code, connection_type_id = @connection, mocp_id = @mocpId, hp = @hp, has_plug = @has_plug, locking_connector = @locking_connector, width=@width, depth=@depth, height=@height, circuit_no=@circuit_no, is_hidden_on_plan=@is_hidden_on_plan, load_type = @loadType, order_no = @order_no, va=@va, original_va=@originalVa, status_id = @statusId, connection_symbol_id = @connectionSymbolId, num_conv_duplex = @numConvDuplex, circuit_half = @circuitHalf, phase_a_va = @phaseAVa, phase_b_va = @phaseBVa, phase_c_va = @phaseCVa WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, conn);
      command.Parameters.AddWithValue("@id", equipment.Id);
      command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
      command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
      command.Parameters.AddWithValue("@voltage", equipment.Voltage);
      command.Parameters.AddWithValue("@fla", equipment.Fla);
      command.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
      command.Parameters.AddWithValue("@spec_sheet_id", equipment.SpecSheetId);
      command.Parameters.AddWithValue("@aic_rating", equipment.AicRating);
      command.Parameters.AddWithValue("@description", equipment.Description);
      command.Parameters.AddWithValue("@spec_sheet_from_client", equipment.SpecSheetFromClient);
      command.Parameters.AddWithValue("@distanceFromParent", equipment.DistanceFromParent);
      command.Parameters.AddWithValue("@category", equipment.Category);
      command.Parameters.AddWithValue("@color_code", equipment.ColorCode);
      command.Parameters.AddWithValue("@owner", equipment.Owner);
      command.Parameters.AddWithValue("@connection", equipment.Connection);
      command.Parameters.AddWithValue("@mocpId", equipment.MocpId);
      command.Parameters.AddWithValue("@hp", equipment.Hp);
      command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
      command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
      command.Parameters.AddWithValue("@width", equipment.Width);
      command.Parameters.AddWithValue("@depth", equipment.Depth);
      command.Parameters.AddWithValue("@height", equipment.Height);
      command.Parameters.AddWithValue("@circuit_no", equipment.CircuitNo);
      command.Parameters.AddWithValue("@is_hidden_on_plan", equipment.IsHiddenOnPlan);
      command.Parameters.AddWithValue("@loadType", equipment.LoadType);
      command.Parameters.AddWithValue("@order_no", equipment.OrderNo);
      command.Parameters.AddWithValue("@va", equipment.Va);
      command.Parameters.AddWithValue("@originalVa", equipment.OriginalVa);
      command.Parameters.AddWithValue("@statusId", equipment.StatusId);
      command.Parameters.AddWithValue("@connectionSymbolId", equipment.ConnectionSymbolId);
      command.Parameters.AddWithValue("@numConvDuplex", equipment.NumConvDuplex);
      command.Parameters.AddWithValue("@circuitHalf", equipment.circuitHalf);
      command.Parameters.AddWithValue("@phaseAVa", equipment.PhaseAVA);
      command.Parameters.AddWithValue("@phaseBVa", equipment.PhaseBVA);
      command.Parameters.AddWithValue("@phaseCVa", equipment.PhaseCVA);
      await command.ExecuteNonQueryAsync();
    }

    public async Task InsertEquipment(
      string projectId,
      string electricalProjectId,
      ElectricalEquipment equipment,
      MySqlConnection conn = null
    )
    {
      if (conn == null)
      {
        conn = Connection;
      }
      string query =
        @"
INSERT IGNORE INTO electrical_equipment
( id,  project_id,  electrical_project_id,  equip_no,  parent_id,  owner_id,  voltage_id,  fla,  is_three_phase,  spec_sheet_id,  aic_rating,  spec_sheet_from_client,  parent_distance,  category_id,  color_code,  connection_type_id,  description,  hp,  has_plug,  locking_connector,  width,  depth,  height,  circuit_no,  is_hidden_on_plan,  load_type,  order_no,  va,  original_va,  date_created,  status_id,  connection_symbol_id,  num_conv_duplex,  phase_a_va,  phase_b_va,  phase_c_va,  mocp_id) VALUES 
(@id, @project_id, @electrical_project_id, @equip_no, @parent_id, @owner_id, @voltage_id, @fla, @is_three_phase, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @parent_distance, @category_id, @color_code, @connection_type_id, @description, @hp, @has_plug, @locking_connector, @width, @depth, @height, @circuit_no, @is_hidden_on_plan, @load_type, @order_no, @va, @original_va, @date_created, @status_id, @connection_symbol_id, @num_conv_duplex, @phase_a_va, @phase_b_va, @phase_c_va, @mocp_id)";
      MySqlCommand command = new MySqlCommand(query, conn);
      command.Parameters.AddWithValue("@id", equipment.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@owner_id", equipment.Owner);
      command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
      command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
      command.Parameters.AddWithValue("@voltage_id", equipment.Voltage);
      command.Parameters.AddWithValue("@fla", equipment.Fla);
      command.Parameters.AddWithValue("@is_three_phase", equipment.Is3Ph);
      command.Parameters.AddWithValue("@spec_sheet_id", equipment.SpecSheetId);
      command.Parameters.AddWithValue("@aic_rating", equipment.AicRating);
      command.Parameters.AddWithValue("@spec_sheet_from_client", equipment.SpecSheetFromClient);
      command.Parameters.AddWithValue("@parent_distance", equipment.DistanceFromParent);
      command.Parameters.AddWithValue("@category_id", equipment.Category);
      command.Parameters.AddWithValue("@color_code", equipment.ColorCode);
      command.Parameters.AddWithValue("@connection_type_id", equipment.Connection);
      command.Parameters.AddWithValue("@description", equipment.Description);
      command.Parameters.AddWithValue("@hp", equipment.Hp);
      command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
      command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
      command.Parameters.AddWithValue("@width", equipment.Width);
      command.Parameters.AddWithValue("@depth", equipment.Depth);
      command.Parameters.AddWithValue("@height", equipment.Height);
      command.Parameters.AddWithValue("@circuit_no", equipment.CircuitNo);
      command.Parameters.AddWithValue("@is_hidden_on_plan", equipment.IsHiddenOnPlan);
      command.Parameters.AddWithValue("@load_type", equipment.LoadType);
      command.Parameters.AddWithValue("@order_no", equipment.OrderNo);
      command.Parameters.AddWithValue("@va", equipment.Va);
      command.Parameters.AddWithValue("@original_va", equipment.Va);
      command.Parameters.AddWithValue(
        "@date_created",
        equipment.DateCreated.ToString("yyyy-MM-dd HH:mm:ss.fff")
      );
      command.Parameters.AddWithValue("@status_id", equipment.StatusId);
      command.Parameters.AddWithValue("@connection_symbol_id", equipment.ConnectionSymbolId);
      command.Parameters.AddWithValue("@num_conv_duplex", equipment.NumConvDuplex);
      command.Parameters.AddWithValue("@phase_a_va", equipment.PhaseAVA);
      command.Parameters.AddWithValue("@phase_b_va", equipment.PhaseBVA);
      command.Parameters.AddWithValue("@phase_c_va", equipment.PhaseCVA);
      command.Parameters.AddWithValue("@mocp_id", equipment.MocpId);
      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateLighting(ElectricalLighting lighting)
    {
      string query =
        "UPDATE electrical_lighting SET notes = @notes, model_no = @model_no, order_no = @order_no, parent_id = @parent_id, voltage_id = @voltageId, color_code = @colorCode, mounting_type_id = @mountingType, occupancy=@occupancy, manufacturer = @manufacturer, wattage = @wattage, em_capable = @em_capable, tag = @tag, symbol_id = @symbolId, description=@description, driver_type_id = @driverTypeId, spec_sheet_from_client=@specFromClient, spec_sheet_id=@specSheetId, qty = @qty, has_photocell = @hasPhotoCell, location_id = @locationId WHERE id = @id";
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
      command.Parameters.AddWithValue("@order_no", lighting.OrderNo);

      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateLightingControl(ElectricalLightingControl control)
    {
      string query =
        @"
                UPDATE electrical_lighting_controls SET driver_type_id = @driverTypeId, occupancy = @occupancy, name = @name WHERE id = @id
                ";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@driverTypeId", control.DriverTypeId);
      command.Parameters.AddWithValue("@occupancy", control.Occupancy);
      command.Parameters.AddWithValue("@id", control.Id);
      command.Parameters.AddWithValue("@name", control.Tag);
      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertLighting(
      string projectId,
      string electricalProjectId,
      ElectricalLighting lighting
    )
    {
      string query =
        @"
INSERT INTO electrical_lighting
( id,  project_id,  electrical_project_id,  notes,  model_no,  parent_id,  voltage_id,  color_code,  mounting_type_id,  occupancy,  manufacturer,  wattage,  em_capable,  tag,  symbol_id,  description,  driver_type_id,  spec_sheet_from_client,  spec_sheet_id,  qty,  has_photocell,  location_id,  order_no) VALUES
(@id, @project_id, @electrical_project_id, @notes, @model_no, @parent_id, @voltage_id, @color_code, @mounting_type_id, @occupancy, @manufacturer, @wattage, @em_capable, @tag, @symbol_id, @description, @driver_type_id, @spec_sheet_from_client, @spec_sheet_id, @qty, @has_photocell, @location_id, @order_no)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", lighting.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@model_no", lighting.ModelNo);
      command.Parameters.AddWithValue("@parent_id", lighting.ParentId);
      command.Parameters.AddWithValue("@manufacturer", lighting.Manufacturer);
      command.Parameters.AddWithValue("@occupancy", lighting.Occupancy);
      command.Parameters.AddWithValue("@wattage", lighting.Wattage);
      command.Parameters.AddWithValue("@em_capable", lighting.EmCapable);
      command.Parameters.AddWithValue("@mounting_type_id", lighting.MountingType);
      command.Parameters.AddWithValue("@tag", lighting.Tag);
      command.Parameters.AddWithValue("@notes", lighting.Notes);
      command.Parameters.AddWithValue("@voltage_id", lighting.VoltageId);
      command.Parameters.AddWithValue("@symbol_id", lighting.SymbolId);
      command.Parameters.AddWithValue("@color_code", lighting.colorCode);
      command.Parameters.AddWithValue("@description", lighting.Description);
      command.Parameters.AddWithValue("@driver_type_id", lighting.DriverTypeId);
      command.Parameters.AddWithValue("@spec_sheet_from_client", lighting.SpecSheetFromClient);
      command.Parameters.AddWithValue("@spec_sheet_id", lighting.SpecSheetId);
      command.Parameters.AddWithValue("@qty", lighting.Qty);
      command.Parameters.AddWithValue("@has_photo_cell", lighting.HasPhotoCell);
      command.Parameters.AddWithValue("@location_id", lighting.LocationId);
      command.Parameters.AddWithValue("@order_no", lighting.OrderNo);
      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertLightingControl(
      string projectId,
      string electricalProjectId,
      ElectricalLightingControl control
    )
    {
      string query =
        @"
        INSERT INTO electrical_lighting_controls
( id,  project_id,  electrical_project_id,  driver_type_id,  occupancy,  name) VALUES
(@id, @project_id, @electrical_project_id, @driver_type_id, @occupancy, @name)
        ";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", control.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@diver_type_id", control.DriverTypeId);
      command.Parameters.AddWithValue("@occupancy", control.Occupancy);
      command.Parameters.AddWithValue("@name", control.Tag);
      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateTransformer(ElectricalTransformer transformer)
    {
      string query =
        "UPDATE electrical_transformers SET parent_id = @parent_id, voltage_id = @voltage, kva_id = @kva, parent_distance = @distanceFromParent, color_code = @color_code, name = @name, circuit_no = @circuitNo, is_hidden_on_plan = @is_hidden_on_plan, is_wall_mounted = @isWallMounted, aic_rating = @aicRating, order_no = @order_no WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
      command.Parameters.AddWithValue("@id", transformer.Id);
      command.Parameters.AddWithValue("@voltage", transformer.Voltage);
      command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
      command.Parameters.AddWithValue("@kva", transformer.Kva);
      command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
      command.Parameters.AddWithValue("@name", transformer.Name);
      command.Parameters.AddWithValue("@circuitNo", transformer.CircuitNo);
      command.Parameters.AddWithValue("@is_hidden_on_plan", transformer.IsHiddenOnPlan);
      command.Parameters.AddWithValue("@isWallMounted", transformer.IsWallMounted);
      command.Parameters.AddWithValue("@aicRating", transformer.AicRating);
      command.Parameters.AddWithValue("@order_no", transformer.OrderNo);
      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertTransformer(
      string projectId,
      string electricalProjectId,
      ElectricalTransformer transformer
    )
    {
      string query =
        @"
INSERT INTO electrical_transformers
( id,  project_id,  electrical_project_id,  parent_id,  voltage_id,  parent_distance,  color_code,  kva_id,  name,  circuit_no,  is_hidden_on_plan,  is_wall_mounted,  aic_rating,  order_no) VALUES
(@id, @project_id, @electrical_project_id, @parent_id, @voltage_id, @parent_distance, @color_code, @kva_id, @name, @circuit_no, @is_hidden_on_plan, @is_wall_mounted, @aic_rating, @order_no)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", transformer.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
      command.Parameters.AddWithValue("@parent_distance", transformer.DistanceFromParent);
      command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
      command.Parameters.AddWithValue("@kva_id", transformer.Kva);
      command.Parameters.AddWithValue("@name", transformer.Name);
      command.Parameters.AddWithValue("@voltage_id", transformer.Voltage);
      command.Parameters.AddWithValue("@circuit_no", transformer.CircuitNo);
      command.Parameters.AddWithValue("@is_hidden_on_plan", transformer.IsHiddenOnPlan);
      command.Parameters.AddWithValue("@is_wall_mounted", transformer.IsWallMounted);
      command.Parameters.AddWithValue("@aic_rating", transformer.AicRating);
      command.Parameters.AddWithValue("@order_no", transformer.OrderNo);
      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertClock(string projectId, string electricalProjectId, TimeClock clock)
    {
      string query =
        @"
INSERT INTO electrical_lighting_timeclocks
( id,  project_id,  electrical_project_id,  name,  bypass_switch_name,  bypass_switch_location,  voltage_id,  adjacent_panel_id) VALUES 
(@id, @project_id, @electrical_project_id, @name, @bypass_switch_name, @bypass_switch_location, @voltage_id, @adjacent_panel_id)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", clock.Id);
      command.Parameters.AddWithValue("@name", clock.Name);
      command.Parameters.AddWithValue("@bypass_switch_name", clock.BypassSwitchName);
      command.Parameters.AddWithValue("@bypass_switch_location", clock.BypassSwitchLocation);
      command.Parameters.AddWithValue("@voltage_id", clock.VoltageId);
      command.Parameters.AddWithValue("@adjacent_panel_id", clock.AdjacentPanelId);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);

      await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateLocation(Location location)
    {
      string query =
        "UPDATE electrical_lighting_locations SET location = @locationDescription, outdoor = @isOutside WHERE id = @id";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", location.Id);
      command.Parameters.AddWithValue("@locationDescription", location.LocationDescription);
      command.Parameters.AddWithValue("@isOutside", location.IsOutside);

      await command.ExecuteNonQueryAsync();
    }

    private async Task InsertLocation(
      string projectId,
      string electricalProjectId,
      Location location
    )
    {
      if (String.IsNullOrEmpty(location.LocationDescription))
      {
        return;
      }
      string query =
        @"
INSERT INTO electrical_lighting_locations
( id,  project_id,  electrical_project_id,  location,  outdoor) VALUES
(@id, @project_id, @electrical_project_id, @location, @outdoor)";
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", location.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@location", location.LocationDescription);
      command.Parameters.AddWithValue("@outdoor", location.IsOutside);

      await command.ExecuteNonQueryAsync();

      query =
        @"
INSERT INTO electrical_lighting_timeclock_control_relays
( id,  project_id,  electrical_project_id,  name,  outdoor) VALUES
(@id, @project_id, @electrical_project_id, @name, @outdoor)";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", location.Id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("@name", location.LocationDescription);
      command.Parameters.AddWithValue("@outdoor", location.IsOutside);

      await command.ExecuteNonQueryAsync();
    }

    private async Task DeleteRemovedItems(string tableName, HashSet<string> ids)
    {
      var idType = "id";

      foreach (var id in ids)
      {
        string query = $"DELETE FROM {tableName} WHERE {idType} = @id";
        MySqlCommand command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
      }
    }

    public async Task<ObservableCollection<ElectricalService>> GetProjectServices(
      string electricalProjectId
    )
    {
      ObservableCollection<ElectricalService> services =
        new ObservableCollection<ElectricalService>();
      string query =
        "SELECT * FROM electrical_services WHERE electrical_project_id = @electricalProjectId ORDER BY order_no";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        services.Add(
          new ElectricalService(
            reader.GetString("id"),
            reader.GetString("project_id"),
            reader.GetString("electrical_project_id"),
            reader.GetString("name"),
            reader.GetInt32("electrical_service_voltage_id"),
            reader.GetInt32("electrical_service_amp_rating_id"),
            reader.GetInt32("electrical_service_meter_config_id"),
            reader.GetString("color_code"),
            reader.GetInt32("aic_rating"),
            reader.GetString("parent_id"),
            reader.GetInt32("order_no")
          )
        );
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return services;
    }

    public async Task<ObservableCollection<ElectricalPanel>> GetProjectPanels(
      string electricalProjectId
    )
    {
      ObservableCollection<ElectricalPanel> panels = new ObservableCollection<ElectricalPanel>();
      string query =
        "SELECT * FROM electrical_panels WHERE electrical_project_id = @electricalProjectId ORDER BY order_no";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        panels.Add(
          new ElectricalPanel(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "project_id"),
            GetSafeString(reader, "electrical_project_id"),
            GetSafeInt(reader, "bus_amp_rating_id"),
            GetSafeInt(reader, "main_amp_rating_id"),
            GetSafeBoolean(reader, "is_mlo"),
            GetSafeBoolean(reader, "is_distribution"),
            GetSafeString(reader, "name"),
            GetSafeString(reader, "color_code"),
            GetSafeString(reader, "parent_id"),
            GetSafeInt(reader, "num_breakers"),
            GetSafeInt(reader, "parent_distance"),
            GetSafeInt(reader, "aic_rating"),
            GetSafeFloat(reader, "load_amperage"),
            GetSafeFloat(reader, "kva"),
            GetSafeInt(reader, "voltage_id"),
            false,
            GetSafeBoolean(reader, "is_recessed"),
            GetSafeInt(reader, "circuit_no"),
            GetSafeBoolean(reader, "is_hidden_on_plan"),
            GetSafeString(reader, "location"),
            GetSafeInt(reader, "voltage_id") == 4 ? GetSafeChar(reader, "high_leg_phase") : '-',
            GetSafeInt(reader, "order_no"),
            GetSafeInt(reader, "status_id"),
            this
          )
        );
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return panels;
    }

    public ObservableCollection<ElectricalPanelNoteRel> GetElectricalPanelNoteRels(string panelId)
    {
      ObservableCollection<ElectricalPanelNoteRel> noteRels =
        new ObservableCollection<ElectricalPanelNoteRel>();
      string query =
        @"
                SELECT 
                electrical_panel_note_panel_rel.id,
                electrical_panel_note_panel_rel.project_id,
                electrical_panel_note_panel_rel.electrical_project_id,
                electrical_panel_note_panel_rel.panel_id,
                electrical_panel_note_panel_rel.note_id,
                electrical_panel_note_panel_rel.circuit_no,
                electrical_panel_note_panel_rel.length,
                electrical_panel_note_panel_rel.stack,
                electrical_panel_notes.note
                FROM electrical_panel_note_panel_rel
                LEFT JOIN electrical_panel_notes ON electrical_panel_notes.id = electrical_panel_note_panel_rel.note_id
                WHERE electrical_panel_note_panel_rel.panel_id = @panelId
                ORDER BY electrical_panel_notes.date
                ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@panelId", panelId);
      MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
      List<string> noteIds = new List<string>();
      while (reader.Read())
      {
        string noteId = GetSafeString(reader, "note_id");
        if (!noteIds.Contains(noteId))
        {
          noteIds.Add(noteId);
        }
        ElectricalPanelNoteRel noteRel = new ElectricalPanelNoteRel(
          GetSafeString(reader, "id"),
          GetSafeString(reader, "project_id"),
          GetSafeString(reader, "electrical_project_id"),
          GetSafeString(reader, "panel_id"),
          GetSafeString(reader, "note_id"),
          GetSafeString(reader, "note"),
          GetSafeInt(reader, "circuit_no"),
          GetSafeInt(reader, "length"),
          GetSafeInt(reader, "stack"),
          (noteIds.IndexOf(noteId) + 1).ToString()
        );
        noteRels.Add(noteRel);
      }
      reader.Close();
      CloseConnection(Connection);
      return noteRels;
    }

    public async Task<
      ObservableCollection<ElectricalPanelNoteRel>
    > GetProjectElectricalPanelNoteRels(string electricalProjectId)
    {
      ObservableCollection<ElectricalPanelNoteRel> noteRels =
        new ObservableCollection<ElectricalPanelNoteRel>();
      string query =
        @"
                SELECT 
                electrical_panel_note_panel_rel.id,
                electrical_panel_note_panel_rel.project_id,
                electrical_panel_note_panel_rel.electrical_project_id,
                electrical_panel_note_panel_rel.panel_id,
                electrical_panel_note_panel_rel.note_id,
                electrical_panel_note_panel_rel.circuit_no,
                electrical_panel_note_panel_rel.length,
                electrical_panel_note_panel_rel.stack,
                electrical_panel_notes.note
                FROM electrical_panel_note_panel_rel
                LEFT JOIN electrical_panel_notes ON electrical_panel_notes.id = electrical_panel_note_panel_rel.note_id
                WHERE electrical_panel_note_panel_rel.electrical_project_id = @electricalProjectId
                ORDER BY electrical_panel_note_panel_rel.panel_id, electrical_panel_notes.date
                ";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      List<string> noteIds = new List<string>();
      string currentPanelId = "";
      while (await reader.ReadAsync())
      {
        string noteId = GetSafeString(reader, "note_id");
        if (currentPanelId != GetSafeString(reader, "panel_id"))
        {
          noteIds.Clear();
          currentPanelId = GetSafeString(reader, "panel_id");
        }
        if (!noteIds.Contains(noteId))
        {
          noteIds.Add(noteId);
        }
        ElectricalPanelNoteRel noteRel = new ElectricalPanelNoteRel(
          GetSafeString(reader, "id"),
          GetSafeString(reader, "project_id"),
          GetSafeString(reader, "electrical_project_id"),
          currentPanelId,
          GetSafeString(reader, "note_id"),
          GetSafeString(reader, "note"),
          GetSafeInt(reader, "circuit_no"),
          GetSafeInt(reader, "length"),
          GetSafeInt(reader, "stack"),
          (noteIds.IndexOf(noteId) + 1).ToString()
        );
        noteRels.Add(noteRel);
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return noteRels;
    }

    public async Task<ObservableCollection<ElectricalPanelNote>> GetProjectElectricalPanelNotes(
      string electricalProjectId
    )
    {
      ObservableCollection<ElectricalPanelNote> notes =
        new ObservableCollection<ElectricalPanelNote>();
      string query =
        @"
                SELECT * 
                FROM electrical_panel_notes
                WHERE electrical_project_id = @electrical_project_id
                ORDER BY id
                ";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      List<string> noteIds = new List<string>();
      while (await reader.ReadAsync())
      {
        string noteId = GetSafeString(reader, "id");
        if (!noteIds.Contains(noteId))
        {
          noteIds.Add(noteId);
        }
        ElectricalPanelNote note = new ElectricalPanelNote(
          GetSafeString(reader, "id"),
          GetSafeString(reader, "project_id"),
          GetSafeString(reader, "electrical_project_id"),
          GetSafeString(reader, "note"),
          (noteIds.IndexOf(noteId) + 1).ToString()
        );
        notes.Add(note);
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return notes;
    }

    public ObservableCollection<ElectricalPanelNote> GetElectricalPanelNotes(string panelId)
    {
      ObservableCollection<ElectricalPanelNote> notes =
        new ObservableCollection<ElectricalPanelNote>();
      string query =
        @"
                SELECT 
                electrical_panel_notes.id as note_id,
                electrical_panel_notes.project_id,
                electrical_panel_notes.electrical_project_id,
                electrical_panel_notes.note,
                electrical_panel_notes.date,
                electrical_panel_note_panel_rel.id as rel_id,
                electrical_panel_note_panel_rel.panel_id,
                electrical_panel_note_panel_rel.circuit_no
                FROM electrical_panel_notes
                LEFT JOIN electrical_panel_note_panel_rel
                ON electrical_panel_note_panel_rel.note_id = electrical_panel_notes.id
                WHERE electrical_panel_note_panel_rel.panel_id = @panelId
                ORDER BY electrical_panel_notes.date
                ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@panelId", panelId);
      MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
      List<string> noteIds = new List<string>();
      while (reader.Read())
      {
        string noteId = GetSafeString(reader, "note_id");
        if (!noteIds.Contains(noteId))
        {
          noteIds.Add(noteId);
          ElectricalPanelNote note = new ElectricalPanelNote(
            GetSafeString(reader, "note_id"),
            GetSafeString(reader, "project_id"),
            GetSafeString(reader, "electrical_project_id"),
            GetSafeString(reader, "note"),
            (noteIds.IndexOf(noteId) + 1).ToString()
          );
          note.DateCreated = GetSafeDateTime(reader, "date").ToString("yyyy-MM-dd HH:mm:ss.fff");
          notes.Add(note);
        }
      }
      reader.Close();
      CloseConnection(Connection);
      return notes;
    }

    public async Task<ObservableCollection<Circuit>> GetProjectCustomCircuits(
      string electricalProjectId
    )
    {
      ObservableCollection<Circuit> customCircuits = new ObservableCollection<Circuit>();
      string query =
        "SELECT * FROM electrical_panel_custom_circuits WHERE electrical_project_id = @electricalProjectId";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
        customCircuits.Add(
          new Circuit(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "panel_id"),
            GetSafeString(reader, "project_id"),
            GetSafeString(reader, "electrical_project_id"),
            GetSafeString(reader, "equip_id"),
            GetSafeInt(reader, "number"),
            GetSafeInt(reader, "va"),
            GetSafeInt(reader, "breaker_size"),
            GetSafeString(reader, "description"),
            GetSafeInt(reader, "load_category"),
            GetSafeBoolean(reader, "custom_breaker_size"),
            GetSafeBoolean(reader, "custom_description")
          )
        );
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return customCircuits;
    }

    public async Task<ObservableCollection<Circuit>> GetProjectElectricalPanelMiniBreakers(
      string electricalProjectId
    )
    {
      ObservableCollection<Circuit> miniBreakers = new ObservableCollection<Circuit>();
      string query =
        @"
                SELECT
                electrical_panel_mini_breakers.id,
                electrical_panel_mini_breakers.project_id,
                electrical_panel_mini_breakers.panel_id,
                electrical_panel_mini_breakers.circuit_no,
                electrical_panel_mini_breakers.equip_a_id,
                electrical_panel_mini_breakers.equip_b_id,
                electrical_panel_mini_breakers.breaker_size_a,
                electrical_panel_mini_breakers.breaker_size_b,
                electrical_panel_mini_breakers.interlock_a_to_next_b,
                electrical_panel_mini_breakers.interlock_b_to_next_a,
                equip_a.va as va_a,
                equip_b.va as va_b,
                equip_a.description as description_a,
                equip_b.description as description_b,
                equip_a.equip_no as equip_no_a,
                equip_b.equip_no as equip_no_b
                FROM electrical_panel_mini_breakers
                LEFT JOIN electrical_equipment as equip_a ON equip_a.id = electrical_panel_mini_breakers.equip_a_id
                LEFT JOIN electrical_equipment as equip_b ON equip_b.id = electrical_panel_mini_breakers.equip_b_id
                WHERE electrical_panel_mini_breakers.electrical_project_id = @electricalProjectId
                ";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        miniBreakers.Add(
          new Circuit(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "panel_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
            string.Empty,
            GetSafeInt(reader, "circuit_no"),
            GetSafeInt(reader, "va_a") + GetSafeInt(reader, "va_b"),
            2020,
            GetSafeString(reader, "equip_no_a")
              + "-"
              + GetSafeString(reader, "description_a")
              + ";"
              + GetSafeString(reader, "equip_no_b")
              + "-"
              + GetSafeString(reader, "description_b"),
            0,
            false,
            false,
            GetSafeString(reader, "equip_a_id"),
            GetSafeString(reader, "equip_b_id"),
            GetSafeInt(reader, "breaker_size_a"),
            GetSafeInt(reader, "breaker_size_b"),
            GetSafeBoolean(reader, "interlock_a_to_next_b"),
            GetSafeBoolean(reader, "interlock_b_to_next_a"),
            GetSafeInt(reader, "va_a"),
            GetSafeInt(reader, "va_b")
          )
        );
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return miniBreakers;
    }

    public async Task<ObservableCollection<ElectricalEquipment>> GetProjectEquipment(
      string electricalProjectId
    )
    {
      ObservableCollection<ElectricalEquipment> equipments =
        new ObservableCollection<ElectricalEquipment>();
      string query =
        "SELECT * FROM electrical_equipment WHERE electrical_project_id = @electricalProjectId ORDER BY order_no, date_created";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
        equipments.Add(
          new ElectricalEquipment(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "project_id"),
            GetSafeString(reader, "electrical_project_id"),
            GetSafeString(reader, "owner_id"),
            GetSafeString(reader, "equip_no"),
            0,
            GetSafeString(reader, "parent_id"),
            GetSafeInt(reader, "voltage_id"),
            GetSafeFloat(reader, "fla"),
            GetSafeFloat(reader, "va"),
            GetSafeFloat(reader, "original_va"),
            GetSafeBoolean(reader, "is_three_phase"),
            GetSafeString(reader, "spec_sheet_id"),
            GetSafeInt(reader, "aic_rating"),
            GetSafeBoolean(reader, "spec_sheet_from_client"),
            GetSafeInt(reader, "parent_distance"),
            GetSafeInt(reader, "category_id"),
            GetSafeString(reader, "color_code"),
            false,
            GetSafeInt(reader, "connection_type_id"),
            GetSafeString(reader, "description"),
            GetSafeInt(reader, "mocp_id"),
            GetSafeString(reader, "hp"),
            GetSafeBoolean(reader, "has_plug"),
            GetSafeBoolean(reader, "locking_connector"),
            GetSafeFloat(reader, "width"),
            GetSafeFloat(reader, "depth"),
            GetSafeFloat(reader, "height"),
            GetSafeInt(reader, "circuit_no"),
            GetSafeBoolean(reader, "is_hidden_on_plan"),
            GetSafeInt(reader, "load_type"),
            GetSafeInt(reader, "order_no"),
            GetSafeInt(reader, "status_id"),
            GetSafeInt(reader, "connection_symbol_id"),
            GetSafeInt(reader, "num_conv_duplex"),
            GetSafeInt(reader, "circuit_half"),
            GetSafeFloat(reader, "phase_a_va"),
            GetSafeFloat(reader, "phase_b_va"),
            GetSafeFloat(reader, "phase_c_va")
          )
        );

      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return equipments;
    }

    public async Task<ObservableCollection<ElectricalLighting>> GetProjectLighting(
      string electricalProjectId
    )
    {
      ObservableCollection<ElectricalLighting> lightings =
        new ObservableCollection<ElectricalLighting>();
      string query =
        "SELECT * FROM electrical_lighting WHERE electrical_project_id = @electricalProjectId ORDER BY order_no";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

      while (await reader.ReadAsync())
      {
        lightings.Add(
          new ElectricalLighting(
            reader.GetString("id"),
            reader.GetString("project_id"),
            reader.GetString("electrical_project_id"),
            reader.GetString("parent_id"),
            reader.GetString("manufacturer"),
            reader.GetString("model_no"),
            reader.IsDBNull(reader.GetOrdinal("qty")) ? 0 : reader.GetInt32("qty"),
            reader.IsDBNull(reader.GetOrdinal("occupancy"))
              ? false
              : reader.GetBoolean("occupancy"),
            reader.IsDBNull(reader.GetOrdinal("wattage")) ? 0 : reader.GetFloat("wattage"),
            reader.IsDBNull(reader.GetOrdinal("em_capable"))
              ? false
              : reader.GetBoolean("em_capable"),
            reader.IsDBNull(reader.GetOrdinal("mounting_type_id"))
              ? 0
              : reader.GetInt32("mounting_type_id"),
            reader.GetString("tag"),
            reader.GetString("notes"),
            reader.IsDBNull(reader.GetOrdinal("voltage_id")) ? 0 : reader.GetInt32("voltage_id"),
            reader.IsDBNull(reader.GetOrdinal("symbol_id")) ? 0 : reader.GetInt32("symbol_id"),
            reader.GetString("color_code"),
            false,
            reader.GetString("description"),
            reader.IsDBNull(reader.GetOrdinal("driver_type_id"))
              ? 0
              : reader.GetInt32("driver_type_id"),
            reader.IsDBNull(reader.GetOrdinal("spec_sheet_from_client"))
              ? false
              : reader.GetBoolean("spec_sheet_from_client"),
            reader.GetString("spec_sheet_id"),
            reader.IsDBNull(reader.GetOrdinal("has_photocell"))
              ? false
              : reader.GetBoolean("has_photocell"),
            reader.GetString("location_id"),
            reader.IsDBNull(reader.GetOrdinal("order_no")) ? 0 : reader.GetInt32("order_no")
          )
        );
      }

      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return lightings;
    }

    public async Task<ObservableCollection<ElectricalLightingControl>> GetProjectLightingControls(
      string electricalProjectId
    )
    {
      ObservableCollection<ElectricalLightingControl> controls =
        new ObservableCollection<ElectricalLightingControl>();
      string query =
        "SELECT * FROM electrical_lighting_controls WHERE electrical_project_id = @electricalProjectId";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        controls.Add(
          new ElectricalLightingControl(
            GetSafeString(reader, "id"),
            GetSafeInt(reader, "driver_type_id"),
            GetSafeBoolean(reader, "occupancy"),
            GetSafeString(reader, "name")
          )
        );
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return controls;
    }

    public async Task<ObservableCollection<ElectricalTransformer>> GetProjectTransformers(
      string electricalProjectId
    )
    {
      ObservableCollection<ElectricalTransformer> transformers =
        new ObservableCollection<ElectricalTransformer>();
      string query =
        "SELECT * FROM electrical_transformers WHERE electrical_project_id = @electricalProjectId ORDER BY order_no";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        transformers.Add(
          new ElectricalTransformer(
            reader.GetString("id"),
            reader.GetString("project_id"),
            reader.GetString("electrical_project_id"),
            reader.GetString("parent_id"),
            reader.IsDBNull(reader.GetOrdinal("parent_distance"))
              ? 0
              : reader.GetInt32("parent_distance"),
            reader.GetString("color_code"),
            reader.IsDBNull(reader.GetOrdinal("voltage_id")) ? 0 : reader.GetInt32("voltage_id"),
            reader.GetString("name"),
            reader.IsDBNull(reader.GetOrdinal("kva_id")) ? 0 : reader.GetInt32("kva_id"),
            false,
            reader.IsDBNull(reader.GetOrdinal("circuit_no")) ? 0 : reader.GetInt32("circuit_no"),
            reader.GetBoolean("is_hidden_on_plan"),
            reader.GetBoolean("is_wall_mounted"),
            reader.IsDBNull(reader.GetOrdinal("aic_rating")) ? 0 : reader.GetInt32("aic_rating"),
            reader.IsDBNull(reader.GetOrdinal("order_no")) ? 0 : reader.GetInt32("order_no")
          )
        );
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return transformers;
    }

    public async Task<ObservableCollection<Location>> GetLightingLocations(
      string electricalProjectId
    )
    {
      ObservableCollection<Location> locations = new ObservableCollection<Location>();
      string query =
        "SELECT * FROM electrical_lighting_locations WHERE electrical_project_id = @electricalProjectId";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var location = new Location();
        location.Id = GetSafeString(reader, "id");
        location.isOutside = GetSafeBoolean(reader, "outdoor");
        location.LocationDescription = GetSafeString(reader, "location");
        locations.Add(location);
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return locations;
    }

    public async Task<ObservableCollection<TimeClock>> GetLightingTimeClocks(
      string electricalProjectId
    )
    {
      ObservableCollection<TimeClock> clocks = new ObservableCollection<TimeClock>();
      string query =
        "SELECT * FROM electrical_lighting_timeclocks WHERE electrical_project_id = @electricalProjectId";
      await OpenConnectionAsync(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var clock = new TimeClock();
        clock.Id = reader.GetString("id");
        clock.Name = reader.GetString("name");
        clock.BypassSwitchName = reader.GetString("bypass_switch_name");
        clock.BypassSwitchLocation = reader.GetString("bypass_switch_location");
        clock.VoltageId = reader.GetInt32("voltage_id");
        clock.AdjacentPanelId = reader.GetString("adjacent_panel_id");
        clocks.Add(clock);
      }
      await reader.CloseAsync();
      await CloseConnectionAsync(Connection);
      return clocks;
    }

    public string CreateElectricalPanelMiniBreaker(string panelId, int circuitNo)
    {
      string electricalProjectId = string.Empty;
      string projectId = string.Empty;
      string id = Guid.NewGuid().ToString();
      string query =
        "SELECT electrical_project_id, project_id FROM electrical_panels WHERE id = @panelId";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("panelId", panelId);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        electricalProjectId = GetSafeString(reader, "electrical_project_id");
        projectId = GetSafeString(reader, "project_id");
      }
      reader.Close();
      if (string.IsNullOrEmpty(electricalProjectId))
      {
        CloseConnection(Connection);
        return string.Empty;
      }
      query =
        @"
                INSERT INTO
                electrical_panel_mini_breakers
                ( id,  project_id,  electrical_project_id,  panel_id,  circuit_no) VALUES
                (@id, @project_id, @electrical_project_id, @panel_id, @circuit_no)";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("id", id);
      command.Parameters.AddWithValue("project_id", projectId);
      command.Parameters.AddWithValue("electrical_project_id", electricalProjectId);
      command.Parameters.AddWithValue("panel_id", panelId);
      command.Parameters.AddWithValue("circuit_no", circuitNo);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
      return id;
    }

    public string GetElectricalPanelMiniBreakerId(string panelId, int circuitNo)
    {
      string id = string.Empty;
      string query =
        "SELECT id FROM electrical_panel_mini_breakers WHERE panel_id = @panelId AND circuit_no = @circuitNo";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("panelId", panelId);
      command.Parameters.AddWithValue("circuitNo", circuitNo);
      MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
      if (reader.Read())
      {
        id = GetSafeString(reader, "id");
      }
      reader.Close();
      CloseConnection(Connection);
      return id;
    }

    public (string, string, int, int, int, int) UpdateElectricalPanelMiniBreaker(
      string id,
      string equipAId,
      string equipBId,
      int breakerSizeA,
      int breakerSizeB,
      bool interlockA,
      bool interlockB,
      int circuitNo
    )
    {
      string query =
        @"
                UPDATE electrical_panel_mini_breakers
                SET 
                equip_a_id = @equipAId,
                equip_b_id = @equipBId,
                breaker_size_a = @breakerSizeA,
                breaker_size_b = @breakerSizeB,
                interlock_a_to_next_b = @interlockA,
                interlock_b_to_next_a = @interlockB
                WHERE id = @id";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("equipAId", equipAId);
      command.Parameters.AddWithValue("equipBId", equipBId);
      command.Parameters.AddWithValue("breakerSizeA", breakerSizeA);
      command.Parameters.AddWithValue("breakerSizeB", breakerSizeB);
      command.Parameters.AddWithValue("interlockA", interlockA);
      command.Parameters.AddWithValue("interlockB", interlockB);
      command.Parameters.AddWithValue("id", id);
      command.ExecuteNonQuery();
      query =
        @"UPDATE electrical_equipment SET circuit_no = @circuitNo, circuit_half = @circuitHalf WHERE id = @equipAId";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("circuitNo", circuitNo);
      command.Parameters.AddWithValue("circuitHalf", 1);
      command.Parameters.AddWithValue("@equipAId", equipAId);
      command.ExecuteNonQuery();
      query =
        @"UPDATE electrical_equipment SET circuit_no = @circuitNo, circuit_half = @circuitHalf WHERE id = @equipBId";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("circuitNo", circuitNo);
      command.Parameters.AddWithValue("circuitHalf", 2);
      command.Parameters.AddWithValue("@equipBId", equipBId);
      command.ExecuteNonQuery();

      query =
        @"
                SELECT
                equip_a.equip_no as no_a,
                equip_a.description as desc_a,
                equip_a.va as va_a,
                equip_a.voltage_id as volt_a,
                equip_b.equip_no as no_b,
                equip_b.description as desc_b,
                equip_b.va as va_b,
                equip_b.voltage_id as volt_b
                FROM electrical_panel_mini_breakers
                LEFT JOIN electrical_equipment as equip_a ON electrical_panel_mini_breakers.equip_a_id = equip_a.id
                LEFT JOIN electrical_equipment as equip_b ON electrical_panel_mini_breakers.equip_b_id = equip_b.id
                WHERE equip_a.id = @equipAId AND equip_b.id = @equipBId
                ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("equipAId", equipAId);
      command.Parameters.AddWithValue("equipBId", equipBId);
      MySqlDataReader reader = command.ExecuteReader();
      string noA = string.Empty;
      string descA = string.Empty;
      int vaA = 0;
      int voltA = 0;
      string noB = string.Empty;
      string descB = string.Empty;
      int vaB = 0;
      int voltB = 0;
      if (reader.Read())
      {
        noA = GetSafeString(reader, "no_a");
        descA = GetSafeString(reader, "desc_a");
        vaA = GetSafeInt(reader, "va_a");
        voltA = GetSafeInt(reader, "volt_a");
        noB = GetSafeString(reader, "no_b");
        descB = GetSafeString(reader, "desc_b");
        vaB = GetSafeInt(reader, "va_b");
        voltB = GetSafeInt(reader, "volt_b");
      }
      reader.Close();
      CloseConnection(Connection);
      return ($"{noA}-{descA}", $"{noB}-{descB}", vaA, vaB, voltA, voltB);
    }

    public void DeleteElectricalPanelMiniBreaker(string panelId, int circuitNo)
    {
      string query =
        @"
                DELETE FROM electrical_panel_mini_breakers WHERE panel_id = @panelId AND circuit_no = @circuitNo
                ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("panelId", panelId);
      command.Parameters.AddWithValue("circuitNo", circuitNo);
      command.ExecuteNonQuery();
      query =
        @"
                UPDATE electrical_equipment SET circuit_no = 0, circuit_half = 0 WHERE parent_id = @panelId AND circuit_no = @circuitNo
                ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("panelId", panelId);
      command.Parameters.AddWithValue("circuitNo", circuitNo);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
    }

    public async Task CloneElectricalProject(
      string projectId,
      string electricalProjectId,
      string newElectricalProjectId
    )
    {
      // HERE add all the electrical tables
      // electrical_disconnects
      // electrical_distribution_breakers
      // electrical_distribution_buses
      // electrical_keyed_notes
      // electrical_keyed_note_tables
      // electrical_lighting_lti_altered_systems
      // electrical_lighting_lti_control_areas
      // electrical_lighting_lti_luminaires
      // electrical_lighting_lti_scope
      // electrical_lighting_lto_exterior_controls
      // electrical_lighting_lto_hardscape_areas
      // electrical_lighting_lto_luminaires
      // electrical_lighting_lto_scope
      // electrical_lighting_lto_use_or_lose_areas
      // electrical_lighting_timeclocks
      // electrical_lighting_timeclock_control_relays
      // electrical_main_breakers
      // electrical_meters
      // electrical_panel_breakers
      // electrical_panel_mini_breakers
      // electrical_panel_notes
      // electrical_panel_note_panel_rel
      // electrical_single_line_keyed_notes
      // electrical_single_line_keyed_note_node_rel
      // electrical_single_line_nodes
      // electrical_single_line_node_links
      // electrical_single_line_node_types
      var services = await GetProjectServices(electricalProjectId);
      var panels = await GetProjectPanels(electricalProjectId);
      var equipments = await GetProjectEquipment(electricalProjectId);
      var lightings = await GetProjectLighting(electricalProjectId);
      var lightingControls = await GetProjectLightingControls(electricalProjectId);
      var transformers = await GetProjectTransformers(electricalProjectId);
      var locations = await GetLightingLocations(electricalProjectId);
      var electricalPanelNotes = await GetProjectElectricalPanelNotes(electricalProjectId);
      var electricalPanelNoteRels = await GetProjectElectricalPanelNoteRels(electricalProjectId);
      var customCircuits = await GetProjectCustomCircuits(electricalProjectId);
      var clocks = await GetLightingTimeClocks(electricalProjectId);

      Dictionary<string, string> parentIdSwitch = new Dictionary<string, string>();
      Dictionary<string, string> locationIdSwitch = new Dictionary<string, string>();
      Dictionary<string, string> panelNoteIdSwitch = new Dictionary<string, string>();

      foreach (var service in services)
      {
        string Id = Guid.NewGuid().ToString();
        parentIdSwitch.Add(service.Id, Id);
        service.Id = Id;
        service.ElectricalProjectId = newElectricalProjectId;
      }
      foreach (var panel in panels)
      {
        string Id = Guid.NewGuid().ToString();
        parentIdSwitch.Add(panel.Id, Id);
        panel.Id = Id;
        panel.ElectricalProjectId = newElectricalProjectId;
      }
      foreach (var clock in clocks)
      {
        string Id = Guid.NewGuid().ToString();
        //locationIdSwitch.Add(location.Id, Id);
        clock.Id = Id;
      }
      foreach (var transformer in transformers)
      {
        string Id = Guid.NewGuid().ToString();
        parentIdSwitch.Add(transformer.Id, Id);
        transformer.Id = Id;
        transformer.ElectricalProjectId = newElectricalProjectId;
      }
      foreach (var equipment in equipments)
      {
        string Id = Guid.NewGuid().ToString();
        equipment.Id = Id;
        equipment.ElectricalProjectId = newElectricalProjectId;
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
        lighting.ElectricalProjectId = newElectricalProjectId;
        if (!string.IsNullOrEmpty(lighting.LocationId) && lighting.LocationId != "0")
        {
          lighting.LocationId = locationIdSwitch[lighting.LocationId];
        }
      }
      foreach (var control in lightingControls)
      {
        string Id = Guid.NewGuid().ToString();
        control.Id = Id;
      }
      foreach (var note in electricalPanelNotes)
      {
        string Id = Guid.NewGuid().ToString();
        note.Id = Id;
        note.ElectricalProjectId = newElectricalProjectId;
      }
      foreach (var note in electricalPanelNoteRels)
      {
        string Id = Guid.NewGuid().ToString();
        note.Id = Id;
        note.ElectricalProjectId = newElectricalProjectId;
        note.PanelId = parentIdSwitch[note.PanelId];
      }
      foreach (var circuit in customCircuits)
      {
        string Id = Guid.NewGuid().ToString();
        circuit.Id = Id;
        circuit.ElectricalProjectId = newElectricalProjectId;
        circuit.PanelId = parentIdSwitch[circuit.PanelId];
      }

      foreach (var panel in panels)
      {
        if (!string.IsNullOrEmpty(panel.parentId))
        {
          panel.ParentId = parentIdSwitch[panel.ParentId];
        }
      }
      foreach (var service in services)
      {
        if (!string.IsNullOrEmpty(service.parentId))
        {
          service.ParentId = parentIdSwitch[service.ParentId];
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
      await UpdateProject(
        projectId,
        newElectricalProjectId,
        services,
        panels,
        equipments,
        transformers,
        lightings,
        lightingControls,
        locations,
        electricalPanelNotes,
        electricalPanelNoteRels,
        customCircuits,
        clocks
      );
    }

    public Dictionary<int, string> GetAllElectricalProjectVersionIds(string projectId)
    {
      Dictionary<int, string> electricalProjectIds = new Dictionary<int, string>();
      string query =
        @"
        SELECT id, version FROM electrical_projects WHERE project_id = @projectId ORDER BY version
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);

      command.Parameters.AddWithValue("@projectId", projectId);

      MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();

      while (reader.Read())
      {
        electricalProjectIds.Add(GetSafeInt(reader, "version"), GetSafeString(reader, "id"));
      }
      reader.Close();
      CloseConnection(Connection);
      return electricalProjectIds;
    }

    public int GetElectricalProjectVersionNo(string id)
    {
      int versionNo = 1;
      string query =
        @"
        SELECT version FROM electrical_projects WHERE id = @id
      ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", id);
      MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();

      if (reader.Read())
      {
        versionNo = GetSafeInt(reader, "version");
      }
      reader.Close();
      CloseConnection(Connection);
      return versionNo;
    }

    public List<string> GetElectricalTables()
    {
      List<string> tables = new List<string> { };
      string query =
        @"SELECT DISTINCT TABLE_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE COLUMN_NAME = @column_name
        AND TABLE_SCHEMA = @table_schema";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@column_name", "electrical_project_id");
      command.Parameters.AddWithValue("@table_schema", "gmep-design-tool");
      MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
      while (reader.Read())
      {
        tables.Add(GetSafeString(reader, "TABLE_NAME"));
      }
      reader.Close();
      CloseConnection(Connection);
      return tables;
    }

    public void DeleteAllElectricalProjectAssets(string electricalProjectId)
    {
      List<string> tables = GetElectricalTables();
      OpenConnection(Connection);

      string query = "";
      MySqlCommand command = new MySqlCommand(query, Connection);
      foreach (string table in tables)
      {
        query = $"DELETE FROM {table} WHERE electrical_project_id = @electrical_project_id";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
        command.ExecuteNonQuery();
      }

      query =
        @"
        DELETE FROM electrical_projects WHERE id = @id
        ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", electricalProjectId);
      command.ExecuteNonQuery();

      CloseConnection(Connection);
    }

    public void SyncProjectDisciplineTable(string discipline)
    {
      string query =
        @"
        SELECT id FROM projects WHERE version = 1
        ";
      OpenConnection(Connection);
      List<string> projectIds = new List<string>();
      MySqlCommand command = new MySqlCommand(query, Connection);
      MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
      while (reader.Read())
      {
        projectIds.Add(GetSafeString(reader, "id"));
      }
      reader.Close();
      query = $"INSERT IGNORE INTO {discipline}_projects (id, project_id) VALUES (@id, @projectId)";
      foreach (string id in projectIds)
      {
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@projectId", id);
        command.ExecuteNonQuery();
      }
      Connection.Close();
    }

    public async Task<(string, string)> CheckActiveSessionOnDiscipline(
      string projectNo,
      int disciplineId
    )
    {
      string activeUserName = string.Empty;
      string activeUserId = string.Empty;
      string query =
        @"
                  SELECT last_accessed, employee_id, first_name, last_name
                  FROM sessions
                  LEFT JOIN employees ON employees.id = sessions.employee_id
                  LEFT JOIN contacts ON employees.contact_id = contacts.id
                  WHERE project_no = @projectNo AND discipline_id = @disciplineId 
                  ORDER BY last_accessed DESC
                ";
      await OpenConnectionAsync(SessionConnection);
      MySqlCommand command = new MySqlCommand(query, SessionConnection);
      command.Parameters.AddWithValue("projectNo", projectNo);
      command.Parameters.AddWithValue("disciplineId", disciplineId);

      MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
      if (reader.Read())
      {
        DateTime lastAccessed = GetSafeDateTime(reader, "last_accessed");

        if ((DateTime.Now - lastAccessed).TotalSeconds < 16)
        {
          activeUserId = GetSafeString(reader, "employee_id");
          activeUserName =
            GetSafeString(reader, "first_name") + " " + GetSafeString(reader, "last_name");

          await reader.CloseAsync();
        }
        else
        {
          await reader.CloseAsync();
          query =
            @"DELETE FROM sessions WHERE project_no = @projectNo AND discipline_id = @disciplineId";
          command = new MySqlCommand(query, SessionConnection);
          command.Parameters.AddWithValue("projectNo", projectNo);
          command.Parameters.AddWithValue("disciplineId", disciplineId);
          await command.ExecuteNonQueryAsync();
        }
      }
      else
      {
        await reader.CloseAsync();
      }
      await CloseConnectionAsync(SessionConnection);

      return (activeUserId, activeUserName);
    }

    public async Task UpdateSession(string sessionId, string projectNo, int disciplineId)
    {
      string id = Guid.NewGuid().ToString();
      string query =
        @"
                UPDATE sessions SET discipline_id = @disciplineId, project_no = @projectNo
                WHERE id = @sessionId
                ";
      await OpenConnectionAsync(SessionConnection);
      MySqlCommand command = new MySqlCommand(query, SessionConnection);
      command.Parameters.AddWithValue("disciplineId", disciplineId);
      command.Parameters.AddWithValue("projectNo", projectNo);
      command.Parameters.AddWithValue("sessionId", sessionId);
      await command.ExecuteNonQueryAsync();
      await CloseConnectionAsync(SessionConnection);
    }

    public async Task UpdateSessionLastAccessedDate(string sessionId)
    {
      string query =
        @"
                UPDATE sessions SET last_accessed = @lastAccessed WHERE id = @id
                ";
      await OpenConnectionAsync(SessionConnection);
      MySqlCommand command = new MySqlCommand(query, SessionConnection);
      command.Parameters.AddWithValue("id", sessionId);
      command.Parameters.AddWithValue("lastAccessed", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
      await command.ExecuteNonQueryAsync();
      await CloseConnectionAsync(SessionConnection);
    }

    public bool CreateEmployee(
      string username,
      string password,
      string emailAddr,
      string firstName,
      string lastName,
      ulong? phoneNumber,
      uint? extension,
      DateTime hireDate,
      string employeeId,
      string entityId,
      string contactId,
      string emailAddressId,
      string phoneNumberId
    )
    {
      OpenConnection(Connection);
      string query = "SELECT username FROM employees WHERE username = @username";

      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@username", username);

      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        MessageBox.Show($"A entry for {username} already exists.");
        reader.Close();
        return false;
      }
      reader.Close();

      var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
      var passhash = BCrypt.Net.BCrypt.HashPassword(password, salt);

      string emailAddrEntityRelId = Guid.NewGuid().ToString();
      string phoneNumberEntityRelId = Guid.NewGuid().ToString();

      query = "INSERT INTO entities (id) VALUES (@entityId)";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@entityId", entityId);
      command.ExecuteNonQuery();

      string gmepCompanyId = "cbc78dfd-7728-4162-a5f2-17e71b112f53";
      query =
        @"
                INSERT INTO contacts (id, entity_id, first_name, last_name, company_id)
                VALUES (@id, @entityId, @firstName, @lastName, @companyId)
                ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", contactId);
      command.Parameters.AddWithValue("@entityId", entityId);
      command.Parameters.AddWithValue("@firstName", firstName);
      command.Parameters.AddWithValue("@lastName", lastName);
      command.Parameters.AddWithValue("@companyId", gmepCompanyId);
      command.ExecuteNonQuery();

      query =
        @"
                INSERT INTO email_addresses (id, email_address)
                VALUES (@id, @emailAddress)
                ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", emailAddressId);
      command.Parameters.AddWithValue("@emailAddress", emailAddr);
      command.ExecuteNonQuery();

      query =
        @"
                INSERT INTO email_addr_entity_rel (id, email_address_id, entity_id, is_primary)
                VALUES (@id, @emailAddressId, @entityId, @isPrimary)
                ";
      command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", emailAddrEntityRelId);
      command.Parameters.AddWithValue("@emailAddressId", emailAddressId);
      command.Parameters.AddWithValue("@entityId", entityId);
      command.Parameters.AddWithValue("@isPrimary", 1);
      command.ExecuteNonQuery();

      if (phoneNumber != null && phoneNumber != 0)
      {
        query =
          @"
                INSERT INTO phone_numbers (id, phone_number, calling_code, extension)
                VALUES (@id, @phoneNumber, @callingCode, @extension)
                ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", phoneNumberId);
        command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
        command.Parameters.AddWithValue("@callingCode", 1);
        command.Parameters.AddWithValue("@extension", extension);
        command.ExecuteNonQuery();

        query =
          @"
                INSERT INTO phone_number_entity_rel (id, phone_number_id, entity_id, is_primary)
                VALUES (@id, @phoneNumberId, @entityId, @isPrimary)
                ";
        command = new MySqlCommand(query, Connection);
        command.Parameters.AddWithValue("@id", phoneNumberEntityRelId);
        command.Parameters.AddWithValue("@phoneNumberId", phoneNumberId);
        command.Parameters.AddWithValue("@entityId", entityId);
        command.Parameters.AddWithValue("@isPrimary", 1);
        command.ExecuteNonQuery();
      }

      query =
        @"
                INSERT INTO employees (id, contact_id, employee_title_id, employee_access_level_id, hire_date, username, passhash)
                VALUES (@id, @contactId, @employeeTitleId, @employeeAccessLevel, @hireDate, @username, @passhash)
                ";

      command = new MySqlCommand(query, Connection);

      command.Parameters.AddWithValue("@id", employeeId);
      command.Parameters.AddWithValue("@contactId", contactId);
      command.Parameters.AddWithValue("@employeeTitleId", 0);
      command.Parameters.AddWithValue("@employeeAccessLevel", 0);
      command.Parameters.AddWithValue("@hireDate", hireDate);
      command.Parameters.AddWithValue("@username", username);
      command.Parameters.AddWithValue("@passhash", passhash);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
      return true;
    }

    public string CreateProposal(
      Proposal p,
      string employeeId,
      int typeId,
      string projectId,
      string id = ""
    )
    {
      if (string.IsNullOrEmpty(id))
      {
        id = Guid.NewGuid().ToString();
      }

      if (string.IsNullOrEmpty(projectId))
      {
        projectId = CreateBlankProject();
        p.ProjectId = projectId;
        p.ProjectNo = "n" + projectId.Substring(0, 6);
      }
      string query =
        @"
        INSERT INTO proposals
        ( id,  rfp_date,  proposal_date,  project_id,  type_id,  employee_id) VALUES
        (@id, @rfp_date, @proposal_date, @project_id, @type_id, @employee_id)
        ";

      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", id);
      command.Parameters.AddWithValue("@rfp_date", p.RfpDate);
      command.Parameters.AddWithValue("@proposal_date", p.ProposalDate);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@type_id", typeId);
      command.Parameters.AddWithValue("@employee_id", employeeId);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
      p.New = false;
      return id;
    }

    public string CreateRfp(string projectId, string storedFilename)
    {
      string id = Guid.NewGuid().ToString();
      string query =
        @"
        INSERT INTO rfp 
        ( id,  project_id,  filename) VALUES
        (@id, @project_id, @filename)
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@id", id);
      command.Parameters.AddWithValue("@project_id", projectId);
      command.Parameters.AddWithValue("@filename", storedFilename);
      command.ExecuteNonQuery();
      CloseConnection(Connection);
      return id;
    }

    public string GetLatestRfpFilename(string projectId)
    {
      string filename = string.Empty;
      string query =
        @"
        SELECT filename FROM rfp WHERE project_id = @project_id ORDER BY date_created DESC LIMIT 1
        ";
      OpenConnection(Connection);
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@project_id", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      if (reader.Read())
      {
        filename = GetSafeString(reader, "filename");
      }
      reader.Close();
      CloseConnection(Connection);

      return filename;
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

    public async Task<bool> IsFileUploadedAsync(string key)
    {
      var response = await _s3Client.ListObjectsV2Async(
        new ListObjectsV2Request { BucketName = _bucketName, Prefix = key }
      );

      return response.S3Objects.Any(o => o.Key == key);
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
        Trace.WriteLine("File uploaded successfully.");
      }
      catch (AmazonS3Exception e)
      {
        Trace.WriteLine(
          "Error encountered on server. Message:'{0}' when writing an object",
          e.Message
        );
      }
      catch (Exception e)
      {
        Trace.WriteLine(
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
          Trace.WriteLine("File downloaded successfully.");
        }

        Process.Start(new ProcessStartInfo { FileName = downloadFilePath, UseShellExecute = true });
      }
      catch (AmazonS3Exception e)
      {
        Trace.WriteLine(
          "Error encountered on server. Message:'{0}' when reading an object.",
          e.Message
        );
      }
      catch (Exception e)
      {
        Trace.WriteLine(
          "Unknown encountered on server. Message:'{0}' when reading an object.",
          e.Message
        );
      }
    }

    public async Task DownloadFileAsync(string keyName, string downloadFilePath)
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
        var deleteRequest = new DeleteObjectRequest { BucketName = _bucketName, Key = keyName };

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
