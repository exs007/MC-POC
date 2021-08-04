namespace MS.DataAccess.Setup
{
    /// <summary>
    ///     Base environment resolver class with fallback logic for detecting the current environment
    /// </summary>
    public abstract class BaseEnvironmentResolver : IEnvironmentResolver
    {
        public string Environment { get; protected set; }
    }
}