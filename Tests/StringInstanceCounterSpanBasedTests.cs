using FileNameCounter.Implementations;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TestProject.Helpers;

namespace TestProject
{
    public class StringInstanceCounterSpanBasedTests
    {

        [Test]
         public void Should_throw_on_empty_target()
           => Assert.Throws<ArgumentException>(() => { new StringInstanceCounterSpanBased(string.Empty, 16); });

        [Test]
        public async Task Should_handle_no_matches()
        {
            var counter = new StringInstanceCounterSpanBased("aba", 8);
            Assert.AreEqual(0, await counter.CountAsync(new StringReader("ZZZZZZZZZZ")));
        }

        [Test]
        public async Task Should_handle_empty_string()
        {
            var counter = new StringInstanceCounterSpanBased("aba", 8);
            Assert.AreEqual(0, await counter.CountAsync(new StringReader("")));
        }

        [Test]
        public async Task Should_handle_interleaved_matches()
        {
            var counter = new StringInstanceCounterSpanBased("aba", 8);
            Assert.AreEqual(2, await counter.CountAsync(new StringReader("ababa")));
        }

        [TestCase("aba3456701234567", "aba", 1)]
        [TestCase("_____aba________", "aba", 1)]
        [TestCase("______aba_______", "aba", 1)]
        [TestCase("_______aba______", "aba", 1)]
        [TestCase("________aba_____", "aba", 1)]

        public async Task Should_handle_border_matches(string s, string target, int expected)
        {
            var counter = new StringInstanceCounterSpanBased(target, 8);
            Assert.AreEqual(expected, await counter.CountAsync(new StringReader(s)));
        }

        [Test]
         public async Task Should_handle_zero_length_stream()
        {
            var counter = new StringInstanceCounterSpanBased("aba", 4);
            Assert.AreEqual(0, await counter.CountAsync(new StringReader(string.Empty)));
        }

        [Test]
        [Ignore("Only to be ran when evaluating algorithms")]
         public async Task Load_test()
        {
            var n = (int)1E4;
            var target = "ababab";
            var sLength = 1024;
            var s = target + Enumerable.Range(0, 512 - target.Length / 2).Select(i => "ab").Aggregate((a,b) => a+b);

            using var reader = new TextReaderMocker(s, n); //gigabyte order
            var counter = new StringInstanceCounterSpanBased(target, sLength);
            var swThis = Stopwatch.StartNew();
            var thisResult = await counter.CountAsync(reader);
            swThis.Stop();

            var random = new Random();
            var regexResult = 0;
            var _a = new Regex(target);
            using var readerDummy = new TextReaderMocker(s, n); //gigabyte order
            var swRegex = Stopwatch.StartNew();
            for (int i = 0; i < n; i++)
            {
                regexResult += _a.Count(s);
            }
            //while (readerDummy.Read() == -1) { }
            swRegex.Stop();


            //Assert.AreEqual(n, regexResult);
            //Assert.AreEqual(n, thisResult);

            Assert.Fail($"{nameof(swThis)} :{swThis.ElapsedMilliseconds}{Environment.NewLine}{nameof(swRegex)}:{swRegex.ElapsedMilliseconds}");
           
        }
    }
}