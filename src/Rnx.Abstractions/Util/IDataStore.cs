using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Util
{
    /// <summary>
    /// Used to maintain typed data
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Adds data registered with type of <paramref name="T"/>
        /// </summary>
        /// <param name="data">Instance of the data</param>
        void Add<T>(T data);

        /// <summary>
        /// Gets the data that was registered with key <paramref name="key"/>
        /// </summary>
        object this[object key] { get; set; }

        /// <summary>
        /// Gets the data that was registered with type of <paramref name="T"/>
        /// </summary>
        T Get<T>();

        /// <summary>
        /// Checks whether data exists for a given key
        /// </summary>
        /// <param name="key">The key to look for</param>
        bool Exists(object key);

        /// <summary>
        /// Checks whether a given type of data (<paramref name="T"/>) exists
        /// </summary>
        /// <typeparam name="T">The type of data to check</typeparam>
        bool Exists<T>();

        /// <summary>
        /// Executes a given <paramref name="action"/>, when the type of data (<paramref name="T"/>) exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        void WhenExists<T>(Action<T> action);

        /// <summary>
        /// Removes data that was registered with type of <paramref name="T"/>
        /// </summary>
        void Remove<T>();

        /// <summary>
        /// Removes data that was registered with key <paramref name="key"/>
        /// </summary>
        void Remove(object key);

        /// <summary>
        /// Clones the current data
        /// </summary>
        IDataStore Clone();
    }
}
