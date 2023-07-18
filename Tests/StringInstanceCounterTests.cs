using FileNameCounter.Implementations;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TestProject.Helpers;

namespace TestProject
{
    public class StringInstanceCounterTests
    {

        [Test]
        public void Should_throw_on_empty()
           => Assert.Throws<ArgumentException>(() => { new StringInstanceCounter(string.Empty, 1024); });

        [TestCase (32)]
        [TestCase (64)]
        [TestCase (128)]
        [TestCase(256)] //There is actually nothing full for BigInteger
        public async Task Should_work_on_just_full_bit_field(int bitFieldLength)
        {
            var target = "a" + new string('b', bitFieldLength - 2) + "a";
            var counter = new StringInstanceCounter(target, 1024);
            Assert.AreEqual(1, await counter.CountAsync(new StringReader(target)));
            Assert.AreEqual(2, await counter.CountAsync(new StringReader(target + target)));
        }

        [TestCase(3)]
        [TestCase(65)]
        [TestCase(129)]
        [TestCase(257)] //Hard to tell what expanded would mean
        public async Task Should_work_on_just_expanded_bit_field(int bitFieldLength)
        {
            var target = "a" + new string('b', bitFieldLength - 2) + "a";
            var counter = new StringInstanceCounter(target, 1024);
            Assert.AreEqual(1, await counter.CountAsync(new StringReader(target)));
            Assert.AreEqual(2, await counter.CountAsync(new StringReader(target + target)));
        }

        [Test]
        public async Task Should_handle_interleaved_matches()
        {
            var counter = new StringInstanceCounter("aba", 1024);
            Assert.AreEqual(2, await counter.CountAsync(new StringReader("ababa")));
        }

        [Test]
        public async Task Should_handle_zero_length_stream()
        {
            var counter = new StringInstanceCounter("aba", 1024);
            Assert.AreEqual(0, await counter.CountAsync(new StringReader(string.Empty)));
        }

        [Test]
        [Ignore("Only to be ran when evaluating algorithms")]
        public async Task Load_test()
        {
            var n = (int)1E4;
            var target = "ababab";
            var sLength = 1024;
            var s = target + Enumerable.Range(0, 512 - target.Length / 2).Select(i => "ab").Aggregate((a, b) => a + b);

            using var reader = new TextReaderMocker(s, n); //gigabyte order
            var counter = new StringInstanceCounter(target, 1024);
            var swThis = Stopwatch.StartNew();
            var thisResult = await counter.CountAsync(reader);
            swThis.Stop();

            var random = new Random();
            var regexResult = 0;
            var _a = new Regex(target);
            var swRegex = Stopwatch.StartNew();
            for (int i = 0; i < n; i++)
            {
                //swRegex.Stop();
                //in case regex cashes results.
                var regexS = target + new string((char)(i), sLength - target.Length);
                swRegex.Start();
                regexResult += _a.Count(regexS);
            }
            swRegex.Stop();


            //Assert.AreEqual(n, regexResult);
            //Assert.AreEqual(n, thisResult);

            Assert.Fail($"{nameof(swThis)} :{swThis.ElapsedMilliseconds}{Environment.NewLine}{nameof(swRegex)}:{swRegex.ElapsedMilliseconds}");
           
        }
    }
}