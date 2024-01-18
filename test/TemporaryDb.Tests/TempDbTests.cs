using Xunit;

namespace TemporaryDb.Tests
{
    public class TempDbTests
    {
        [Fact]
        public void CreatesDbDuringConstruction()
        {
            var mock = new MockDatabase();

            // ReSharper disable once UnusedVariable
            var tempDb = new TempDb(mock);

            Assert.True(mock.CreateCalled);
            Assert.False(mock.DropCalled);
        }

        [Fact]
        public void DropsDatabaseWhenDisposed()
        {
            var mock = new MockDatabase();
            var db = new TempDb(mock);

            Assert.True(mock.CreateCalled);
            Assert.False(mock.DropCalled);

            db.Dispose();

            Assert.True(mock.DropCalled);
        }

        [Fact]
        public void ConnectionStringReturnsDatabaseConnectionString()
        {
            var mock = new MockDatabase();
            var db = new TempDb(mock);

            Assert.Equal(mock.ConnectionString, db.ConnectionString);
        }

        private class MockDatabase : Database
        {
            public bool CreateCalled { get; private set; }
            public bool DropCalled { get; private set; }

            public override void CreateDatabase()
            {
                CreateCalled = true;
            }

            public override void DropDatabase()
            {
                DropCalled = true;
            }

            public override string ConnectionString => "MockConnectionString";
        }
    }
}
