namespace TemporaryDb
{
    /// <summary>
    /// Creates a LocalDb database on construction and drops the database when the class is disposed.
    /// </summary>
    public class TempLocalDb : TempDb
    {
        /// <summary>
        /// Create a LocalDb database with the given name
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        public TempLocalDb(string databaseName) 
            : base(new LocalDbDatabase(databaseName))
        {
        }
    }
}