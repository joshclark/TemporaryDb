using System.Data.SqlClient;
using System.IO;

namespace TemporaryDb
{
    /// <summary>
    /// A <see cref="Database"/> implementation of SQL Server LocalDb.
    /// </summary>
    public class LocalDbDatabase : Database
    {
        /// <summary>
        /// The default instance to be used if one is not provided.
        /// </summary>
        public const string DefaultInstanceName = "MSSQLLocalDB"; // or "v11.0"

        private readonly string _databaseName;
        private readonly string _instanceName;
        private readonly string _fileName;

        /// <summary>
        /// Create a LocalDB database
        /// </summary>
        /// <param name="databaseName">The name of the database to create</param>
        /// <param name="fileName">The filename on disk for the database.  If this is not an absolute path, 
        /// the file will be relative to the current directory.  This defaults to the database name with a ".mdf" extension</param>
        /// <param name="instanceName">The LocalDB instance to use.  Defaults to "MSSQLLocalDB" if not specified. </param>
        public LocalDbDatabase(string databaseName, string fileName = null, string instanceName = null)
        {
            _databaseName = databaseName;
            _fileName = fileName ?? $"{databaseName}.mdf";
            _instanceName = instanceName ?? DefaultInstanceName;

            if (!Path.IsPathRooted(_fileName))
                _fileName = Path.Combine(Directory.GetCurrentDirectory(), _fileName);
        }


        /// <inheritdoc />
        public override string ConnectionString => CreateConnectionString(_databaseName);

        /// <summary>
        /// A connection string to the "master" database for this instance.
        /// </summary>
        public string MasterConnectionString => CreateConnectionString();

        /// <summary>
        /// The filename of the database file that will be used.
        /// </summary>
        public string FileName => _fileName;

        /// <inheritdoc />
        public override void CreateDatabase()
        {
            DropDatabase();

            string createDatabase = $"CREATE DATABASE [{_databaseName}] ON PRIMARY (NAME=[{_databaseName}], FILENAME = '{_fileName}')";
            using (var connection = new SqlConnection(MasterConnectionString))
            using (var command = new SqlCommand(createDatabase, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        /// <inheritdoc />
        public override void DropDatabase()
        {
            var deleteIfExists = $@"
IF EXISTS(SELECT * FROM sys.databases where name = '{_databaseName}')
BEGIN
    ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{_databaseName}]
END";
            using (var connection = new SqlConnection(MasterConnectionString))
            using (var command = new SqlCommand(deleteIfExists, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }

            if (File.Exists(_fileName))
                File.Delete(_fileName);
        }

        private string CreateConnectionString(string databaseName = "master")
        {
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = $"(localdb)\\{_instanceName}",
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
            };

            return builder.ConnectionString;
        }
    }
}