
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using NUnit.Framework.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FileNameCounter;
using FileNameCounter.Interfaces;
using FileNameCounter.Implementations;
using System.Text;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace TestProject
{
    public class IntegrationTests
    {
        private IFileInfo _fileInfo_Simple;
        private IFileInfo _fileInfo_A_B;
        private IFileInfo _fileInfo_noDot;
        private IFileInfo _fileInfo_;
        private IFileInfo _fileInfo_ababa;
        private IFileInfo _fileInfo_empty;
        private IMain _main;
        private MockFileSystem _fileSystem;



        [SetUp]
        public void Setup()
        {
            _fileSystem = new MockFileSystem();

            _fileInfo_Simple = _fileSystem.FileInfo.New("C:TestFiles/Simple.txt");
            _fileInfo_A_B = _fileSystem.FileInfo.New("C:TestFiles/A.B.txt");
            _fileInfo_noDot = _fileSystem.FileInfo.New("C:TestFiles/noDot");
            _fileInfo_ = _fileSystem.FileInfo.New("C:TestFiles/.txt");
            _fileInfo_ababa = _fileSystem.FileInfo.New("C:TestFiles/ababa.txt");
            _fileInfo_empty = _fileSystem.FileInfo.New("C:TestFiles/empty.txt");



            _fileSystem.AddFile(_fileInfo_Simple, new MockFileData("Simple"));
            _fileSystem.AddFile(_fileInfo_A_B, new MockFileData("Content of A.B.txt"));
            _fileSystem.AddFile(_fileInfo_noDot, new MockFileData("Content of noDot"));
            _fileSystem.AddFile(_fileInfo_, new MockFileData("Content of file missing main part"));
            _fileSystem.AddFile(_fileInfo_empty, new MockFileData(string.Empty));
            _fileSystem.AddFile(_fileInfo_ababa, new MockFileData(
                """
                abababa
                abab
                abababa
                ababa
                """));


            HostApplicationBuilder builder = ProgramHelper.SuitableForBothApplicationAndTest();

            builder.Services.AddSingleton<IFileSystem, MockFileSystem>(p => _fileSystem);
           
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
        public async Task Should_handle_empty_file()
            => await VerifySuccesfulCase(_fileInfo_empty, "empty", 0);

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

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [Ignore("Heavy test, perform occasionally")]
        public async Task Verify_random(int seed)
        {
            var sb = new StringBuilder();
            var rand = new Random(seed);
            var nofInserts = rand.Next(0, 1000);
            //var target = new string('a', rand.Next(1, 30));

            var target = "abab";
            var insertString = target + target; //abababab
            var expectedHits = nofInserts * 3;
            var expectedRegexHits = 2 * nofInserts;

            var fileName = $"{target}.txt";
            var fullFileName = $"C:TestFiles/{fileName}";
            var r = new Regex(Regex.Escape(target), RegexOptions.Compiled);

            var randData = Enumerable.Range(0, nofInserts)
                .Select(i => insertString)
                .Aggregate((string a, string n)
                    => a + new string('Y', rand.Next(1, 100))
                       + n);

            //Assert.AreEqual(expectedRegexHits, r.Count(randData));
            _fileSystem.AddFile(fullFileName, new MockFileData(randData));
            Assert.AreEqual(
                Messages.Result.Successful(expectedHits, target, fileName),
                await _main.Run(new string[] { fullFileName }));
        }
    }
}
