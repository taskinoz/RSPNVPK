using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace RSPNVPK
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Run(args);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {e.Message}");
                Console.ResetColor();
            }
        }

        static void Run(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Invalid usage, use -h for help");
                return;
            }

            if (args[0] == "-h" || args[0] == "/h" || args[0] == "--help")
            {
                PrintHelp();
                return;
            }

            foreach (var a in args)
            {
                switch (a)
                {
                    case "-c":
                    case "/c":
                    case "pack":
                        CreateVpk(args);
                        return;
                    case "-x":
                    case "/x":
                    case "-u":
                    case "/u":
                    case "unpack":
                    case "extract":
                        UnpackVpk(args);
                        return;
                }
            }

            PatchVpk(args);
        }

        static void PrintHelp()
        {
            Console.WriteLine(
                "RSPNVPK <VPKNAME> - repack a VPK patch\n" +
                "   -h - Help\n" +
                "   -s - Run without warning about backups\n" +
                "   -n - The number of the client VPK\n" +
                "   -d - Choose the directory with the files you're packing\n" +
                "   -b - Create a backup of the englishclient VPK with the extension .backup\n" +
                "   -o - Choose the output directory for the new VPK (defaults to current directory if not specified)\n" +
                "   -z - Compress the packed files with lzham\n\n" +
                "RSPNVPK -c <name> -d <directory> [-o output] [-n number] [-z] [-s]\n" +
                "   Create a brand new VPK from a directory of files.\n" +
                "   <name> is the new VPK name, e.g. -c englishclient_myvpk.bsp.pak000_dir.vpk\n" +
                "   or a plain name like -c myvpk which becomes englishclient_myvpk.bsp.pak000_dir.vpk\n\n" +
                "RSPNVPK -x <file> [-o output]\n" +
                "   Unpack a VPK. Accepts a *_dir.vpk or one of its pak000_NNN.vpk archive files.\n" +
                "   -o chooses where the files are extracted (defaults to the current directory)"
            );
        }

        static void PatchVpk(string[] args)
        {
            var silent = false;
            var vpkname = "228";
            var vpkdir = args[0];
            var output = "";
            var compress = false;
            var directory = "";

            for (var i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-s":
                    case "/s":
                        silent = true;
                        break;
                    case "-n":
                        vpkname = args[++i];
                        // It doesn't like client vpk names less than 2
                        // so add a 0 in front of it
                        for (var x = vpkname.Length; x < 3; x++)
                            vpkname = $"0{vpkname}";
                        break;
                    case "-d":
                        directory = args[++i] + Path.DirectorySeparatorChar;
                        // Check if directory exists
                        if (!Directory.Exists(directory))
                        {
                            Console.WriteLine($"Directory {directory} does not exist");
                            return;
                        }
                        break;
                    case "-b":
                        silent = true;
                        System.IO.File.Copy(vpkdir, vpkdir + ".backup", true);
                        break;
                    case "-o":
                        output = args[++i] + Path.DirectorySeparatorChar;
                        // Check if output directory exists, if not create it
                        if (!Directory.Exists(output))
                            Directory.CreateDirectory(output);
                        break;
                    case "-z":
                    case "/z":
                        compress = true;
                        break;
                    default:
                        break;
                }
            }

            if (!vpkdir.EndsWith("_dir.vpk"))
            {
                Console.WriteLine($"Invalid directory file {vpkdir}");
                return;
            }

            var vpkarch = vpkdir.Replace("_dir.vpk", $"_{vpkname}.vpk").Replace("english", "");
            if (directory == "")
                directory = vpkdir.Replace(".vpk", "") + Path.DirectorySeparatorChar;

            Console.WriteLine($"VPK directory: {vpkdir}\n" +
                $"VPK archive: {vpkarch}\n" +
                $"Directory: {directory}");

            // .vpkignore logic
            var ignoreFileDir = $"{directory}.vpkignore";
            string[] ignoreFiles = { };
            if (System.IO.File.Exists(ignoreFileDir))
            {
                ignoreFiles = System.IO.File.ReadAllLines(ignoreFileDir);
                // Remove ignore comments
                ignoreFiles = ignoreFiles.Where(val => !val.Contains("#")).ToArray();
                ignoreFiles = ignoreFiles.Concat(new string[] { ".vpkignore" }).ToArray();
            }

            var filesList = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Select(path => path.Replace(directory, "").Replace(Path.DirectorySeparatorChar, '/')).ToList();
            var filesEdit = new List<string>();
            var filesDelete = new List<string>();

            foreach (var file in filesList)
            {
                if (file.EndsWith(".delete"))
                {
                    filesDelete.Add(file.Replace(".delete", null)); // Efficiency! ecksde
                }
                else
                {
                    // Check if file is in .vpkignore list
                    bool ignore = false;
                    foreach (var check in ignoreFiles)
                    {
                        if (file.Contains(check))
                            ignore = true;
                    }
                    if (!ignore)
                        filesEdit.Add(file);
                }
            }
            filesList = null; // Dispose ecksde

            Console.ForegroundColor = ConsoleColor.Blue;
            foreach (var edit in ignoreFiles)
            {
                Console.WriteLine($"\t[#]{(edit.EndsWith("/") ? edit + ".." : edit)}");
            }
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
 ____  ____  ____  _   _ _     _ ____  _  __
|  _ \/ ___||  _ \| \ | | \   / |  _ \| |/ /
| |_) \___ \| |_) |  \| |\ \ / /| |_) | ' /
|  _ < ___) |  __/| |\  | \ V / |  __/| . \
|_| \_|____/|_|   |_| \_|  \_/  |_|   |_|\_\
");
                Console.WriteLine($"Would you like to backup {vpkdir}? y/n");
                if (Console.ReadLine().ToLower() == "y")
                    System.IO.File.Copy(vpkdir, vpkdir + ".backup", true);
            }

            var fstream = new FileStream(vpkdir, FileMode.Open, FileAccess.ReadWrite);
            var k0k = new FileStream(output + vpkarch, FileMode.OpenOrCreate, FileAccess.Write);
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

                        WritePacked(block.Path, edit, directory, k0k, vpkname, compress, ref list, i);

                        kek = edit;
                        break;
                    }
                }

                if (kek != null)
                    filesEdit.Remove(kek);
                else
                {
                    foreach (var edit in filesDelete)
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

                WritePacked(edit, edit, directory, k0k, vpkname, compress, ref list, -1);
            }
            Console.ResetColor();

            writer.BaseStream.Position = 0;
            VPK.DirFile.Write(writer, list.ToArray());

            Console.WriteLine("Done!\nPress Enter to exit!");
            if (!silent)
                Console.ReadLine();
        }

        // Writes a file into the archive stream and adds its block to the list.
        // If listIndex is -1 the block is appended, otherwise it replaces list[listIndex].
        static void WritePacked(string blockPath, string edit, string directory, FileStream k0k, string vpkname, bool compress, ref List<VPK.DirEntryBlock> list, int listIndex)
        {
            var fb = File.ReadAllBytes(directory + edit);
            if (fb.Length == 0)
                throw new Exception("Brih");

            var packed = VPK.Packer.Pack(fb, (ulong)k0k.Position, Convert.ToUInt16(vpkname), VPK.Packer.DEFAULT_FLAGS, VPK.Packer.DEFAULT_FLAGS2, blockPath, compress);

            if (listIndex == -1)
                list.Add(packed.Block);
            else
                list[listIndex] = packed.Block;

            foreach (var chunk in packed.Chunks)
                k0k.Write(chunk);
            k0k.Flush();
        }

        static void CreateVpk(string[] args)
        {
            var name = "";
            var directory = "";
            var output = "";
            var vpknum = "000";
            var compress = false;
            var silent = false;

            for (var i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-c":
                    case "/c":
                    case "pack":
                        break;
                    case "-d":
                        directory = args[++i];
                        break;
                    case "-o":
                        output = args[++i];
                        break;
                    case "-n":
                        vpknum = args[++i];
                        for (var x = vpknum.Length; x < 3; x++)
                            vpknum = $"0{vpknum}";
                        break;
                    case "-z":
                    case "/z":
                        compress = true;
                        break;
                    case "-s":
                    case "/s":
                        silent = true;
                        break;
                    default:
                        if (name == "")
                            name = args[i];
                        else
                            Console.WriteLine($"Ignoring unknown argument: {args[i]}");
                        break;
                }
            }

            if (name == "")
            {
                Console.WriteLine("Invalid usage: -c requires a VPK name (use -h for help)");
                return;
            }

            if (directory == "" || !Directory.Exists(directory))
            {
                Console.WriteLine($"Directory {directory} does not exist");
                return;
            }
            directory = directory.TrimEnd(Path.DirectorySeparatorChar, '/') + Path.DirectorySeparatorChar;

            if (!name.EndsWith("_dir.vpk"))
                name = $"englishclient_{name}.bsp.pak000_dir.vpk";

            var archName = name.Replace("_dir.vpk", $"_{vpknum}.vpk");
            if (archName.StartsWith("english"))
                archName = archName.Substring("english".Length);

            if (output == "")
                output = ".";
            Directory.CreateDirectory(output);

            var dirPath = Path.Combine(output, name);
            var archPath = Path.Combine(output, archName);

            Console.WriteLine($"VPK directory: {dirPath}\n" +
                $"VPK archive: {archPath}\n" +
                $"Directory: {directory}\n" +
                (compress ? "Compression: lzham\n" : "Compression: none\n"));

            var files = GetPackableFiles(directory);

            var blocks = new List<VPK.DirEntryBlock>();

            Console.ForegroundColor = ConsoleColor.Green;
            using (var k0k = new FileStream(archPath, FileMode.Create, FileAccess.Write))
            {
                foreach (var file in files)
                {
                    Console.WriteLine($"Packing {file}...");

                    var fullPath = Path.Combine(directory, file.Replace('/', Path.DirectorySeparatorChar));
                    var fb = File.ReadAllBytes(fullPath);

                    var packed = VPK.Packer.Pack(fb, (ulong)k0k.Position, Convert.ToUInt16(vpknum), VPK.Packer.DEFAULT_FLAGS, VPK.Packer.DEFAULT_FLAGS2, file, compress);

                    blocks.Add(packed.Block);
                    foreach (var chunk in packed.Chunks)
                        k0k.Write(chunk);
                    k0k.Flush();
                }
            }
            Console.ResetColor();

            using (var fs = new FileStream(dirPath, FileMode.Create, FileAccess.ReadWrite))
            {
                var writer = new BinaryWriter(fs);
                VPK.DirFile.Write(writer, blocks.OrderBy(b => Path.GetExtension(b.Path)).ThenBy(b => b.Path).ToArray());
            }

            Console.WriteLine($"Created {dirPath} and {archPath}");
            if (!silent)
                Console.WriteLine("Press Enter to exit!");
            if (!silent)
                Console.ReadLine();
        }

        // Enumerates files in a directory, applying .vpkignore rules.
        static List<string> GetPackableFiles(string directory)
        {
            var ignoreFileDir = $"{directory}.vpkignore";
            string[] ignoreFiles = { };
            if (System.IO.File.Exists(ignoreFileDir))
            {
                ignoreFiles = System.IO.File.ReadAllLines(ignoreFileDir);
                ignoreFiles = ignoreFiles.Where(val => !val.Contains("#")).ToArray();
                ignoreFiles = ignoreFiles.Concat(new string[] { ".vpkignore" }).ToArray();
            }

            var files = new List<string>();
            foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                var file = path.Replace(directory, "").TrimStart(Path.DirectorySeparatorChar, '/').Replace(Path.DirectorySeparatorChar, '/');

                var ignore = false;
                foreach (var check in ignoreFiles)
                {
                    if (file.Contains(check))
                        ignore = true;
                }
                if (!ignore)
                    files.Add(file);
            }

            return files;
        }

        static void UnpackVpk(string[] args)
        {
            var file = "";
            var output = "";

            for (var i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-x":
                    case "/x":
                    case "-u":
                    case "/u":
                    case "unpack":
                    case "extract":
                        break;
                    case "-o":
                        output = args[++i];
                        break;
                    default:
                        if (file == "")
                            file = args[i];
                        else
                            Console.WriteLine($"Ignoring unknown argument: {args[i]}");
                        break;
                }
            }

            if (file == "")
            {
                Console.WriteLine("Invalid usage: -x requires a VPK file (use -h for help)");
                return;
            }

            if (output == "")
                output = ".";

            VPK.Unpacker.Unpack(file, output);
        }
    }
}
