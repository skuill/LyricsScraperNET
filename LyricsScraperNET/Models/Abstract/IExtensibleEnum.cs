namespace LyricsScraperNET.Models.Abstract
{
    /// <summary>
    /// A string value with well-known instances (an "extensible enum"): new values can be added without breaking
    /// a consumer's <c>switch</c>, and the value is written to JSON and logs as text.
    /// </summary>
    internal interface IExtensibleEnum<TSelf>
        where TSelf : struct, IExtensibleEnum<TSelf>
    {
        string Value { get; }

        static abstract TSelf Create(string value);
    }
}
