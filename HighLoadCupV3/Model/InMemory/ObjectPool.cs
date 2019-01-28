using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory
{
    public class ObjectPool<T> where T : IResetable, new()
    {
        public Queue<T> _pool;

        public ObjectPool(int initialCount)
        {
            _pool = new Queue<T>();
            for (int i = 0; i < initialCount; i++)
            {
                _pool.Enqueue(new T());
            }
        }

        public T Rent()
        {
            lock (_pool)
            {
                if(_pool.Count > 0)
                {
                    return _pool.Dequeue();
                }
                else
                {
                    return new T();
                }
            }
        }

        public void Return(T obj)
        {
            lock (_pool)
            {
                obj.Reset();
                _pool.Enqueue(obj);
            }
        }

        public void Return(IEnumerable<T> objects)
        {
            lock (_pool)
            {
                foreach (var obj in objects)
                {
                    obj.Reset();
                    _pool.Enqueue(obj);
                }
            }
        }
    }
}