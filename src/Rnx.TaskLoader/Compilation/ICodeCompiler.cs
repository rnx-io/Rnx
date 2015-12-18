using System.Reflection;

namespace Rnx.TaskLoader.Compilation
{
    /// <summary>
    /// Used to compile an <see cref="Assembly"/> from the specified sources codes that contain
    /// the specified task descriptors.
    /// </summary>
    public interface ICodeCompiler
    {
        Assembly Compile(params string[] sourceCodes);
    }
}