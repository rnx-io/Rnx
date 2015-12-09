using System.IO;

namespace Rnx.Common.Buffers
{
    public interface IBufferElement
    {
        IBufferElementData Data { get; }
        Stream Stream { get; set; }
        string Text { get; set; }
        bool HasText { get; }
        IBufferElement Clone();
    }
}