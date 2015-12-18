using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Buffers
{
    /// <summary>
    /// Central part for the dataflow within Rnx
    /// </summary>
    public interface IBuffer : IDisposable
    {
        /// <summary>
        /// Notifies subscribers when the first element was added to the buffer.
        /// If no element is added to the buffer, this event will be raised when CompleteAdding is called.
        /// </summary>
        event EventHandler Ready;

        /// <summary>
        /// Notifies subscribers when all elements where added to the buffer.
        /// </summary>
        event EventHandler AddingComplete;

        /// <summary>
        /// Notifies subscribers when a new element was added to the buffer
        /// </summary>
        event EventHandler<IBufferElement> ElementAdded;

        /// <summary>
        /// Lets a consumer get the elements in the buffer. This is a blocking call, i.e.
        /// the call will return all elements that are currently in the buffer and will block
        /// until the producer is done adding new elements, i.e. till CompleteAdding is called
        /// </summary>
        IEnumerable<IBufferElement> Elements { get; }

        /// <summary>
        /// When parallel processing of elements is desired, a consumer needs to use ElementsPartitioner instead
        /// of Elements, for example Parallel.ForEach(buffer.ElementsPartitioner, element => ...)
        /// </summary>
        Partitioner<IBufferElement> ElementsPartitioner { get; }

        /// <summary>
        /// Adds a new element to the buffer
        /// </summary>
        void Add(IBufferElement element);

        /// <summary>
        /// Called from the producer to signal that all elements were added to the buffer
        /// </summary>
        void CompleteAdding();

        /// <summary>
        /// Copies the elements of the current buffer to a target buffer
        /// </summary>
        void CopyTo(IBuffer targetBuffer);
    }
}