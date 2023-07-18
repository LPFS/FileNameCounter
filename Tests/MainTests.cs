using Moq;
using Moq.AutoMock;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using NUnit.Framework.Internal;
using FileNameCounter.Interfaces;
using FileNameCounter.Implementations;
using FileNameCounter;
using FileNameCounter.Exceptions;

namespace TestProject
{
    public class MainTests
    {
        private AutoMocker _autoMocker;
        private Mock<IArgumentProcessor> _processor;
        private IFileInfo _fileInfo;

        [SetUp]
        public void Setup()
        {
            _autoMocker = new AutoMocker(MockBehavior.Loose, DefaultValue.Mock);
            _processor = _autoMocker.GetMock<IArgumentProcessor>();
            var fileSystem = new MockFileSystem();
            _fileInfo = fileSystem.FileInfo.New("C:TestFiles/file.txt");
            fileSystem.AddFile(_fileInfo, new MockFileData("Content of file.txt"));
            _autoMocker.Use<IFileSystem>(fileSystem);
        }


        [Test]
        public async Task Happy_case()
        {
            var expectedNofMatches = 5;
            var expectedTargetString = "target";

            _processor.Setup(p => p.Process(It.IsAny<string[]>())).Returns((_fileInfo, expectedTargetString));

            var stringCounter = _autoMocker.GetMock<IStringInstanceCounter>();

            stringCounter
                .Setup(c => c.CountAsync(It.IsAny<TextReader>()))
                .ReturnsAsync(expectedNofMatches);

            _autoMocker.GetMock<IStringInstanceCounterFactory>()
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(stringCounter.Object);


            var main = _autoMocker.CreateInstance<Main>();
            Assert.AreEqual(Messages.Result.Successful(expectedNofMatches, expectedTargetString, _fileInfo.Name),
                await main.Run(Array.Empty<string>()));
        }

        [Test]
        public async Task Should_return_error_message_when_processor_throws()
        {
            var expected = "Some message";
            _processor.Setup(p => p.Process(It.IsAny<string[]>()))
                .Throws<ArgumentProcessorException>(() => new ArgumentProcessorException(expected));
            Assert.AreEqual(expected, await _autoMocker.CreateInstance<Main>().Run(Array.Empty<string>()));
        }
    }
}
