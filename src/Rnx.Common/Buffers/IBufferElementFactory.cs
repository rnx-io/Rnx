using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Buffers
{
    public interface IBufferElementFactory
    {
        IBufferElement Create(string text);
        IBufferElement Create(Func<Stream> streamCallback);
    }
}
