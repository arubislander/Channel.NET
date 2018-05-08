using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ArubIslander.Collections.Generic
{
    public class ChannelClosedException : Exception { }

    public interface InChannel<T> : IEnumerable<T> {
        void Put(T item);
        void PutRange(IEnumerable<T> items);
        void Close();
    }

    public interface OutChannel<T> : IEnumerable<T> {
        T Receive();
    }

    public class Channel<T> : InChannel<T>, OutChannel<T>, IEnumerable<T>, IDisposable
    {
        BlockingCollection<T> _store;

        public Channel(int maxCapacity)
        {
            _store = new BlockingCollection<T>(maxCapacity);
        }

        public void Put(T item)
        {
            if (_store.IsAddingCompleted)
                throw new ChannelClosedException();

            _store.Add(item);
        }

        public void PutRange(IEnumerable<T> items)
        {
            if (_store.IsAddingCompleted)
                throw new ChannelClosedException();

            foreach (var item in items)
            {
                _store.Add(item);
            }
        }

        public T Receive()
        {
            if (_store.TryTake(out T value))
                return value;

            return default(T);
        }

        public void Close()
        {
            if (!_store.IsAddingCompleted)
                _store.CompleteAdding();
        }

        public int Count { get => _store.Count; }

        public bool IsClosed { get { return _store.IsCompleted; } }

        void IDisposable.Dispose() => Close();

        public IEnumerator<T> GetEnumerator() => _store.GetConsumingEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _store.GetConsumingEnumerable().GetEnumerator();
    }
}
