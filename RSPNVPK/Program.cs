using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace RSPNVPK
{
    class Program
    {
        /*
        static void Main2(string[] args)
        {
            var fstream = new FileStream(@"D:\OriginGays\Titanfall2\vpk\englishclient_mp_common.bsp.pak000_dir.vpk", FileMode.Open, FileAccess.Read);
            var vpk = new VPK.DirFile(fstream);
            Console.WriteLine($"{vpk.Header.DirectorySize:X4} | {vpk.Header.EmbeddedChunkSize:X4}");

            foreach(var block in vpk.EntryBlocks)
            {
                Console.WriteLine($"{block.Path}: {block.CRC:X8} | {block.FileIdx} | {block.NumBytes:X4}");
                foreach (var e in block.Entries)
                {
                    Console.WriteLine($"\t@{e.StartPosition:X16} {e.Flags:X8} | {e.Flags2:X4} | {e.Compressed} | {e.CompressedSize:X16} | {e.DecompressedSize:X16}");
                }
            }
        }
        // */

        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Invalid usage, use -h for help");
                return;
            }

            var silent = false;
            var vpkname = "228";
            var vpkdir = "";

            if (args[0]!="-h")
            {
                vpkdir = args[0];
            }
            else
            {
                Console.WriteLine("RSPNVPK <VPKNAME>\n"+
                                  "   -h - Help\n"+
                                  "   -s - Run without warning about backups\n"+
                                  "   -n - The number of the client vpk\n"+
                                  "   -d - Choose the directory with the files you're packing"
                );
                return;
            }

            if(!vpkdir.EndsWith("_dir.vpk"))
            {
                Console.WriteLine($"Invalid directory file {vpkdir}");
                return;
            }

            var vpkarch = "";
            var directory = "";

            if(args.Length > 1)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-s":
                        case "/s":
                            silent = true;
                            break;
                        case "-n":
                            vpkname = args[i+1].ToString();
                            // It doesn't like client vpk names less than 2
                            // so add a 0 in front of it
                            for (var x = vpkname.Length;x < 3;x++) {
                              vpkname = $"0{vpkname}";
                            }
                            break;
                        case "-d":
                            directory = args[i+1].ToString() + Path.DirectorySeparatorChar;
                            break;
                        default:
                            if (directory=="")
                                directory = vpkdir.Replace(".vpk", "") + Path.DirectorySeparatorChar;
                            vpkarch = vpkdir.Replace("_dir.vpk", $"_{vpkname}.vpk").Replace("english", "");
                            break;
                    }
                }
            }

            Console.WriteLine($"VPK directory: {vpkdir}\n" +
                $"VPK archive: {vpkarch}\n" +
                $"Directory: {directory}");

            var filesList = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Select(path => path.Replace(directory, "").Replace(Path.DirectorySeparatorChar, '/')).ToList();
            var filesEdit = new List<string>();
            var filesDelete = new List<string>();

            foreach(var file in filesList)
            {
                if (file.EndsWith(".delete"))
                {
                    filesDelete.Add(file.Replace(".delete", null)); // Efficiency! ecksde
                }
                else
                {
                    filesEdit.Add(file);
                }
            }
            filesList = null; // Dispose ecksde

            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var edit in filesEdit)
            {
                Console.WriteLine($"\t[+]{edit}");
            }
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var edit in filesDelete)
            {
                Console.WriteLine($"\t[-]{edit}");
            }
            Console.ResetColor();


            if (!silent)
            {
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
            }

            var fstream = new FileStream(vpkdir, FileMode.Open, FileAccess.ReadWrite);
            var k0k = new FileStream(vpkarch, FileMode.OpenOrCreate, FileAccess.Write);
            k0k.Position = 0;
            k0k.SetLength(0);

            var writer = new BinaryWriter(fstream);
            var vpk = new VPK.DirFile(fstream);
            Console.WriteLine($"{vpk.Header.DirectorySize:X4} | {vpk.Header.EmbeddedChunkSize:X4}");

            var list = vpk.EntryBlocks.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                var block = list[i];
                string kek = null;

                foreach (var edit in filesEdit)
                {
                    if (edit == block.Path)
                    {
                        var bak = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Replacing {edit}...");
                        Console.ForegroundColor = bak;

                        var fb = File.ReadAllBytes(directory + edit);
                        if (fb.Length == 0)
                            throw new Exception("Brih");

                        list[i] = new VPK.DirEntryBlock(fb, (ulong)k0k.Position, Convert.ToUInt16(vpkname), 0x101, 0, block.Path);

                        k0k.Write(fb);
                        k0k.Flush();

                        kek = edit;
                        break;
                    }
                }

                if (kek != null)
                    filesEdit.Remove(kek);
                else
                {
                    foreach(var edit in filesDelete)
                    {
                        if (edit == block.Path)
                        {
                            var bak = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Deleting {edit}...");
                            Console.ForegroundColor = bak;

                            list.RemoveAt(i);
                            i--; // Negate ++

                            kek = edit;

                            break;
                        }
                    }

                    if (kek != null)
                        filesDelete.Remove(kek);
                }
            }

            // if there are still files left...
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var edit in filesEdit)
            {
                Console.WriteLine($"Adding {edit}...");

                var fb = File.ReadAllBytes(directory + edit);
                if (fb.Length == 0)
                    throw new Exception("Brih");

                list.Add(new VPK.DirEntryBlock(fb, (ulong)k0k.Position, Convert.ToUInt16(vpkname), 0x101, 0, edit));

                k0k.Write(fb);
                k0k.Flush();
            }
            Console.ResetColor();

            writer.BaseStream.Position = 0;
            VPK.DirFile.Write(writer, list.ToArray());

            Console.WriteLine("Done!\nPress Enter to exit!");
            if(!silent)
                Console.ReadLine();
        }
    }
}
