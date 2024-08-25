using Palmalytics.Extensions;

namespace Palmalytics.Tests.Extensions
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void Test_IEnumerableExtensions_Join()
        {
            // Arrange
            var data = new[] { "a", "b", "c" };

            // Act
            var joined = data.Join(", ");

            // Assert
            joined.Should().Be("a, b, c");
        }

        [Fact]
        public void Test_IEnumerableExtensions_Join_With_Select()
        {
            // Arrange
            var data = new[] { "a", "b", "c" };

            // Act
            var joined = data.Join(x => x.ToUpper(), ", ");

            // Assert
            joined.Should().Be("A, B, C");
        }
    }
}
