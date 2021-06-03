using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RSPNVPK.VPK
{
    class DirFile
    {
        public Header Header { get; private set; }
        public BinaryReader Reader { get; private set; }

        public DirEntryBlock[] EntryBlocks { get; private set; }

        public int FileNum { get; private set; }

        public DirFile(FileStream fstream)
        {
            fstream.Seek(0, SeekOrigin.Begin);
            Reader = new BinaryReader(fstream);
            Header = new Header(Reader);

            // Read entry blocks
            EntryBlocks = DirEntryBlock.Parse(Reader).ToArray();

            ushort maxFileIdx = 0;
            foreach(var e in EntryBlocks)
            {
                if (e.FileIdx > maxFileIdx) maxFileIdx = e.FileIdx;
            }
            FileNum = maxFileIdx + 1;
        }
    }
}
