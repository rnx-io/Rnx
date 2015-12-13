using System.Reflection;

namespace Rnx.TaskLoader.Compilation
{
    public interface ICodeCompiler
    {
        Assembly Compile(params string[] sourceCodes);
    }
}