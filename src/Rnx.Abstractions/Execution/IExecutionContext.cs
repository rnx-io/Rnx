using Rnx.Abstractions.Tasks;
using Rnx.Abstractions.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Execution
{
    /// <summary>
    /// Common contextual information that can be used by tasks
    /// </summary>
    public interface IExecutionContext
    {
        /// <summary>
        /// The name of the parent user defined task
        /// </summary>
        ITaskDescriptor RootTaskDescriptor { get; }

        /// <summary>
        /// All filesystem-based operations inside of tasks should be relative to this directory.
        /// For example, if a user specifies that all PDFs should be read from "MyDocuments/*.pdf" and the base directory
        /// is "c:/MyProject" then the task can resolve this as "c:/MyProject/MyDocuments/*.pdf"
        /// </summary>
        string BaseDirectory { get; }
    }
}