namespace Palmalytics.SqlServer.Tests
{
    public class PersistentDataSetFixture : IDisposable
    {
        private readonly SqlServerDataStore persistentDataStore;

        public PersistentDataSetFixture()
        {
            // Create a data store that persists between tests
            persistentDataStore = DataStoreHelpers.CreatePersistentDataStore();

            // Add test data
            var requests = DataSetHelpers.GetRequests();
            foreach (var request in requests)
                persistentDataStore.AddRequestAsync(request).Wait();
        }

        public void Dispose()
        {
            persistentDataStore.DropSchema();
        }
    }
}
