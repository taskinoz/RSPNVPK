using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RSPNVPK.VPK
{
    class Header
    {
        public uint Magic { get; private set; }
        public uint Version { get; private set; } // I can't be bothered to make it 2 words

        // Names custody of cra0 aka cra0kalo aka *REDACTED*(?) aka God-Knows-Who
        public uint DirectorySize { get; private set; }
        public uint EmbeddedChunkSize { get; private set; } // must be 0 for directory based VPKs
        public uint SelfHashesSize { get; private set; } // God knows what that is
        public uint SignatureSize { get; private set; } // God knows what that is

        // ---

        static public readonly uint MAGIC = 0x55AA1234;
        static public readonly uint VERSION = 0x30002; // 2.3 never changed, lmfao

        public Header(BinaryReader reader)
        {
            Magic = reader.ReadUInt32();
            if (Magic != MAGIC)
                throw new Exception("Magic is invalid!");

            Version = reader.ReadUInt32();
            if (Version != VERSION)
                throw new Exception("Version is unsupported!");

            DirectorySize = reader.ReadUInt32();
            EmbeddedChunkSize = reader.ReadUInt32();
            
            //SelfHashesSize = reader.ReadUInt32();
            //SignatureSize = reader.ReadUInt32();
        }
    }
}
