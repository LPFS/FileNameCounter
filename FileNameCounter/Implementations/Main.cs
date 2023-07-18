using FileNameCounter.Exceptions;
using FileNameCounter.Interfaces;
using Microsoft.Extensions.Hosting;
using System.IO.Abstractions;
using System.Security;

namespace FileNameCounter.Implementations
{
    public class Main : IMain
    {
        private readonly IArgumentProcessor _argumentProcessor;
        private readonly IStringInstanceCounterFactory _stringInstanceCounterFactory;

        public Main(
            IArgumentProcessor argumentProcessor,
            IStringInstanceCounterFactory stringInstanceCounterFactory)
        {
            _argumentProcessor = argumentProcessor;
            _stringInstanceCounterFactory = stringInstanceCounterFactory;
        }

        public async Task<string> Run(string[] args)
        {
            try
            {
                IFileInfo fileInfo;
                string target;
                try
                {
                    (fileInfo, target) = _argumentProcessor.Process(args);
                }
                catch (ArgumentProcessorException e)
                {
                    return e.Message;
                }

                long noOfInstances;
                try
                {
                    // The comment for OpenText is actually wrong, UTF-8 is default but BOM is considered.
                    using var reader = fileInfo.OpenText();
                    var counter = _stringInstanceCounterFactory.Create(target);
                    noOfInstances = await counter.CountAsync(reader);
                }
                catch (Exception e)
                {
                    return e switch
                    {
                        SecurityException => Messages.Processing.UnexpectedNoAccess,
                        UnauthorizedAccessException => Messages.Processing.UnexpectedNoAccess,
                        FileNotFoundException => Messages.Processing.UnexpectedNoFile,
                        DirectoryNotFoundException => Messages.Processing.UnexpectedNoFile,
                        IOException => Messages.Processing.ProblemReading,
                        ObjectDisposedException => Messages.Processing.ProblemReading,
                        _ => Messages.UnknownError
                    };
                }
                return Messages.Result.Successful(noOfInstances, target, fileInfo.Name);
            }

            catch
            {
                return Messages.UnknownError;
            }
        }
    }
}