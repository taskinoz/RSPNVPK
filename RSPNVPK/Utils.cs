using System;
using LzhamWrapper.Decompression;
using LzhamWrapper.Compression;

namespace RSPNVPK
{
    public static class Utils
    {
        public const uint DICT_SIZE = 20;

        public static byte[] DecompressMemory(byte[] compressedBytes, ulong uncompressedSize)
        {
            var compressedLength = new UIntPtr((uint)compressedBytes.Length);

            var decompressedBytes = new byte[uncompressedSize];
            var decompressedLength = new UIntPtr(uncompressedSize);

            uint adler32 = 0;

            var parameters = new DecompressionParameters { DictionarySize = DICT_SIZE, Flags = DecompressionFlags.OutputUnbuffered };
            parameters.Initialize();

            var result = LzhamWrapper.Lzham.DecompressMemory(parameters, compressedBytes, ref compressedLength, 0, decompressedBytes, ref decompressedLength, 0, ref adler32);
            if (result != DecompressStatus.Success)
            {
                throw new Exception("Lzham.DecompressMemory failed. Status: " + result.ToString());
            }
            if (decompressedLength.ToUInt64() != uncompressedSize)
            {
                throw new Exception($"Data length mismatch, poor modding tool: {decompressedLength} vs {uncompressedSize}");
            }

            return decompressedBytes;
        }

        public static byte[] CompressMemory(byte[] decompressedBytes)
        {
            var decompressedLength = (int)decompressedBytes.Length;

            var parameters = new CompressionParameters { DictionarySize = DICT_SIZE };
            parameters.Initialize();

            var compressBuf = new byte[decompressedBytes.Length];
            var compressBufSize = (int)decompressedBytes.Length;

            uint adler32 = 0;

            var result = LzhamWrapper.Lzham.CompressMemory(parameters, decompressedBytes, ref decompressedLength, 0, compressBuf, ref compressBufSize, 0, ref adler32);
            if (result != CompressStatus.Success)
            {
                throw new Exception("Lzham.CompressMemory failed. Status: " + result.ToString());
            }
            if (decompressedLength != decompressedBytes.Length)
            {
                throw new Exception($"Data length mismatch: {decompressedLength} vs {decompressedBytes.Length}");
            }

            // WTF?!
            var ret = new byte[compressBufSize];
            Array.Copy(compressBuf, ret, compressBufSize);
            return ret;
        }
    }
}
