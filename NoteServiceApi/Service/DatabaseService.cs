using Microsoft.Data.SqlClient;
using NoteServiceApi.Models;
using System.Diagnostics;
using System.Security.Principal;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NoteServiceApi.Service
{
    public interface IDatabaseService
    {
        Task Configure();


        /// <summary>
        /// Check if the Note database exists, if not create it
        /// </summary>
        /// <returns></returns>
        Task<MethodResponse<int>> CheckDatabaseExists();

        /// <summary>
        /// Check if our test tables exist. If not, create them
        /// </summary>
        /// <returns></returns>
        Task<MethodResponse<int>> CheckTablesExist();

        /// <summary>
        /// Executes given sql and returns number of rows affected 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlParameters"></param>
        /// <param name="useDefault"></param>
        /// <returns></returns>
        Task<MethodResponse<int>> ExecuteNonQuery(string sql, List<SqlParameter>? sqlParameters = null, bool useDefault = false);

        /// <summary>
        /// Executes given sql and returns identity of last insert
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        Task<MethodResponse<int>> ExecuteInsert(string sql, List<SqlParameter>? sqlParameters = null);

        /// <summary>
        /// Execute sql to read record(s) and return them in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        Task<MethodResponse<List<T>>> ExecuteReader<T>(string sql, List<SqlParameter>? sqlParameters = null) where T : class, new();
    }


    public class DatabaseService : IDatabaseService
    {
        public readonly string DatabaseName = "NoteDB";

        private readonly IConfiguration _configuration;

        public DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// Executes given sql and returns number of rows affected 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        public async Task<MethodResponse<int>> ExecuteNonQuery(string sql, List<SqlParameter>? sqlParameters = null, bool useDefault = false)
        {
            var response = new MethodResponse<int>() { Success = true, Message = "", Result = 0 };
            try
            {
                var connString = _configuration.GetConnectionString($"{(useDefault ? "Default" : DatabaseName)}");
                using SqlConnection conn = new SqlConnection(connString);
                await conn.OpenAsync();
                using SqlCommand cmd = new SqlCommand(sql, conn);

                if (sqlParameters?.Any() == true)
                    cmd.Parameters.AddRange(sqlParameters.ToArray());

                response.Result = await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }

        /// <summary>
        /// Executes given sql and returns identity of last insert
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        public async Task<MethodResponse<int>> ExecuteInsert(string sql, List<SqlParameter>? sqlParameters = null)
        {
            var response = new MethodResponse<int>() { Success = true, Message = "", Result = 0 };
            try
            {
                sql += $"; SELECT SCOPE_IDENTITY();";

                var connString = _configuration.GetConnectionString($"{DatabaseName}");
                using SqlConnection conn = new SqlConnection(connString);
                await conn.OpenAsync();
                using SqlCommand cmd = new SqlCommand(sql, conn);

                if (sqlParameters?.Any() == true)
                    cmd.Parameters.AddRange(sqlParameters.ToArray());

                response.Result = ((int?)(decimal?)await cmd.ExecuteScalarAsync()) ?? 0;
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }

        /// <summary>
        /// Execute sql to read record(s) and return them in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        public async Task<MethodResponse<List<T>>> ExecuteReader<T>(string sql, List<SqlParameter>? sqlParameters = null) where T: class, new()
        {
            var response = new MethodResponse<List<T>>() { Success = true, Message = "", Result = null };
            try
            {
                var connString = _configuration.GetConnectionString($"{DatabaseName}");
                using SqlConnection conn = new SqlConnection(connString);
                await conn.OpenAsync();
                using SqlCommand cmd = new SqlCommand(sql, conn);

                if (sqlParameters?.Any() == true)
                    cmd.Parameters.AddRange(sqlParameters.ToArray());

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                response.Result = MapToList<T>(reader);
            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }




        /// <summary>
        /// Check if the Note database exists, if not create it
        /// </summary>
        /// <returns></returns>

        public async Task<MethodResponse<int>> CheckDatabaseExists()
        {
            var sql = $"IF NOT EXISTS (SELECT* FROM sys.databases WHERE name = '{DatabaseName}') BEGIN CREATE DATABASE [{DatabaseName}] END";
            return await ExecuteNonQuery(sql, null, true);
        }


        /// <summary>
        /// Check if our test tables exist. If not, create them
        /// </summary>
        /// <returns></returns>
        public async Task<MethodResponse<int>> CheckTablesExist()
        {            
            var response = new MethodResponse<int>() { Success = true, Message = "" };
            try
            {                
                var sql = @"IF NOT EXISTS (SELECT* FROM sysobjects WHERE name = 'Owner' and xtype = 'U') BEGIN 
                              CREATE TABLE [Owner] (Id INT PRIMARY KEY IDENTITY(1,1), [Name] varchar(100),
                                                   [CreatedBy] varchar(100), [CreatedOn] DATETIME2(7) DEFAULT CURRENT_TIMESTAMP,
                                                   [LastModifiedBy] varchar(100), [LastModified] DATETIME2(7) DEFAULT CURRENT_TIMESTAMP,
                                                   [Active] bit NOT NULL DEFAULT 1)
                            END";
                response = await ExecuteNonQuery(sql);
                
                if (response.Success)
                {
                    var sql2 = @"IF NOT EXISTS (SELECT* FROM sysobjects WHERE name = 'Note' and xtype = 'U') BEGIN 
                              CREATE TABLE [Note] (Id INT PRIMARY KEY IDENTITY(1,1), OwnerId INT NOT NULL CONSTRAINT fk_owner FOREIGN KEY REFERENCES Owner(Id),
                                                   [Title] varchar(100), [Text] varchar(max), 
                                                   [CreatedBy] varchar(100), [CreatedOn] DATETIME2(7) DEFAULT CURRENT_TIMESTAMP,
                                                   [LastModifiedBy] varchar(100), [LastModified] DATETIME2(7) DEFAULT CURRENT_TIMESTAMP,
                                                   [Active] bit NOT NULL DEFAULT 1)
                            END";
                    var response2 = await ExecuteNonQuery(sql2);
                    response.Result += response2.Result;
                    response.SetFailedIfSubFailed(response2);
                }

                if (response.Success)
                {
                    var sql3 = @$"IF NOT EXISTS (SELECT * FROM [Owner]) BEGIN 
                                  INSERT INTO [Owner] ([Name], [CreatedBy])
                                    SELECT 'Bo Nix', 'Initial Load' UNION 
                                    SELECT 'Christian McCaffrey', 'Initial Load' UNION 
                                    SELECT 'Amon-Ra St. Brown', 'Initial Load' UNION 
                                    SELECT 'Trey McBride', 'Initial Load' UNION 
                                    SELECT 'Lindsey Vonn', 'Initial Load' UNION 
                                    SELECT 'Serena Williams', 'Initial Load' 
                                 END;";

                    var response3 = await ExecuteNonQuery(sql3);
                    response.Result += response3.Result;
                    response.SetFailedIfSubFailed(response3);
                }

            }
            catch (Exception ex)
            {
                response.SetToException(ex);
            }
            return response;
        }


        public static List<T> MapToList<T>(SqlDataReader reader) where T : new()
        {
            var list = new List<T>();
            var properties = typeof(T).GetProperties();

            while (reader.Read())
            {
                T obj = new T();
                foreach (var prop in properties)
                {
                    // Check if property name matches column name
                    if (reader.GetName(reader.GetOrdinal(prop.Name)) == prop.Name)
                    {
                        var value = reader[prop.Name];
                        // Handle DBNull
                        if (value != DBNull.Value)
                        {
                            prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType), null);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }


        public async Task Configure()
        {
            await CheckDatabaseExists();
            await CheckTablesExist();
        }

    }
}
