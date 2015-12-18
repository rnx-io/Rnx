using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Buffers
{
    /// <summary>
    /// Factory-interface to create new <see cref="IBuffer"/>s
    /// </summary>
    public interface IBufferFactory
    {
        /// <summary>
        /// Creates a new <see cref="IBuffer"/> object
        /// </summary>
        IBuffer Create();
    }
}