using System;
using System.Linq;
using System.IO;

namespace RSPNVPK
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Invalid usage...");
                return;
            }

            var vpkdir = args[0];
            if(!vpkdir.EndsWith("_dir.vpk"))
            {
                Console.WriteLine($"Invalid directory file {vpkdir}");
                return;
            }

            var vpkarch = vpkdir.Replace("_dir.vpk", "_228.vpk").Replace("english", "");
            var directory = vpkdir.Replace(".vpk", "")+"\\";

            Console.WriteLine($"VPK directory: {vpkdir}\n" +
                $"VPK archive: {vpkarch}\n" +
                $"Directory: {directory}");

            var filesEdit = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Select(path => path.Replace(directory, "").Replace('\\', '/'));
            foreach(var edit in filesEdit)
            {
                Console.WriteLine($"\t{edit}");
            }

            Console.WriteLine(@"
 _____ _   _ ___ ____    _____ ___   ___  _
|_   _| | | |_ _/ ___|  |_   _/ _ \ / _ \| |
  | | | |_| || |\___ \    | || | | | | | | |
  | | |  _  || | ___) |   | || |_| | |_| | |___
  |_| |_| |_|___|____/    |_| \___/ \___/|_____|

 ____   ___  _____ ____  _   _ _ _____   __  __    _    _  _______
|  _ \ / _ \| ____/ ___|| \ | ( )_   _| |  \/  |  / \  | |/ / ____|
| | | | | | |  _| \___ \|  \| |/  | |   | |\/| | / _ \ | ' /|  _|
| |_| | |_| | |___ ___) | |\  |   | |   | |  | |/ ___ \| . \| |___
|____/ \___/|_____|____/|_| \_|   |_|   |_|  |_/_/   \_\_|\_\_____|

 ____    _    ____ _  ___   _ ____  ____  _ _ _
| __ )  / \  / ___| |/ / | | |  _ \/ ___|| | | |
|  _ \ / _ \| |   | ' /| | | | |_) \___ \| | | |
| |_) / ___ \ |___| . \| |_| |  __/ ___) |_|_|_|
|____/_/   \_\____|_|\_\\___/|_|   |____/(_|_|_)
");
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();

            var fstream = new FileStream(vpkdir, FileMode.Open, FileAccess.ReadWrite);
            var k0k = new FileStream(vpkarch, FileMode.OpenOrCreate, FileAccess.Write);
            k0k.Position = 0;
            
            var writer = new BinaryWriter(fstream);
            var vpk = new VPK.DirFile(fstream);
            Console.WriteLine($"{vpk.Header.DirectorySize:X4} | {vpk.Header.EmbeddedChunkSize:X4}");
            foreach (var block in vpk.EntryBlocks)
            {
                foreach (var edit in filesEdit)
                {
                    if (edit == block.Path)
                    {
                        Console.WriteLine($"Replacing {edit}...");

                        if (block.Entries.Length > 1)
                            throw new Exception("!!! NOT SUPPORTED !!!");

                        var crc = new Crc32();
                        var fb = File.ReadAllBytes(directory + edit);
                        if (fb.Length == 0)
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
            }

            Console.WriteLine("Done!\nPress Enter to exit!");
            Console.ReadLine();
        }
    }
}
