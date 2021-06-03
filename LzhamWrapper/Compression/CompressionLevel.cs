namespace LzhamWrapper.Compression
{
    public enum CompressionLevel : uint
    {
        Fastest,
        Faster,
        Default,
        Better,
        Uber,
        TotalLevels,
        Force = 0xFFFFFFFF
    }
}