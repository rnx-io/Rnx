using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using System.IO;
using Rnx.Tasks.Core.FileSystem;
using System.IO.Compression;

namespace Rnx.Tasks.Core.Compression.Internal
{
    public class ZipTaskInternal : ZipTask
    {
        private readonly IBufferElementFactory _bufferElementFactory;

        public ZipTaskInternal(IBufferElementFactory bufferElementFactory, string zipEntryFilePath)
            : base(zipEntryFilePath)
        {
            _bufferElementFactory = bufferElementFactory;
        }

        protected override IBufferElementFactory GetBufferElementFactory(IExecutionContext ctx) => _bufferElementFactory;
    }
}