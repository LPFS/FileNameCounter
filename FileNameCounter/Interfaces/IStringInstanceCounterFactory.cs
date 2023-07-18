namespace FileNameCounter.Interfaces
{
    /// <summary>
    /// The rationale for this factory is currently 
    /// only to facilitate unit testing.
    /// </summary>
    public interface IStringInstanceCounterFactory
    {
        public IStringInstanceCounter Create(string target);
    }
}
