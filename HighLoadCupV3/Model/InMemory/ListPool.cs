using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory
{
    public class ListPool<T>
    {
        private readonly Queue<List<T>> _pool;
        private readonly int _capacity;

        public ListPool(int initialCount, int capacity)
        {
            _capacity = capacity;
            _pool = new Queue<List<T>>();
            for (int i = 0; i < initialCount; i++)
            {
                _pool.Enqueue(new List<T>(capacity));
            }
        }

        public List<T> Rent()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Dequeue();
                }
                else
                {
                    return new List<T>(_capacity);
                }
            }
        }

        public void Return(List<T> list)
        {
            lock (_pool)
            {
                list.Clear();
                _pool.Enqueue(list);
            }
        }
    }
}