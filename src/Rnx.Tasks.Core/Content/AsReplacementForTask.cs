using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Exceptions;

namespace Rnx.Tasks.Core.Content
{
    public class AsReplacementForTaskDescriptor : TaskDescriptorBase<AsReplacementForTask>
    {
        internal string PlaceHolder { get; }
        internal string TemplateContent { get; }
        internal ITaskDescriptor TemplateContentProvidingTaskDescriptor { get; }

        public AsReplacementForTaskDescriptor(string placeHolder, string templateContent)
        {
            PlaceHolder = placeHolder;
            TemplateContent = templateContent;
        }

        public AsReplacementForTaskDescriptor(string placeHolder, ITaskDescriptor templateContentProvidingTaskDescriptor)
        {
            PlaceHolder = placeHolder;
            TemplateContentProvidingTaskDescriptor = templateContentProvidingTaskDescriptor;
        }
    }

    public class AsReplacementForTask : RnxTask
    {
        private readonly AsReplacementForTaskDescriptor _taskDescriptor;
        private readonly ITaskExecuter _taskExecuter;
        private readonly IBufferFactory _bufferFactory;

        public AsReplacementForTask(AsReplacementForTaskDescriptor taskDescriptor, ITaskExecuter taskExecuter, IBufferFactory bufferFactory)
        {
            _taskDescriptor = taskDescriptor;
            _taskExecuter = taskExecuter;
            _bufferFactory = bufferFactory;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var templateContent = _taskDescriptor.TemplateContent;

            if(_taskDescriptor.TemplateContentProvidingTaskDescriptor != null)
            {
                using (var outputBuffer = _bufferFactory.Create())
                {
                    _taskExecuter.Execute(_taskDescriptor.TemplateContentProvidingTaskDescriptor, new NullBuffer(), outputBuffer, executionContext);
                    var templateContentProvidingElement = outputBuffer.Elements.FirstOrDefault();

                    if(templateContentProvidingElement == null)
                    {
                        throw new RnxException($"Invalid task descriptor. The provided task descriptor for {nameof(AsReplacementForTask)} hasn't yielded any elements.");
                    }

                    templateContent = templateContentProvidingElement.Text;
                }
            }

            if(templateContent == null)
            {
                throw new NullReferenceException("The provided template content must not be null");
            }

            foreach(var e in input.Elements)
            {
                e.Text = templateContent.Replace(_taskDescriptor.PlaceHolder, e.Text);
                output.Add(e);
            }
        }
    }
}