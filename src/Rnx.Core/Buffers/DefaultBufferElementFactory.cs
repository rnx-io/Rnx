using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Rnx.Common.Buffers;

namespace Rnx.Core.Buffers
{
    public class DefaultBufferElementFactory : IBufferElementFactory
    {
        public IBufferElement Create(Func<Stream> streamCallback)
        {
            return new BufferElement(streamCallback);
        }

        public IBufferElement Create(string text)
        {
            return new BufferElement(text);
        }
    }
}
