namespace FileNameCounter.Interfaces
{
    /// <summary>
    /// Counts number of matches of a string in a TextReader.
    /// </summary>
    public interface IStringInstanceCounter
    {
        /// <summary>
        /// Counts the number of instances of the target string in 
        /// the TextReader stream, which will be read to the end. Matches
        /// are counted interleaved, e.g. with target string 'aba' the TextReader
        /// stream 'ababa' will give two matches.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Number of matches.</returns>
        Task<long> CountAsync(TextReader reader);
    }
}