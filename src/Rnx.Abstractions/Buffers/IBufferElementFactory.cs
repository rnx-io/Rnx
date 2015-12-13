using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Buffers
{
    /// <summary>
    /// Factory-interface to create new <see cref="IBufferElement"/>s
    /// </summary>
    public interface IBufferElementFactory
    {
        /// <summary>
        /// Creates a new IBufferElement and assigns it inital text information
        /// </summary>
        IBufferElement Create(string text);

        /// <summary>
        /// Creates a new IBufferElement with a callback to a Stream
        /// </summary>
        IBufferElement Create(Func<Stream> streamCallback);
    }
}