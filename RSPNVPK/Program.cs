using System;
using System.IO;

namespace RSPNVPK
{
    class Program
    {
        static void Main(string[] args)
        {
            var fstream = new FileStream(@"D:\OriginGays\Titanfall2\vpk\englishclient_frontend.bsp.pak000_dir.vpk", FileMode.Open, FileAccess.Read);
            var vpk = new VPK.DirFile(fstream);
            Console.WriteLine($"{vpk.Header.DirectorySize:X4} | {vpk.Header.EmbeddedChunkSize:X4}");
            foreach (var block in vpk.EntryBlocks)
            {
                Console.WriteLine($"{block.Path}: {block.CRC:X8} | {block.FileIdx} | {block.NumBytes:X4}");
                foreach (var e in block.Entries)
                {
                    Console.WriteLine($"\t{e.Flags:X8} | {e.Flags2:X4} | {e.Compressed} | {e.CompressedSize:X16} | {e.DecompressedSize:X16}");
                }

                if(block.Path == "resource/ui/menus/main.menu")
                {
                    // decompress that bad boy!
                    var stream = new FileStream(@"D:\OriginGays\Titanfall2\vpk\client_frontend.bsp.pak000_006.vpk", FileMode.Open, FileAccess.Read);
                    stream.Position = (long)block.Entries[0].Offset;
                    var buf = new byte[block.Entries[0].CompressedSize];
                    stream.Read(buf);
                    var ret = Utils.DecompressMemory(buf, block.Entries[0].DecompressedSize);
                    File.WriteAllBytes(@"D:\OriginGays\Titanfall2\vpk\AAAAAA.main_menu.txt", ret);

                    var bruh = File.ReadAllBytes(@"D:\OriginGays\Titanfall2\vpk\AAAAAA.main_menu_mod.txt");
                    var k0k = Utils.CompressMemory(bruh);
                    File.WriteAllBytes(@"D:\OriginGays\Titanfall2\vpk\AAAAAA.main_menu_mod.dat", k0k);
                }
            }
        }
    }
}
