using System;
using System.IO;

namespace RSPNVPK
{
    class Program
    {
        static readonly string[] filesEdit =
        {
            //"scripts/kb_act.lst",

            //*
            "resource/ui/menus/advanced_hud.menu",
            "resource/ui/menus/audio.menu",
            "resource/ui/menus/controls.menu",
            //"resource/ui/menus/extras.menu", // not in the game
            "resource/ui/menus/panels/mainmenu.res",
            "resource/ui/menus/panels/spotlight.res",
            "scripts/kb_act.lst",
            // vscripts
            "scripts/vscripts/ui/menu_controls.nut",
            "scripts/vscripts/_items.nut",
            "scripts/vscripts/ui/menu_advanced_hud.nut",
            "scripts/vscripts/ui/menu_audio_settings.nut",
            //"scripts/vscripts/ui/menu_extra_settings.nut", // doesn't exist
            "scripts/vscripts/ui/menu_main.nut",
            "scripts/vscripts/ui/_menus.nut", //*/
        };

        static void Main(string[] args)
        {
            var fstream = new FileStream(@"D:\OriginGays\Titanfall2\vpk\englishclient_frontend.bsp.pak000_dir.vpk", FileMode.Open, FileAccess.ReadWrite);
            var k0k = new FileStream(@"D:\OriginGays\Titanfall2\vpk\client_frontend.bsp.pak000_228.vpk", FileMode.OpenOrCreate, FileAccess.Write);
            k0k.Position = 0;
            
            var writer = new BinaryWriter(fstream);
            var vpk = new VPK.DirFile(fstream);
            Console.WriteLine($"{vpk.Header.DirectorySize:X4} | {vpk.Header.EmbeddedChunkSize:X4}");
            foreach (var block in vpk.EntryBlocks)
            {
                Console.WriteLine($"{block.Path}: {block.CRC:X8} | {block.FileIdx} | {block.NumBytes:X4}");
                foreach (var e in block.Entries)
                {
                    Console.WriteLine($"\t@{e.StartPosition:X16} {e.Flags:X8} | {e.Flags2:X4} | {e.Compressed} | {e.CompressedSize:X16} | {e.DecompressedSize:X16}");
                }

                foreach(var edit in filesEdit)
                {
                    if(edit == block.Path)
                    {
                        if (block.Entries.Length > 1)
                            throw new Exception("Brih");

                        var crc = new Crc32();
                        var fb = File.ReadAllBytes(@"D:\Projects\Enhanced-Menu-Mod\src\" + edit);
                        if(fb.Length == 0)
                            throw new Exception("Brih");

                        // block's CRC
                        writer.BaseStream.Position = block.StartPos;
                        writer.Write((uint)crc.Get(fb));
                        //writer.BaseStream.Position += 4;
                        writer.BaseStream.Position += 2;
                        writer.Write((ushort)228);

                        var compressedSize = (ulong)fb.Length;
                        var decompressedSize = (ulong)fb.Length;

                        // Write offset alongside compressed and uncompressed sizes
                        writer.BaseStream.Position = block.Entries[0].StartPosition + 6; // skip 32 and 16 flags
                        writer.Write((ulong)k0k.Position);
                        writer.Write(compressedSize);
                        writer.Write(decompressedSize);
                        writer.Flush();

                        k0k.Write(fb);
                        k0k.Flush();
                    }
                }

                /*if(block.Path == "resource/ui/menus/main.menu")
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
                }*/
            }
        }
    }
}
