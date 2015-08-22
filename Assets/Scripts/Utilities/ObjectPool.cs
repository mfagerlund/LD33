using System;
using System.Collections.Generic;

namespace CrowdPleaser.Utilities
{
    public class ObjectPool<T> where T : class
    {
        // List is faster than queues, linked lists and pretty much anything...
        private readonly Func<T> _constructor;
        private readonly List<T> _pool = new List<T>(100);

        public ObjectPool(Func<T> constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }

            _constructor = constructor;
            _pool = new List<T>();
        }

        public int PooledItemsCount
        {
            get
            {
                lock (_pool)
                {
                    return _pool.Count;
                }
            }
        }

        public T GetFromObjectPoolOrCreateItem(Action<T> initiator)
        {
            T item = null;
            lock (_pool)
            {
                if (_pool.Count>0)
                {
                    item = _pool[_pool.Count-1];
                    _pool.RemoveAt(_pool.Count - 1);
                }
            }
            if (item == null)
            {
                item = _constructor();
            }
            if (initiator != null)
            {
                initiator(item);
            }

            return item;
        }

        public void ReturnItem(T item)
        {
            lock (_pool)
            {
                _pool.Add(item);
            }
        }
    }  
}