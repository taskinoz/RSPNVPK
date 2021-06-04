using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RSPNVPK.VPK
{
    class DirEntry
    {
        public uint Flags { get; private set; }
        public ushort Flags2 { get; private set; } // word, huh

        public ulong Offset { get; private set; }
        public ulong CompressedSize { get; private set; }
        public ulong DecompressedSize { get; private set; }

        // ---

        public long StartPosition { get; private set; } // Seek

        public bool Compressed => CompressedSize != DecompressedSize;

        public DirEntry(BinaryReader reader)
        {
            StartPosition = reader.BaseStream.Position;

            Flags = reader.ReadUInt32();
            Flags2 = reader.ReadUInt16();

            Offset = reader.ReadUInt64();
            CompressedSize = reader.ReadUInt64();
            DecompressedSize = reader.ReadUInt64();

            // Game can't go beyond that normally?
            if (DecompressedSize > 0x100000)
                throw new Exception("Wtf are you faggin' bro?"); // ASSERT
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Flags);
            writer.Write(Flags2);

            writer.Write(Offset);
            writer.Write(CompressedSize);
            writer.Write(DecompressedSize);
        }

        public DirEntry(uint flags, ushort flags2, ulong offset, ulong decompressed)
        {
            StartPosition = 0; // ?

            Flags = flags;
            Flags2 = flags2;

            Offset = offset;
            CompressedSize = decompressed;
            DecompressedSize = decompressed;

            // Game can't go beyond that normally?
            if (DecompressedSize > 0x100000)
                throw new Exception("Wtf are you faggin' bro?"); // ASSERT
        }

        static public IEnumerable<DirEntry> Parse(BinaryReader reader)
        {
            ushort bs;
            do
            {
                yield return new DirEntry(reader);
                bs = reader.ReadUInt16(); // TODO: figure out what this is exactly...
                if (bs != 0 && bs != DirEntryBlock.TERMINTAOR)
                {
                    //throw new Exception("Bruh assert");
                    //Console.WriteLine($"A?: {bs:X4}");
                }
            }
            while (bs != DirEntryBlock.TERMINTAOR);
        }
    }
}
