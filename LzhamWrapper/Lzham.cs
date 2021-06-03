using System;
using System.Runtime.InteropServices;
using LzhamWrapper.Compression;
using LzhamWrapper.Decompression;

namespace LzhamWrapper
{
    public class Lzham
    {
        private const string LzhamDll = "lzham_x64.dll";

        public static uint GetVersion()
        {
            return lzham_get_version();
        }

        #region Compression

        public static unsafe CompressionHandle CompressInit(CompressionParameters parameters)
        {
            parameters.Initialize();
            byte* pBytes = (byte*)&parameters;
            return lzham_compress_init(pBytes);
        }

        public static CompressionHandle CompressReinit(CompressionHandle state)
        {
            return lzham_compress_reinit(state);
        }

        public static unsafe CompressStatus Compress(CompressionHandle state, byte[] inBuf, ref int inBufSize, int inBufOffset, byte[] outBuf, ref int outBufSize, int outBufOffset, bool noMoreInputBytesFlag)
        {
            if (inBufOffset + inBufSize > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                IntPtr inSize = new IntPtr(inBufSize);
                IntPtr outSize = new IntPtr(outBufSize);
                CompressStatus result = (CompressStatus)lzham_compress(state, inBytes + inBufOffset, ref inSize, outBytes + outBufOffset, ref outSize, noMoreInputBytesFlag ? 1 : 0);
                inBufSize = inSize.ToInt32();
                outBufSize = outSize.ToInt32();
                return result;
            }
        }

        public static unsafe CompressStatus Compress2(CompressionHandle state, byte[] inBuf, ref int inBufSize, int inBufOffset, byte[] outBuf, ref int outBufSize, int outBufOffset, Flush flushType)
        {
            if (inBufOffset + inBufSize > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                IntPtr inSize = new IntPtr(inBufSize);
                IntPtr outSize = new IntPtr(outBufSize);
                CompressStatus result = (CompressStatus)lzham_compress2(state, inBytes + inBufOffset, ref inSize, outBytes + outBufOffset, ref outSize, flushType);
                inBufSize = inSize.ToInt32();
                outBufSize = outSize.ToInt32();
                return result;
            }
        }

        public static unsafe CompressStatus CompressMemory(CompressionParameters parameters, byte[] inBuf, ref int inBufSize, int inBufOffset, byte[] outBuf, ref int outBufSize, int outBufOffset, ref uint adler32)
        {
            if (inBufOffset + inBufSize > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            parameters.Initialize();
            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                byte* pBytes = (byte*)&parameters;
                IntPtr outSize = new IntPtr(outBufSize);
                CompressStatus result = (CompressStatus)lzham_compress_memory(pBytes, outBytes + outBufOffset, ref outSize, inBytes + inBufOffset, inBufSize, ref adler32);
                outBufSize = outSize.ToInt32();
                return result;
            }
        }

        public static uint CompressDeinit(IntPtr state)
        {
            return (uint)lzham_compress_deinit(state);
        }

        #endregion Compression

        #region Decompression

        public static unsafe DecompressionHandle DecompressInit(DecompressionParameters parameters)
        {
            parameters.Initialize();
            byte* pBytes = (byte*)&parameters;
            return lzham_decompress_init(pBytes);
        }

        public static unsafe DecompressionHandle DecompressReinit(DecompressionHandle state, DecompressionParameters parameters)
        {
            parameters.Initialize();
            byte* pBytes = (byte*)&parameters;
            return lzham_decompress_reinit(state, pBytes);
        }

        public static unsafe DecompressStatus Decompress(DecompressionHandle state, byte[] inBuf, ref int inBufSize, int inBufOffset, byte[] outBuf, ref int outBufSize, int outBufOffset, bool noMoreInputBytesFlag)
        {
            if (inBufOffset + inBufSize > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                IntPtr inSize = new IntPtr(inBufSize);
                IntPtr outSize = new IntPtr(outBufSize);
                DecompressStatus result = (DecompressStatus)lzham_decompress(state, inBytes + inBufOffset, ref inSize, outBytes + outBufOffset, ref outSize, noMoreInputBytesFlag ? 1 : 0);
                inBufSize = inSize.ToInt32();
                outBufSize = outSize.ToInt32();
                return result;
            }
        }

        public static unsafe DecompressStatus DecompressMemory(DecompressionParameters parameters, byte[] inBuf, ref UIntPtr inBufSize, int inBufOffset, byte[] outBuf, ref UIntPtr outBufSize, int outBufOffset, ref uint adler32)
        {
            if (inBufOffset + inBufSize.ToUInt32() > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize.ToUInt32() > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            parameters.Initialize();
            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                byte* pBytes = (byte*)&parameters;
                DecompressStatus result = (DecompressStatus)lzham_decompress_memory(pBytes, outBytes + outBufOffset, ref outBufSize, inBytes + inBufOffset, inBufSize, ref adler32);
                return result;
            }
        }

        public static uint DecompressDeinit(IntPtr state)
        {
            return (uint)lzham_decompress_deinit(state);
        }

        #endregion Decompression

        #region P/Invoke

#pragma warning disable IDE1006 // Naming Styles

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint lzham_get_version();

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe CompressionHandle lzham_compress_init(byte* parameters);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern CompressionHandle lzham_compress_reinit(CompressionHandle state);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int lzham_compress(CompressionHandle state, byte* inBuf, ref IntPtr inBufSize, byte* outBuf, ref IntPtr outBufSize, int noMoreInputBytesFlag);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int lzham_compress2(CompressionHandle state, byte* inBuf, ref IntPtr inBufSize, byte* outBuf, ref IntPtr outBufSize, Flush flushType);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int lzham_compress_memory(byte* parameters, byte* dstBuffer, ref IntPtr dstLength, byte* srcBuffer, int srcLength, ref uint adler32);

        /// <summary>
        /// Deinitializes a compressor, releasing all allocated memory.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Adler32 of source data (only valid on success).</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int lzham_compress_deinit(IntPtr state);

        /// <summary>
        /// Initializes a decompressor.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe DecompressionHandle lzham_decompress_init(byte* parameters);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe DecompressionHandle lzham_decompress_reinit(DecompressionHandle state, byte* parameters);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int lzham_decompress(DecompressionHandle state, byte* inBuf, ref IntPtr inBufSize, byte* outBuf, ref IntPtr outBufSize, int noMoreInputBytesFlag);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int lzham_decompress_memory(byte* parameters, byte* dstBuffer, ref UIntPtr dstLength, byte* srcBuffer, UIntPtr srcLength, ref uint adler32);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int lzham_decompress_deinit(IntPtr state);

#pragma warning restore IDE1006 // Naming Styles

        #endregion P/Invoke
    }
}