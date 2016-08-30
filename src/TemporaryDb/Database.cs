namespace TemporaryDb
{
    /// <summary>
    /// Abstract base class that represents how to create and drop a database.  Different 
    /// database types (Sqlite, LocalDB, etc.) will derive from this class and implement their 
    /// own specific create/drop commands
    /// </summary>
    public abstract class Database
    {
        /// <summary>
        /// Creates the database
        /// </summary>
        public abstract void CreateDatabase();
        
        /// <summary>
        /// Drops the database and deletes its backing files.
        /// </summary>
        public abstract void DropDatabase();

        /// <summary>
        /// The connection string to use to connect to this database.
        /// </summary>
        public abstract string ConnectionString { get; }
    }
}