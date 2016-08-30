using System;

namespace TemporaryDb
{
    /// <summary>
    /// A disposable class that wraps a <see cref="Database"/> implementation by creating the database on construction and dropping it when disposed.
    /// </summary>
    public class TempDb : IDisposable
    {
        private readonly Database _database;

        /// <summary>
        /// Create a database from the given <see cref="Database"/> implementation
        /// </summary>
        /// <param name="database">The database implementation to wrap</param>
        public TempDb(Database database)
        {
            _database = database;
            _database.CreateDatabase();
        }

        /// <summary>
        /// Returns a connection string to the wrapped database.
        /// </summary>
        public string ConnectionString => _database.ConnectionString;

        /// <summary>
        /// Drops the database when the class is disposed/
        /// </summary>
        public void Dispose()
        {
            _database.DropDatabase();
        }
    }
}
