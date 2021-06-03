using System;

namespace LzhamWrapper.Compression
{
    [Flags]
    public enum CompressionFlags : uint
    {
        ForcePolarCoding = 1,

        /// <summary>
        /// Improves ratio by allowing the compressor's parse graph to grow "higher" (update to 4 parent nodes per output node) at the cost of speed.
        /// </summary>
        ExtremeParsing = 2,

        /// <summary>
        /// Guarantees that the comprssed output will always be the same given the same input and output parameters (no variation between runs due to kernel threading scheduling).
        /// <br/>
        /// If enabled, the compressor is free to use any optimizations which could lower the decompression rate (such as adaptively resetting the Huffman table update rate to
        /// maximum frequency, which is costly for the decompressor).
        /// </summary>
        DeterministicParsing = 4,
        TradeIffDecompressionForCompressionRatio = 16,
        WriteZlibStream = 32
    }
}