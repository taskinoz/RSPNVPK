using System;
using System.Collections.Generic;

namespace RSPNVPK.VPK
{
    internal class PackedFile
    {
        public DirEntryBlock Block { get; }
        public List<byte[]> Chunks { get; }

        public PackedFile(DirEntryBlock block, List<byte[]> chunks)
        {
            Block = block;
            Chunks = chunks;
        }
    }

    internal static class Packer
    {
        // Entries have a size limit of 1 MiB, so anything over is split into separate entries
        public const uint CHUNK_SIZE = 0x100000;

        // Only bother compressing chunks above this size (matches what the game does)
        public const int COMPRESS_THRESHOLD = 4096;

        // Default entry flags used by RSPNVPK
        public const uint DEFAULT_FLAGS = 0x101;
        public const ushort DEFAULT_FLAGS2 = 0;

        /// <summary>
        /// Splits a file into 1 MiB entries (optionally lzham compressed) and builds its DirEntryBlock.
        /// </summary>
        public static PackedFile Pack(byte[] data, ulong offset, ushort fileIdx, uint flags, ushort flags2, string path, bool compress)
        {
            var crc = new Crc32().Get(data);

            var entries = new List<DirEntry>();
            var chunks = new List<byte[]>();

            var pos = 0;
            var remaining = data.Length;
            var k = offset;

            while (remaining > 0)
            {
                var sz = (int)Math.Min(CHUNK_SIZE, (uint)remaining);
                var chunk = new byte[sz];
                Array.Copy(data, pos, chunk, 0, sz);

                byte[] stored = chunk;
                if (compress && sz >= COMPRESS_THRESHOLD)
                    stored = Utils.CompressMemory(chunk);

                chunks.Add(stored);
                entries.Add(new DirEntry(flags, flags2, k, (ulong)stored.Length, (ulong)chunk.Length));

                k += (ulong)stored.Length;
                pos += sz;
                remaining -= sz;
            }

            var block = new DirEntryBlock(crc, fileIdx, entries.ToArray(), path);
            return new PackedFile(block, chunks);
        }
    }
}
