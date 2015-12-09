using Rnx.Common.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Buffers
{
    public class BufferElementData : IBufferElementData
    {
        private readonly Dictionary<object, object> _dataEntries;

        public BufferElementData()
        {
            _dataEntries = new Dictionary<object, object>();
        }

        public void Add<T>(T data)
        {
            _dataEntries[typeof(T)] = data;
        }

        public void WhenExists<T>(Action<T> action)
        {
            object res;

            if (_dataEntries.TryGetValue(typeof(T), out res))
            {
                action((T)res);
            }
        }

        public bool Exists(object key) => _dataEntries.ContainsKey(key);

        public bool Exists<T>() => Exists(typeof(T));

        public object this[object key]
        {
            get { return _dataEntries[key]; }
            set { _dataEntries[key] = value; }
        }

        public T Get<T>()
        {
            object res;
            return _dataEntries.TryGetValue(typeof(T), out res) ? (T)res : default(T);
        }

        public void Remove<T>()
        {
            _dataEntries.Remove(typeof(T));
        }

        public void Remove(object key)
        {
            _dataEntries.Remove(key);
        }

        public object Clone()
        {
            var clone = new BufferElementData();

            foreach (var kvp in _dataEntries)
            {
                object clonedValue = kvp.Value is Common.Util.ICloneable ? ((Common.Util.ICloneable)kvp.Value).Clone() : kvp.Value;
                clone._dataEntries.Add(kvp.Key, clonedValue);
            }

            return clone;
        }
    }
}