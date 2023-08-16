using FileNameCounter.Interfaces;

namespace FileNameCounter.Implementations
{
    /// <inheritdoc/>
    public class StringInstanceCounterFactory
        : IStringInstanceCounterFactory
    {
        private readonly int _bufferSize;
        public StringInstanceCounterFactory(int bufferSize)
        {
            _bufferSize = bufferSize;
        }
        public IStringInstanceCounter Create(string target)
            => new StringInstanceCounter(target, _bufferSize);
    }

    /// <inheritdoc/>
    public class StringInstanceCounterSpanBasedFactory
        : IStringInstanceCounterFactory
    {
        private readonly int _bufferSize;

        public StringInstanceCounterSpanBasedFactory(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public IStringInstanceCounter Create(string target)
            => new StringInstanceCounterSpanBased(target, _bufferSize);
    }

    /// <inheritdoc/>
    public class StringInstanceCounterSpanBasedSimplifiedFactory
        : IStringInstanceCounterFactory
    {
        private readonly int _bufferSize;

        public StringInstanceCounterSpanBasedSimplifiedFactory(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public IStringInstanceCounter Create(string target)
            => new StringInstanceCounterSpanBasedSimplified(target, _bufferSize);
    }
}
