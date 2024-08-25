namespace Palmalytics.Utilities.Collections
{
    internal class RingBuffer<T>(int capacity)
    {
        private readonly T[] buffer = new T[capacity];

        private int position;
        private int count;

        public void Add(T item)
        {
            lock (buffer)
            {
                buffer[position++] = item;

                if (position >= buffer.Length)
                    position = 0;

                if (count < buffer.Length)
                    count++;
            }
        }

        public T[] ToArray()
        {
            lock (buffer)
            {
                var array = new T[count];

                var index = position - count;
                if (index < 0)
                    index += buffer.Length;

                for (int i = 0; i < count; i++)
                {
                    array[i] = buffer[index];
                    index = (index + 1) % buffer.Length;
                }

                return array;
            }
        }

        public int Count => count;
    }
}
