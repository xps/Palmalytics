using Palmalytics.Utilities.Collections;

namespace Palmalytics.Tests.Utilities
{
    public class RingBufferTests
    {
        [Fact]
        public void Test_RingBuffer_Empty()
        {
            var buffer = new RingBuffer<int>(5);

            buffer.ToArray().Should().BeEmpty();
        }

        [Fact]
        public void Test_RingBuffer_Less_Than_Capacity()
        {
            var buffer = new RingBuffer<int>(5);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            buffer.ToArray().Should().Equal(1, 2, 3);
        }

        [Fact]
        public void Test_RingBuffer_At_Capacity()
        {
            var buffer = new RingBuffer<int>(5);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4);
            buffer.Add(5);

            buffer.ToArray().Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void Test_RingBuffer_More_Than_Capacity()
        {
            var buffer = new RingBuffer<int>(5);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4);
            buffer.Add(5);
            buffer.Add(6);
            buffer.Add(7);
            buffer.Add(8);
            buffer.Add(9);

            buffer.ToArray().Should().Equal(5, 6, 7, 8, 9);
        }
    }
}