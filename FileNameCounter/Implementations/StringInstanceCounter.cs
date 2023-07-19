#pragma warning disable IDE0054 // Use compound assignment - does not work for generics.
using System.Numerics;
using FileNameCounter.Interfaces;

namespace FileNameCounter.Implementations
{
    /// <inheritdoc/>
    public class StringInstanceCounter : IStringInstanceCounter
    {
        private readonly IGenericCounter _genericCounter;

        /// <summary>
        /// Creates an instance that holds internal structures to search for a
        /// specific string.
        /// </summary>
        /// <param name="target">The string to search for.</param>
        /// <param name="bufferSize">The size in characters to read from a TextReader.</param>
        /// <exception cref="ArgumentException"></exception>
        public StringInstanceCounter(string target, int bufferSize)
        {
            if (string.IsNullOrEmpty(target)) throw new ArgumentException("Must not be empty.", nameof(target));
            var l = target.Length;
            _genericCounter = l switch
            {
                _ when l > 128 => new GenericCounter<BigInteger>(target, bufferSize),
                _ when l > 64 => new GenericCounter<UInt128>(target, bufferSize), // Probably better than BigInteger
                _ when l > 32 => new GenericCounter<UInt64>(target, bufferSize), // One word in most modern systems
                _ => new GenericCounter<uint>(target, bufferSize), // In the off chance that someone is using a 32 bit word
            };
        }

        /// <inheritdoc/>
        public Task<long> CountAsync(TextReader reader) 
            => _genericCounter.CountAsync(reader);

        private interface IGenericCounter
        {
            Task<long> CountAsync(TextReader textReader);
        }

        /// <summary>
        /// This class represents a state machine which state
        /// is updated for each consumed character.
        /// </summary>
        /// <typeparam name="T">A bit field,size is dependent on the
        /// length of the target string.</typeparam>
        private class GenericCounter<T> : IGenericCounter
            where T : IShiftOperators<T, int, T>, IBinaryInteger<T>
        {
            /// <summary>
            /// Holds a mask for the positions of every character in the target
            /// string. For example, with 'madame' as the target string
            /// _masks['m'] -> 0000..0001 0001 - 'm' is in two positions
            /// _masks['a'] -> 0000..0000 1010 - 'a' is in two positions
            /// _masks['d'] -> 0000..0000 0100
            /// _masks['e'] -> 0000..0010 0000
            /// </summary>
            private readonly Dictionary<char, T> _masks;

            /// <summary>
            /// Holds a bit in the position of the last letter,
            /// again with target string 'madame' it would be
            /// 0000..0010 0000
            /// </summary>
            private readonly T hitMask;

            /// <summary>
            /// This is the inverse of the hit mask, it is needed for
            /// the BigInteger version where we need to stop the state
            /// from growing. For target string 'madame' it would be
            /// 1111..1101 1111
            /// </summary>
            private readonly T clearHitMask;

            private readonly T _1 = T.CreateChecked(1);
            private readonly T _0 = T.CreateChecked(0);
            private readonly int _bufferSize;

            public GenericCounter(string target, int bufferSize)
            {
                hitMask = _1 << target.Length - 1;
                clearHitMask = ~hitMask;
                _masks = new Dictionary<char, T>();
                for (int i = 0; i < target.Length; i++)
                {
                    var c = target[i];
                    if (!_masks.TryGetValue(c, out var oldMask))
                        oldMask = _0;
                    _masks[c] = oldMask | _1 << i;
                }

                _bufferSize = bufferSize;
            }

            public Task<long> CountAsync(TextReader reader)
            {
                var hitCount = 0L;

                // State holds the pattern matching from previous iterations
                // If target string is 'madame' the state will develop this way.
                // State shown just before using the clearHitMask.
                // 
                // X - 0000 0000 - No match
                // m - 0000 0001 - matching first
                // a - 0000 0010 - matching up to second
                // d - 0000 0100 - matching up to third
                // a - 0000 1000 - matching up to fourth
                // m - 0001 0001 - matching up to fifth but also possible to be the first m.
                // e - 0010 0000 - matching up to sixth, alternative match terminated.
                //                    As we have a hit, this is recored by hit counter.
                // X - 0000 0000 - No match

                T state = _0;

                int nofCharsRead;
                var buffer = new char[_bufferSize];
                while ((nofCharsRead = reader.ReadBlockAsync(buffer, 0, _bufferSize).Result) != 0)
                {
                    for (int i = 0; i < nofCharsRead; i++)
                    {
                        var c = buffer[i];
                        if (!_masks.TryGetValue(c, out var mask))
                            mask = _0;

                        state = (state << 1) + _1;
                        state &= mask;
                        if ((state & hitMask) != _0)
                        {
                            hitCount++;
                            state = state & clearHitMask;
                        }
                    }
                }
                return Task.FromResult(hitCount);
            }
        }
    }
}
