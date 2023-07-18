using FileNameCounter.Exceptions;
using FileNameCounter.Interfaces;
using System.IO.Abstractions;
using System.Security;
using static FileNameCounter.Messages.Arguments;

namespace FileNameCounter.Implementations
{
    /// <summary>
    /// This class is responsible for verifying the command
    /// line arguments and to construct a target string and
    /// a FileInfo, or to produce messages describing any faults.
    /// </summary>
    public class ArgumentProcessor : IArgumentProcessor
    {
        private readonly IFileSystem _fileSystem;

        public ArgumentProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }


        public (IFileInfo, string target) Process(string[] args)
        {
            if (args?.Length != 1)
                throw new ArgumentProcessorException(Help);

            IFileInfo file;
            try
            {
                file = _fileSystem.FileInfo.New(args[0].Trim());
            }
            catch (Exception e)
            {
                throw new ArgumentProcessorException(e switch
                {
                    SecurityException => NoAccess,
                    UnauthorizedAccessException => NoAccess,
                    PathTooLongException => PathTooLong,
                    NotSupportedException => ContainsColon,
                    _ => Unexpected
                });
            }

            if (!file.Exists)
                throw new ArgumentProcessorException(FileDoesNotExist);

            var targetString = file.Name[0..^file.Extension.Length];
            if (string.IsNullOrEmpty(targetString))
                throw new ArgumentProcessorException(NoMainPart);
            return (file, targetString);
        }
    }
}
