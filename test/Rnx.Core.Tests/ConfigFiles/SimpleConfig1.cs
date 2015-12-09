
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using Rnx.Core.Tests;
using System;

public class SimpleConfig1
{
    public SimpleConfig1(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new Exception("Dependency injection failed");
    }

    public ITask Minify => new TestTask();

    public ITask DoStuff()
    {
        return new TestMultiTask(
                    new TestTask(),
                    new TestMultiTask(
                        new TestTask(),
                        Minify
                        )
                    );
    }
}