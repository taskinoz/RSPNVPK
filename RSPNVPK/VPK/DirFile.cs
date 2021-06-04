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

        public static void Write(BinaryWriter writer, DirEntryBlock[] blocks)
        {
            var map = new Dictionary<string, Dictionary<string, List<DirEntryBlock>>>();
            
            foreach(var b in blocks)
            {
                var ext = Path.GetExtension(b.Path).Remove(0, 1);
                var fname = Path.GetFileName(b.Path);

                var path = b.Path.Replace("/"+fname, "");

                //map[ext] = new Dictionary<string, List<DirEntryBlock>>();
                //[path].Add(b);
                if(!map.ContainsKey(ext))
                {
                    map.Add(ext, new Dictionary<string, List<DirEntryBlock>>());
                }
                if(!map[ext].ContainsKey(path))
                {
                    map[ext].Add(path, new List<DirEntryBlock>());
                }

                map[ext][path].Add(b);
            }

            writer.BaseStream.Position = 0;
            writer.Write(Header.MAGIC);
            writer.Write(Header.VERSION);
            writer.Write((ulong)0);
            writer.Flush();

            foreach (var i in map)
            {
                writer.WriteSourceString(i.Key);

                foreach (var j in i.Value)
                {
                    writer.WriteSourceString(j.Key);

                    foreach(var e in j.Value)
                    {
                        writer.WriteSourceString(Path.GetFileNameWithoutExtension(e.Path));
                        e.Write(writer);
                    }

                    writer.WriteSourceString("");
                }

                writer.WriteSourceString("");
            }

            writer.WriteSourceString("");

            writer.Flush();

            var treeSize = writer.BaseStream.Position - 16;
            writer.BaseStream.Position = 8;
            writer.Write((uint)treeSize);

            writer.Flush();
        }
    }
}
