using FileNameCounter.Interfaces;

namespace FileNameCounter.Implementations
{
    // Searches for interleaving matches. It works by reading N
    // fixed size strings and counting number of hits. This
    // would have been really easy except for the problem that we might
    // have matches spanning two strings.To accommodate for this we will
    // create N-1 intermediate strings (actually spans) from the end of each string
    // concatenated with the next. Every such intermediate string will be
    // formed of targetLength - 1 characters from each original string (except for a last shorter string). 
    // Please note that there can be no doubling of matches from this, as that would
    // mean that a string of targetLength - 1 would contain targetLength characters.
    // 
    // Searching for madame in two strings:
    // XXXmadameXXma
    // damemadame
    // Intermediate string:
    // eXXma + damem
    //
    // Each original string has one hit and the intermediate will give one more.

    ///<inheritdoc/>>
    public class StringInstanceCounterSpanBased : IStringInstanceCounter
    {
        private readonly int _bufferSize;
        private readonly int _copyZoneLength;
        private readonly int _higherBufferStart;
        private readonly string _target;

        public StringInstanceCounterSpanBased(string target, int bufferSize)
        {
            if (target.Length > bufferSize)
                throw new ArgumentException($"Must be greater than on equal to {target.Length}.", nameof(bufferSize));

            if (string.IsNullOrEmpty(target)) throw new ArgumentException("Searching for empty string not allowed.", nameof(target));
            _bufferSize = bufferSize;
            _copyZoneLength = target.Length - 1;
            _higherBufferStart = _copyZoneLength + _bufferSize;
            _target = target;
        }
        
        public async Task<long> CountAsync(TextReader reader)
        {
            // Two buffers plus room to copy the border part for the second buffer.
            // This means that for an even number of reads the characters are already positioned
            // for the intermediate count. For the odd ones however we need to do a copy as indicated
            // in the diagram below.
            // If we search for 'madame' and have a bufferSize of 16 the buffer will look like this
            //
            //   0                               1                               2
            //   0 1 2 3 4 5 6 7 8 9 A B C D E F 0 1 2 3 4 5 6 7 8 9 A B C D E F 0 1 2 3 4 
            //  | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | | |
            //            |         |           |         |         |           |         | 
            //            |<---- First read buffer(A)---->|<---- Second read buffer(B) -->| 
            //   <--To--->|         |           |         |         |           |<-From ->|   
            //   <--Border B+A ---->|           |<--Border A+B ---->|           |         |

            var buffer = new char[_copyZoneLength + 2 * _bufferSize];
            long totalCount = 0;
            long i = 0;
            int readChars;
            while ((readChars = await reader.ReadBlockAsync(buffer, CurrentBufferStart(i), _bufferSize)) != 0)
            {
                totalCount += Count(buffer, readChars, i);
                i++;
            }
            return totalCount;
        }

        private int CurrentBufferStart(long i)
            => (int)(_copyZoneLength + (i % 2) * _bufferSize);

        private static bool HigherBufferUsed(long i)
            => i % 2 == 1;

        private const int _copyZoneStart = 0;

        
        private long Count(char[] buffer, int readChars, long i)
        {
            var currentBuffer = new ReadOnlySpan<char>(buffer, CurrentBufferStart(i), readChars);
            var count = 0;
            interleavingCount(currentBuffer);
            if (i > 0)
            {
                if (HigherBufferUsed(i))
                {
                    Array.Copy(buffer, _higherBufferStart,
                               buffer, _copyZoneStart,
                               _copyZoneLength);
                }

                var border = new ReadOnlySpan<char>(
                    buffer,
                    CurrentBufferStart(i) - _copyZoneLength,
                    _copyZoneLength + Math.Min(_copyZoneLength, readChars));

                interleavingCount(border);
            }
            return count;

            // when we have a hit we restart the search on
            // the next character.
            void interleavingCount(ReadOnlySpan<char> buffer)
            {
                int hit;
                while ((hit = buffer.IndexOf(_target)) != -1)
                {
                    buffer = buffer.Slice(hit + 1);
                    count++;
                }
            }
        }
    }
}
