using System.IO;

namespace Rnx.Abstractions.Buffers
{
    /// <summary>
    /// Represents the element that flows through the pipeline/buffers.
    /// </summary>
    public interface IBufferElement
    {
        /// <summary>
        /// Contains additional data for the element. Stages in the pipeline can add data
        /// which can then later be extracted by subsequent stages
        /// </summary>
        IBufferElementData Data { get; }

        /// <summary>
        /// Represents the underlying Stream for this element, e.g. a FileStream for a file-based element.
        /// This property is mostly called for binary transformations (e.g. compression) or for copying data
        /// from one stream to another.
        /// If a task is operating on text only (e.g. replacing a string), the Text-property should be used.
        /// </summary>
        Stream Stream { get; set; }

        /// <summary>
        /// Gets the textual representation of the current element
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Returns whether the Text of the element was set from the outside or already read from the underlying stream.
        /// This does NOT mean that the underlying stream does not contain text.
        /// </summary>
        bool HasText { get; }

        /// <summary>
        /// Creates a clone of the current element
        /// </summary>
        IBufferElement Clone();
    }
}