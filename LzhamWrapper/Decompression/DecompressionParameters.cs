using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LzhamWrapper.Decompression
{
    /// <summary>
    /// Must call <see cref="Initialize"/> before passing to unmanaged methods.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct DecompressionParameters
    {
        private uint m_struct_size;
        private uint m_dict_size_log2;
        private DecompressionFlags m_decompress_flags;
        private uint m_num_seed_bytes;
        private byte* m_pSeed_bytes;

        public uint DictionarySize
        {
            get => m_dict_size_log2;
            set => m_dict_size_log2 = value;
        }

        public DecompressionFlags Flags
        {
            get => m_decompress_flags;
            set => m_decompress_flags = value;
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
            m_struct_size = (uint)sizeof(DecompressionParameters);
        }
    }
}