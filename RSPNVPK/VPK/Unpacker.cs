using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RSPNVPK.VPK
{
    internal static class Unpacker
    {
        /// <summary>
        /// Extracts a VPK directory file (and all of its archives) into the output directory.
        /// Accepts either a *_dir.vpk or one of its archive files (pak000_NNN.vpk).
        /// </summary>
        public static void Unpack(string vpkFile, string outputDir)
        {
            var dirFile = vpkFile;
            if (!dirFile.EndsWith("_dir.vpk"))
                dirFile = ArchiveToDirFile(dirFile);

            if (!File.Exists(dirFile))
                throw new FileNotFoundException($"Could not find the VPK directory file: {dirFile}", dirFile);

            Directory.CreateDirectory(outputDir);

            using (var fs = new FileStream(dirFile, FileMode.Open, FileAccess.Read))
            {
                var vpk = new DirFile(fs);
                var total = 0L;

                foreach (var archiveGroup in vpk.EntryBlocks.GroupBy(b => b.FileIdx))
                {
                    var archive = ArchivePathForIndex(dirFile, archiveGroup.Key);
                    if (!File.Exists(archive))
                    {
                        Console.WriteLine($"Warning: archive {archive} not found, skipping {archiveGroup.Count()} file(s)");
                        continue;
                    }

                    Console.WriteLine($"Extracting from {archive}...");

                    using (var afs = new FileStream(archive, FileMode.Open, FileAccess.Read))
                    {
                        foreach (var block in archiveGroup)
                        {
                            ExtractBlock(afs, block, outputDir);
                            total++;
                        }
                    }
                }

                Console.WriteLine($"Done! Extracted {total} file(s) to {outputDir}");
            }
        }

        private static void ExtractBlock(FileStream archive, DirEntryBlock block, string outputDir)
        {
            var rel = block.Path.Replace('/', Path.DirectorySeparatorChar);
            var outPath = Path.Combine(outputDir, rel);
            var outDir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDir))
                Directory.CreateDirectory(outDir);

            using (var ofs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                if (block.PreloadData != null && block.PreloadData.Length > 0)
                    ofs.Write(block.PreloadData, 0, block.PreloadData.Length);

                foreach (var entry in block.Entries)
                {
                    if (entry.Offset + entry.CompressedSize > (ulong)archive.Length)
                        throw new Exception($"Entry out of range for {block.Path}: offset {entry.Offset}, size {entry.CompressedSize} (archive length {archive.Length})");

                    archive.Seek((long)entry.Offset, SeekOrigin.Begin);
                    var data = new byte[entry.CompressedSize];
                    ReadFully(archive, data);

                    if (entry.Compressed)
                        data = Utils.DecompressMemory(data, entry.DecompressedSize);

                    ofs.Write(data, 0, data.Length);
                }
            }
        }

        /// <summary>
        /// Turns an archive file name like englishclient_foo.bsp.pak000_005.vpk into its _dir.vpk name.
        /// </summary>
        private static string ArchiveToDirFile(string archivePath)
        {
            var name = Path.GetFileName(archivePath);
            name = Regex.Replace(name, @"pak000_\d+", "pak000_dir");
            if (name.StartsWith("client_"))
                name = "english" + name;

            var dir = Path.GetDirectoryName(archivePath);
            return string.IsNullOrEmpty(dir) ? name : Path.Combine(dir, name);
        }

        /// <summary>
        /// Builds the archive file name for the given archive index, e.g.
        /// englishclient_frontend.bsp.pak000_dir.vpk + 5 => client_frontend.bsp.pak000_005.vpk
        /// </summary>
        private static string ArchivePathForIndex(string dirFile, int index)
        {
            var name = Path.GetFileName(dirFile).Replace("_dir.vpk", $"_{index:000}.vpk");
            if (name.StartsWith("english"))
                name = name.Substring("english".Length);

            var dir = Path.GetDirectoryName(dirFile);
            return string.IsNullOrEmpty(dir) ? name : Path.Combine(dir, name);
        }

        private static void ReadFully(Stream stream, byte[] buffer)
        {
            var read = 0;
            while (read < buffer.Length)
            {
                var n = stream.Read(buffer, read, buffer.Length - read);
                if (n == 0)
                    throw new EndOfStreamException("Unexpected end of archive while reading entry");
                read += n;
            }
        }
    }
}
