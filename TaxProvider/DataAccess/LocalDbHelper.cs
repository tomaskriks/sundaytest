using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class LocalDbHelper : IDisposable
    {
        private string _connectionString;//     "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Tomas\\Documents\\Visual Studio 2017\\Projects\\TaxProvider\\DataAccess\\Database.mdf\";Integrated Security=True";


        private SqlConnection DbConnection { get; }

        public LocalDbHelper()
        {
            _connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _connectionString =
                    "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database.mdf;Integrated Security=True";
            }
            //if (_connectionString.Contains("|DataDirectory|"))
            //{
            //    string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //    string path = (System.IO.Path.GetDirectoryName(executable));
            //    AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //    string Path = Environment.CurrentDirectory;
            //    string[] appPath = Path.Split(new string[] { "bin" }, StringSplitOptions.None);
            //    AppDomain.CurrentDomain.SetData("DataDirectory", appPath[0]);
            //}

            DbConnection = new SqlConnection(_connectionString);
        }

        public LocalDbHelper(string connectionString)
        {
            _connectionString = connectionString;
            DbConnection = new SqlConnection(_connectionString);
        }

        public int ExecuteNonQuery(string query, bool storedProcedure = false, IDictionary<string,object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(query));

            int result = 0;

            using( var command = new SqlCommand(query, DbConnection)) { 
                try
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }

                    }
                    if (DbConnection.State != ConnectionState.Open)
                    {
                        DbConnection.Open();
                    }
                    if(storedProcedure){command.CommandType = CommandType.StoredProcedure;}

                    result = command.ExecuteNonQuery();
                }
                finally
                {
                    if (DbConnection.State == ConnectionState.Open)
                    {
                        DbConnection.Close();
                    }
                }
            }
            return result;
        }

        public DataSet GetData(string sqlSelectString, bool storedProcedure = false, IDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(sqlSelectString))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(sqlSelectString));

            var result = new DataSet();

            try
            {

                if (DbConnection.State != ConnectionState.Open)
                {
                    DbConnection.Open();
                }
                var adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(sqlSelectString,DbConnection);
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    if (storedProcedure) { adapter.SelectCommand.CommandType = CommandType.StoredProcedure; }

                }
                adapter.Fill(result);
                adapter.Dispose();
            }
            finally
            {
                if (DbConnection.State == ConnectionState.Open)
                {
                    DbConnection.Close();
                }
            }

            return result;
        }

        public void Dispose()
        {
            DbConnection.Dispose();
        }
    }
}
