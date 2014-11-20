namespace SharpTools.Testing.EntityFramework.Internal
{
    /// <summary>
    /// The interface of a strategy used to generate identifiers for entities.
    /// </summary>
    internal interface IIdentifierGenerator
    {
        object Generate();
    }
}
