namespace TestProject.Helpers
{
    /// <summary>
    /// Only the Read() method gives useful results.
    /// </summary>
    public class TextReaderMocker : TextReader
    {
        private readonly IEnumerator<char> _chars;
        private readonly string _s;
        private readonly int _n;
        private int blockCount;

        /// <summary>
        /// When calling the Read() method the consecutive results will
        /// correspond to repeating s n times.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        public TextReaderMocker(string s, int n)
        {
            _chars = GenerateChars(s, n).GetEnumerator();
            _s = s;
            _n = n;
        }

        public override Task<int> ReadBlockAsync(char[] buffer, int start, int size)
        {
            if (size != _s.Length)
                throw new ArgumentException($"Must be equal to {nameof(_s.Length)}", nameof(size));
            if (blockCount < _n)
            {
                Array.Copy(_s.ToCharArray(), buffer, size);
                blockCount++;
                return Task.FromResult(size);
            }
            return Task.FromResult(0);
        }


        private static IEnumerable<char> GenerateChars(string s, int n)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < s.Length; j++)
                {
                    yield return s[j];
                }
            }
        }

        public override int Read() => _chars.MoveNext() ? _chars.Current : -1;
    }
}
