using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory
{
    public class DictionaryPool<TKey, TValue> 
    {
        private readonly Queue<Dictionary<TKey, TValue>> _pool;
        private readonly int _capacity;

        public DictionaryPool(int initialCount, int capacity)
        {
            _capacity = capacity;
            _pool = new Queue<Dictionary<TKey, TValue>>();
            for (int i = 0; i < initialCount; i++)
            {
                _pool.Enqueue(new Dictionary<TKey, TValue>(capacity));
            }
        }

        public Dictionary<TKey, TValue> Rent()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Dequeue();
                }
                else
                {
                    return new Dictionary<TKey, TValue>(_capacity);
                }
            }
        }

        public void Return(Dictionary<TKey, TValue> dictionary)
        {
            lock (_pool)
            {
                dictionary.Clear();
                _pool.Enqueue(dictionary);
            }
        }
    }
}