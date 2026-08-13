using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RSPNVPK.VPK
{
    class DirEntryBlock
    {
        public uint CRC { get; private set; } // File's CRC
        public ushort NumBytes { get; private set; }
        public ushort FileIdx { get; private set; }

        // entries...
        public DirEntry[] Entries { get; private set; }

        // Preload data, stored inside the directory file itself
        public byte[] PreloadData { get; private set; }

        // ---
        
        public long StartPos { get; private set; }
        public string Path { get; private set; }

        static public readonly ushort TERMINTAOR = ushort.MaxValue;

        public DirEntryBlock(BinaryReader reader, string path)
        {
            StartPos = reader.BaseStream.Position;
            Path = path;

            CRC = reader.ReadUInt32();
            NumBytes = reader.ReadUInt16();
            FileIdx = reader.ReadUInt16();

            // read entries
            Entries = DirEntry.Parse(reader).ToArray();

            // read preload data that lives in the directory file
            PreloadData = new byte[NumBytes];
            for (var i = 0; i < NumBytes; i++)
                PreloadData[i] = reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(CRC);
            writer.Write(NumBytes);
            writer.Write(FileIdx);

            for(var i=0; i<Entries.Length; i++)
            {
                Entries[i].Write(writer);

                if (i != (Entries.Length - 1))
                {
                    writer.Write((ushort)0);
                } else
                {
                    writer.Write(TERMINTAOR);
                }
            }

            if (PreloadData != null && PreloadData.Length > 0)
                writer.Write(PreloadData);
        }

        // Decompressed constructor
        public DirEntryBlock(byte[] data, ulong offset, ushort fileIdx, uint flags, ushort flags2, string path)
        {
            Path = path;

            var crc = new Crc32();
            CRC = crc.Get(data);
            NumBytes = 0; // ?
            FileIdx = fileIdx;

            var ent_num = (data.Length + 0x100000 - 1) / 0x100000;
            var entries = new DirEntry[ent_num];

            var k = offset;
            var szt = (ulong)data.Length;
            for(var i=0; i < ent_num; i++)
            {
                var sz = Math.Min(0x100000u, szt);
                szt -= sz;
                entries[i] = new DirEntry(flags, flags2, k, sz);
                k += sz;
            }

            Entries = entries;
        }

        // Constructor with precomputed entries (used by Packer when writing compressed chunks)
        public DirEntryBlock(uint crc, ushort fileIdx, DirEntry[] entries, string path)
        {
            CRC = crc;
            NumBytes = 0;
            FileIdx = fileIdx;
            Entries = entries;
            Path = path;
            PreloadData = new byte[0];
        }

        public static IEnumerable<DirEntryBlock> Parse(BinaryReader reader)
        {
            string extension, path, name;
            while (!string.IsNullOrEmpty(extension = reader.ReadNTString()))
            {
                while (!string.IsNullOrEmpty(path = reader.ReadNTString()))
                {
                    while (!string.IsNullOrEmpty(name = reader.ReadNTString()))
                    {
                        // the game stores an empty path as a single space
                        var fullPath = path == " " ? $"{name}.{extension}" : $"{path}/{name}.{extension}"; // TODO: rewrite
                        yield return new DirEntryBlock(reader, fullPath);
                    }
                }
            }
        }
    }
}
