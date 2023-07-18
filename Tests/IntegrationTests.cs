
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using NUnit.Framework.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FileNameCounter;
using FileNameCounter.Interfaces;
using FileNameCounter.Implementations;

namespace TestProject
{
    public class IntegrationTests
    {
        private IFileInfo _fileInfo_Simple;
        private IFileInfo _fileInfo_A_B;
        private IFileInfo _fileInfo_noDot;
        private IFileInfo _fileInfo_;
        private IFileInfo _fileInfo_ababa;
        private IMain _main;



        [SetUp]
        public void Setup()
        {
            var fileSystem = new MockFileSystem();

            _fileInfo_Simple = fileSystem.FileInfo.New("C:TestFiles/Simple.txt");
            _fileInfo_A_B = fileSystem.FileInfo.New("C:TestFiles/A.B.txt");
            _fileInfo_noDot = fileSystem.FileInfo.New("C:TestFiles/noDot");
            _fileInfo_ = fileSystem.FileInfo.New("C:TestFiles/.txt");
            _fileInfo_ababa = fileSystem.FileInfo.New("C:TestFiles/ababa.txt");


            fileSystem.AddFile(_fileInfo_Simple, new MockFileData("Content of Simple.txt"));
            fileSystem.AddFile(_fileInfo_A_B, new MockFileData("Content of A.B.txt"));
            fileSystem.AddFile(_fileInfo_noDot, new MockFileData("Content of noDot"));
            fileSystem.AddFile(_fileInfo_, new MockFileData("Content of file missing main part"));
            fileSystem.AddFile(_fileInfo_ababa, new MockFileData(
                """
                abababa
                abab
                abababa
                ababa
                """));


            HostApplicationBuilder builder = ProgramHelper.SuitableForBothApplicationAndTest();

            builder.Services.AddSingleton<IFileSystem, MockFileSystem>(p => fileSystem);
           
            using IHost host = builder.Build();
            using IServiceScope serviceScope = host.Services.CreateScope();
            _main = serviceScope.ServiceProvider.GetRequiredService<IMain>();
        }

        private async Task VerifySuccesfulCase(IFileInfo fileInfo, string expectedTarget, int expectedNofMatches) 
            => Assert.AreEqual(
                Messages.Result.Successful(expectedNofMatches, expectedTarget, fileInfo.Name),
                await _main.Run(new string[] { fileInfo.FullName }));

        [Test]
        public async Task Happy_case()
            => await VerifySuccesfulCase(_fileInfo_Simple, "Simple", 1);

        [Test]
        public async Task Should_handle_dot_in_main_file_name()
            => await VerifySuccesfulCase(_fileInfo_A_B, "A.B", 1);

        [Test]
        public async Task Should_handle_filename_has_no_dot() 
            => await VerifySuccesfulCase(_fileInfo_noDot, "noDot", 1);

        [Test]
        public async Task Should_handle_a_bit_more_complex_case()
            => await VerifySuccesfulCase(_fileInfo_ababa, "ababa", 5);


        [TestCase(new string[] { "C:file:name.txt" }, Messages.Arguments.ContainsColon)]
        [TestCase(new[] { "one", "two" }, Messages.Arguments.Help, Description = "Too many arguments.")]
        [TestCase(new string[] { }, Messages.Arguments.Help, Description = "No arguments.")]
        [TestCase(new string[] { "noSuchFile.txt" }, Messages.Arguments.FileDoesNotExist)]
        public async Task Should_give_error_message(string[] arguments, string expected) 
            => Assert.AreEqual(expected, await _main.Run(arguments));

        [Test]
        public async Task Should_give_error_for_no_main_part() 
            => Assert.AreEqual(
                Messages.Arguments.NoMainPart, 
                await _main.Run(new[] { _fileInfo_.FullName }));
    }
}
