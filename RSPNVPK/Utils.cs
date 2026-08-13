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

            // The output buffer needs to be larger than the input for incompressible data,
            // so start with a comfortable size and grow if lzham reports a too small buffer.
            var compressBufSize = (int)(decompressedBytes.Length + (decompressedBytes.Length / 4) + 1024);
            if (compressBufSize < 4096) compressBufSize = 4096;
            var compressBuf = new byte[compressBufSize];

            uint adler32 = 0;

            while (true)
            {
                var inLen = decompressedLength;
                var outLen = compressBufSize;

                var result = LzhamWrapper.Lzham.CompressMemory(parameters, decompressedBytes, ref inLen, 0, compressBuf, ref outLen, 0, ref adler32);
                if (result == CompressStatus.OutputBufferTooSmall)
                {
                    compressBufSize *= 2;
                    compressBuf = new byte[compressBufSize];
                    continue;
                }
                if (result != CompressStatus.Success)
                {
                    throw new Exception("Lzham.CompressMemory failed. Status: " + result.ToString());
                }
                if (inLen != decompressedBytes.Length)
                {
                    throw new Exception($"Data length mismatch: {inLen} vs {decompressedBytes.Length}");
                }

                var ret = new byte[outLen];
                Array.Copy(compressBuf, ret, outLen);
                return ret;
            }
        }
    }
}
