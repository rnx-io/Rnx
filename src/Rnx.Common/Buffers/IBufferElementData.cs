using System;

namespace Rnx.Common.Buffers
{
    public interface IBufferElementData : Util.ICloneable
    {
        void Add<T>(T data);
        object this[object key] { get; set; }
        T Get<T>();
        bool Exists(object key);
        bool Exists<T>();
        void WhenExists<T>(Action<T> action);
        void Remove<T>();
        void Remove(object key);
    }
}