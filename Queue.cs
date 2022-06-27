using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Devcoons.Misc
{
    public class Queue<T>
    {
        private readonly object _Locker = new object();

        private System.Collections.Concurrent.ConcurrentQueue<T> _Queue = new System.Collections.Concurrent.ConcurrentQueue<T>();

        public void Enqueue(T item)
        {

            Thread.MemoryBarrier();
            lock (_Locker)
            {
                _Queue.Enqueue(item);
            }
            Thread.MemoryBarrier();
        }

        public virtual void EnqueueRange(IEnumerable<T> items)
        {
            lock (_Locker)
            {
                if (items == null)
                    return;

                foreach (T item in items)
                    _Queue.Enqueue(item);
            }
        }

        public virtual T[] DequeueAll()
        {
            lock (_Locker)
            {
                if (_Queue.Count == 0)
                    return null;

                T[] result = _Queue.Take(_Queue.Count).ToArray();
                _Queue = new System.Collections.Concurrent.ConcurrentQueue<T>();
                return result;
            }
        }

        public void Clear()
        {
            lock (_Locker)
            {
                _Queue = new System.Collections.Concurrent.ConcurrentQueue<T>();
            }
        }


        public Int32 Count
        {
            get
            {
                lock (_Locker)
                {
                    return _Queue.Count;
                }
            }
        }

        public Boolean TryDequeue(out T item)
        {
            Thread.MemoryBarrier();

            if (System.Threading.Monitor.TryEnter(_Locker, 20))
            {
                try
                {
                    Thread.MemoryBarrier();
                    if (_Queue.TryDequeue(out item) == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                finally
                {
                    System.Threading.Monitor.Exit(_Locker);
                }
            }
            item = default(T);
            return false;
        }
    }
}
