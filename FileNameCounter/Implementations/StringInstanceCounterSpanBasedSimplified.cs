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
    public class StringInstanceCounterSpanBasedSimplified : IStringInstanceCounter
    {
        private readonly int _bufferSize;
        private readonly int _copyZoneLength;
        private readonly int _fromZoneStart;
        private readonly string _target;

        public StringInstanceCounterSpanBasedSimplified(string target, int bufferSize)
        {
            if (target.Length > bufferSize)
                throw new ArgumentException($"Must be greater than on equal to {target.Length}.", nameof(bufferSize));

            if (string.IsNullOrEmpty(target)) throw new ArgumentException("Searching for empty string not allowed.", nameof(target));
            _bufferSize = bufferSize;
            _copyZoneLength = target.Length - 1;
            _fromZoneStart = bufferSize;
            _target = target;
        }
        
        public async Task<long> CountAsync(TextReader reader)
        {
            // If we search for 'madame' and have a bufferSize of 16 the buffer will look like this
            // The To-zone will contain the content of the previous read buffer's From-zone.
            //
            //   0                               1         
            //   0 1 2 3 4 5 6 7 8 9 A B C D E F 0 1 2 3 4 
            //  | | | | | | | | | | | | | | | | | | | | | |
            //  |         |         |           |         | 
            //  |         |<------ Read buffer ---------->| 
            //  |<--To--->|         |           |<-From ->|   


            var buffer = new char[_copyZoneLength + _bufferSize];
            long totalCount = 0;
            long i = 0;
            int readChars;
            while ((readChars = await reader.ReadBlockAsync(buffer, _copyZoneLength, _bufferSize)) != 0)
            {
                totalCount += Count(buffer, readChars, i);
                i++;
            }
            return totalCount;
        }

        private const int _toZoneStart = 0;

        
        private long Count(char[] buffer, int readChars, long i)
        {
            var currentBuffer =
                i == 0
                ? new ReadOnlySpan<char>(buffer, _copyZoneLength, readChars)
                : new ReadOnlySpan<char>(buffer, 0, _copyZoneLength + readChars);

            var count = interleavingCount(currentBuffer);
            Array.Copy(buffer, _fromZoneStart,
                               buffer, _toZoneStart,
                               _copyZoneLength);
            return count;

            // when we have a hit we restart the search on
            // the next character.
            int interleavingCount(ReadOnlySpan<char> buffer)
            {
                int hit;
                var count = 0;
                while ((hit = buffer.IndexOf(_target)) != -1)
                {
                    buffer = buffer.Slice(hit + 1);
                    count++;
                }
                return count;
            }
        }
    }
}
