using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LzhamWrapper.Compression
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct CompressionParameters
    {
        private uint m_struct_size;
        private uint m_dict_size_log2;
        private CompressionLevel m_level;
        private int m_max_helper_threads;
        private uint m_cpucache_total_lines;
        private uint m_cpucache_line_size;
        private CompressionFlags m_compress_flags;
        private uint m_num_seed_bytes;
        private byte* m_pSeed_bytes;

        public uint DictionarySize
        {
            get => m_dict_size_log2;
            set => m_dict_size_log2 = value;
        }

        public CompressionLevel CompressionLevel
        {
            get => m_level;
            set => m_level = value;
        }

        public int MaxHelperThreads
        {
            get => m_max_helper_threads;
            set => m_max_helper_threads = value;
        }

        public uint CPUCacheTotalLines
        {
            get => m_cpucache_total_lines;
            set => m_cpucache_total_lines = value;
        }

        public uint CPUCacheLineSize
        {
            get => m_cpucache_line_size;
            set => m_cpucache_line_size = value;
        }

        public CompressionFlags Flags
        {
            get => m_compress_flags;
            set => m_compress_flags = value;
        }

        [DisallowNull]
        public byte[]? SeedBytes
        {
            get
            {
                if (m_num_seed_bytes == 0) return null;

                byte[] buffer = new byte[m_num_seed_bytes];
                Marshal.Copy((IntPtr)m_pSeed_bytes, buffer, 0, buffer.Length);

                return buffer;
            }
            set
            {
                fixed (byte* pArr = &value[0])
                {
                    m_pSeed_bytes = pArr;
                    m_num_seed_bytes = (uint)value.Length;
                }
            }
        }

        public void Initialize()
        {
            m_struct_size = (uint)sizeof(CompressionParameters);
        }
    }
}