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
            if (NumBytes != 0)
                throw new Exception($"NumBytes != 0");
            FileIdx = reader.ReadUInt16();

            // read entries
            Entries = DirEntry.Parse(reader).ToArray();
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
                        var fullPath = path + '/' + name + '.' + extension; // TODO: rewrite
                        yield return new DirEntryBlock(reader, fullPath);
                    }
                }
            }
        }
    }
}
