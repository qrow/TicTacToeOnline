using System.Collections.Concurrent;
using System.Linq;

namespace TicTacToeOnline.Engine
{
    /// <summary>
    /// Abstracting data access and persistance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {
        bool TryAdd(string id, T item);
        bool TryGetValue(string id, out T item);
        void TryRemove(string id, out T item);
        IQueryable<T> Query();
    }

    /// <summary>
    /// Implements thread safe inmemory data storage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InMemoryRepository<T> : IRepository<T>
    {
        private readonly ConcurrentDictionary<string, T> _data;

        public InMemoryRepository()
        {
            _data = new ConcurrentDictionary<string, T>();
        }

        #region IRepository<T> Members

        public bool TryAdd(string id, T item)
        {
            return _data.TryAdd(id, item);
        }

        public bool TryGetValue(string id, out T item)
        {
            return _data.TryGetValue(id, out item);
        }

        public void TryRemove(string id, out T item)
        {
            _data.TryRemove(id, out item);
        }

        public IQueryable<T> Query()
        {
            return _data.Select(d => d.Value).AsQueryable();
        }

        #endregion
    }
}