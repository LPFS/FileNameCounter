using Moq;
using Moq.AutoMock;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using NUnit.Framework.Internal;
using FileNameCounter.Implementations;
using FileNameCounter;
using FileNameCounter.Exceptions;

namespace TestProject
{
    public class ArgumentProcessorTests
    {
        private AutoMocker _autoMocker;
        private ArgumentProcessor _argumentProcessor;
        private IFileInfo _fileInfo_Simple;
        private IFileInfo _fileInfo_A_B;
        private IFileInfo _fileInfo_noDot;
        private IFileInfo _fileInfo_;

        [SetUp]
        public void Setup()
        {
            _autoMocker = new AutoMocker(MockBehavior.Loose, DefaultValue.Mock);

            var fileSystem = new MockFileSystem();

            _fileInfo_Simple = fileSystem.FileInfo.New("C:TestFiles/Simple.txt");
            _fileInfo_A_B = fileSystem.FileInfo.New("C:TestFiles/A.B.txt");
            _fileInfo_noDot = fileSystem.FileInfo.New("C:TestFiles/noDot");
            _fileInfo_ = fileSystem.FileInfo.New("C:TestFiles/.txt");


            fileSystem.AddFile(_fileInfo_Simple, new MockFileData("Content of Simple.txt"));
            fileSystem.AddFile(_fileInfo_A_B, new MockFileData("Content of A.B.txt"));
            fileSystem.AddFile(_fileInfo_noDot, new MockFileData("Content of noDot"));
            fileSystem.AddFile(_fileInfo_, new MockFileData("Content of file missing main part"));


            _autoMocker.Use<IFileSystem>(fileSystem);

            _argumentProcessor = _autoMocker.CreateInstance<ArgumentProcessor>();
        }

        private Task VerifySuccesfulCaseAsync(IFileInfo expectedFileInfo, string expectedTargetString)
        {
            var (actualFileInfo, actualTargetString) = _argumentProcessor.Process(new[] { expectedFileInfo.FullName });
            Assert.AreEqual(expectedFileInfo.FullName, actualFileInfo.FullName);
            Assert.AreEqual(expectedTargetString, actualTargetString);
            return Task.CompletedTask;
        }

        [Test]
        public Task Happy_case()
            => VerifySuccesfulCaseAsync(_fileInfo_Simple, "Simple");

        [Test]
        public Task Should_handle_dot_in_main_file_name()
            => VerifySuccesfulCaseAsync(_fileInfo_A_B, "A.B");

        [Test]
        public Task Should_handle_filename_has_no_dot() 
            => VerifySuccesfulCaseAsync(_fileInfo_noDot, "noDot");


        [TestCase(new string[] { "C:file:name.txt" }, Messages.Arguments.ContainsColon)]
        [TestCase(new[] { "one", "two" }, Messages.Arguments.Help, Description = "Too many arguments.")]
        [TestCase(new string[] { }, Messages.Arguments.Help, Description = "No arguments.")]
        [TestCase(new string[] { "noSuchFile.txt" }, Messages.Arguments.FileDoesNotExist)]
        public Task Should_throw(string[] arguments, string expectedMessage)
        {
            var e = Assert.Throws<ArgumentProcessorException>(() => _argumentProcessor.Process(arguments));
            Assert.AreEqual(expectedMessage, e!.Message);
            return Task.CompletedTask;
        }

        [Test]
        public Task Should_throw_for_no_main_part()
        {
            var e = Assert.Throws<ArgumentProcessorException>(() => _argumentProcessor.Process(new[] { _fileInfo_.FullName }));
            Assert.AreEqual(Messages.Arguments.NoMainPart, e!.Message);
            return Task.CompletedTask;
        }


        [Ignore("Not supported by MockFileSystem, file does not exist overrides.")]
        [Test]
        public Task Should_handle_filename_to_long()
        {
            return Task.CompletedTask;
        }

        [Ignore("Not supported by MockFileSystem")]
        [Test]
        public Task Should_handle_access_violations()
        {
            return Task.CompletedTask;
        }
    }
}
