using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Util;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System;
using Rnx.Abstractions.Execution.Decorators;
using System.Collections.Generic;
using System.Linq;

namespace Rnx.Abstractions.Tasks
{
    /// <summary>
    /// Convinient base class for tasks
    /// </summary>
    public abstract class RnxTask : ITask, ITaskDecorationProvider
    {
        public virtual IEnumerable<ITaskDecorator> TaskDecorators { get; set; } = Enumerable.Empty<ITaskDecorator>();

        public virtual string Name => GetType().Name;
        public override string ToString() => Name;

        public abstract void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}