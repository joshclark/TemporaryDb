using System;
using System.IO;
using System.Threading;
using Microsoft.Data.SqlClient;
using Xunit;

namespace TemporaryDb.Tests
{
    public class LocalDbDatabaseTests : IDisposable
    {
        private readonly string _databaseName = "mydb-with-dashes";
        private readonly string _filename = "localdbFileName.mdf";
        private readonly string _instanceName = "my_instance";

        private LocalDbDatabase? _db;

        public void Dispose()
        {
            _db?.DropDatabase();
        }

        [Fact]
        public void ConnectionStringDefaultsInstanceNameWhenNotProvided()
        {
            var db = new LocalDbDatabase(_databaseName);
            var builder = new SqlConnectionStringBuilder(db.ConnectionString);

            Assert.Equal($"(localdb)\\{LocalDbDatabase.DefaultInstanceName}", builder.DataSource);
        }

        [Fact]
        public void ConnectionStringUsesPassedInInstanceName()
        {
            var db = new LocalDbDatabase(_databaseName, _filename, _instanceName);
            var builder = new SqlConnectionStringBuilder(db.ConnectionString);

            Assert.Equal($"(localdb)\\{_instanceName}", builder.DataSource);
        }

        [Fact]
        public void ConnectionStringUsesPassedInDatabaseName()
        {
            var db = new LocalDbDatabase(_databaseName);
            var builder = new SqlConnectionStringBuilder(db.ConnectionString);

            Assert.Equal(_databaseName, builder.InitialCatalog);
        }

        [Fact]
        public void FileNameUsesPassedInValue()
        {
            var db = new LocalDbDatabase(_databaseName, _filename);
            var expected = Path.Combine(Directory.GetCurrentDirectory(), _filename);
            Assert.Equal(expected, db.FileName);
        }

        [Fact]
        public void FilenameDefaultsToDatabaseName()
        {
            var db = new LocalDbDatabase(_databaseName);
            var expected = Path.Combine(Directory.GetCurrentDirectory(), _databaseName + ".mdf");
            Assert.Equal(expected, db.FileName);
        }

        [Fact]
        public void FileNameRootedAtCurrentDirectoryIfNotAlreadyRooted()
        {
            var db = new LocalDbDatabase(_databaseName, _filename);
            var expected = Path.Combine(Directory.GetCurrentDirectory(), _filename);
            Assert.Equal(expected, db.FileName);
        }

        [Fact]
        public void FileNameUsedAsIsWhenRooted()
        {
            var expected = Path.Combine(Path.GetTempPath(), _filename);
            var db = new LocalDbDatabase(_databaseName, expected);
            Assert.Equal(expected, db.FileName);
        }

        [Fact]
        public void MasterConnectionStringUsesAllValuesExceptDatabaseName()
        {
            var db = new LocalDbDatabase(_databaseName, _filename, _instanceName);
            var builder = new SqlConnectionStringBuilder(db.MasterConnectionString);

            Assert.Equal($"(localdb)\\{_instanceName}", builder.DataSource);
            Assert.Equal("master", builder.InitialCatalog);
        }

        [Fact]
        public void CreateDatabaseUsesCorrectFilename()
        {
            var filename = Path.Combine(Path.GetTempPath(), _filename);
            _db = new LocalDbDatabase(_databaseName, filename);

            Assert.False(File.Exists(filename));

            _db.CreateDatabase();

            Assert.True(File.Exists(filename));
        }

        [Fact]
        public void CreateDatabaseCreatesAUseableDatabase()
        {
            var filename = Path.Combine(Path.GetTempPath(), _filename);
            _db = new LocalDbDatabase(_databaseName, filename);

            _db.CreateDatabase();

            AssertUsableDatabase(_db.ConnectionString);
        }

        [Fact]
        public void CreateDatabaseWorksEvenIfFilesAlreadyExist()
        {
            var filename = Path.Combine(Path.GetTempPath(), _filename);
            File.AppendAllText(filename, "foo");

            _db = new LocalDbDatabase(_databaseName, filename);

            _db.CreateDatabase();

            AssertUsableDatabase(_db.ConnectionString);
        }

        [Fact]
        public void CreateDatabaseCreatesNewDatabaseIfDatabaseAlreadyExists()
        {
            var filename = Path.Combine(Path.GetTempPath(), _filename);
            _db = new LocalDbDatabase(_databaseName, filename);

            // Make sure database exists.
            _db.CreateDatabase();
            AssertUsableDatabase(_db.ConnectionString);

            // Now create it again.
            _db.CreateDatabase();
            AssertUsableDatabase(_db.ConnectionString);
        }

        [Fact]
        public void DropDatabaseRemovesDatabaseFromLocalDb()
        {
            var filename = Path.Combine(Path.GetTempPath(), _filename);
            _db = new LocalDbDatabase(_databaseName, filename);

            // Make sure database exists.
            _db.CreateDatabase();
            Assert.True(DatabaseExist(_db.MasterConnectionString, _databaseName));

            _db.DropDatabase();
            Assert.False(DatabaseExist(_db.MasterConnectionString, _databaseName));
        }

        [Fact]
        public void DropDatabaseRemovesFilesFromFileSystem()
        {
            var filename = Path.Combine(Path.GetTempPath(), _filename);
            _db = new LocalDbDatabase(_databaseName, filename);

            // Make sure database exists.
            _db.CreateDatabase();
            Assert.True(File.Exists(filename));

            _db.DropDatabase();
            Assert.False(File.Exists(filename));
        }

        [Fact]
        public void DropDatabaseHandlesDatabaseDoesNotExist()
        {
            var filename = Path.Combine(Path.GetTempPath(), _filename);
            _db = new LocalDbDatabase(_databaseName, filename);

            Assert.False(DatabaseExist(_db.MasterConnectionString, _databaseName));

            _db.DropDatabase();
        }

        [Fact]
        public void DropDatabaseIgnoresErrorsWhenDataFileDoesNotExist()
        {
            var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(directory);

            var filename = Path.Combine(directory, _filename);
            _db = new LocalDbDatabase(_databaseName, filename);

            _db.CreateDatabase();

            // Let the filesystem let go of the file so it isn't in use.
            Thread.Sleep(250);

            Directory.Delete(directory, true);

            _db = new LocalDbDatabase(_databaseName);

            _db.CreateDatabase();
            _db.DropDatabase();
        }

        private void AssertUsableDatabase(string connectionString)
        {
            var createTable = "CREATE TABLE Person (Name varchar(32) not null);";
            var insertPerson = "INSERT INTO Person (Name) VALUES ('Bob')";
            var selectCount = "SELECT COUNT(*) FROM Person";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(createTable, conn))
                    cmd.ExecuteNonQuery();

                using (var cmd = new SqlCommand(insertPerson, conn))
                    cmd.ExecuteNonQuery();

                using (var cmd = new SqlCommand(selectCount, conn))
                    Assert.Equal(1, (int) cmd.ExecuteScalar());
            }
        }

        private bool DatabaseExist(string connectionString, string databaseName)
        {
            var sql = $"SELECT COUNT(name) FROM sys.databases WHERE name = '{databaseName}'";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                    return (int)cmd.ExecuteScalar() == 1;
            }

        }


    }
}
