namespace Palmalytics.Tests.Configuration
{
    public class ConfigurationTests
    {
        [Fact]
        public void Test_Configuration_UserDataStore_Throws_If_DataStore_Doesnt_Implement_IDataStore()
        {
            // Arrange
            var options = new PalmalyticsOptions();

            // Act
            Action act = () => options.UseDataStore(typeof(ConfigurationTests));

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("type")
                .WithMessage($"Type {typeof(ConfigurationTests)} must implement IDataStore (Parameter 'type')");
        }
    }
}
