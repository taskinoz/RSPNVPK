using System;

namespace LzhamWrapper.Decompression
{
    [Flags]
    public enum DecompressionFlags : uint
    {
        OutputUnbuffered = 1 << 0,
        ComputeAdler32 = 1 << 1,
        ReadZlibStream = 1 << 2
    }
}