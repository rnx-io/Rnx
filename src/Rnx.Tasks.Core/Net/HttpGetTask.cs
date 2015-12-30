using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using System.Net.Http;
using System.IO;
using Rnx.Tasks.Core.FileSystem;

namespace Rnx.Tasks.Core.Net
{
    public class HttpGetTaskDescriptor : TaskDescriptorBase<HttpGetTask>
    {
        internal string Uri { get; }
        internal string FilePath { get; }

        public HttpGetTaskDescriptor(string uri, string filePath = null)
        {
            Uri = uri;
            FilePath = filePath;
        }
    }

    public class HttpGetTask : RnxTask
    {
        private readonly HttpGetTaskDescriptor _taskDescriptor;
        private readonly IBufferElementFactory _bufferElementFactory;

        public HttpGetTask(HttpGetTaskDescriptor taskDescriptor, IBufferElementFactory bufferElementFactory)
        {
            _taskDescriptor = taskDescriptor;
            _bufferElementFactory = bufferElementFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            Stream responseStream = null;

            using (var httpClient = new HttpClient())
            {
                responseStream = httpClient.GetStreamAsync(_taskDescriptor.Uri).Result;
            }
            
            var ms = new MemoryStream();

            using (responseStream)
            {
                responseStream.CopyTo(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);

            var e = _bufferElementFactory.Create(() => ms);

            if(!string.IsNullOrWhiteSpace(_taskDescriptor.FilePath))
            {
                e.Data.Add(new WriteFileData(_taskDescriptor.FilePath));
            }

            output.Add(e);
        }
    }
}