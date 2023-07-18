using System.IO.Abstractions;

namespace FileNameCounter.Interfaces
{
    public interface IArgumentProcessor
    {
        (IFileInfo, string target) Process(string[] args);
    }
}
