# RSPNVPK

**Author:** [mrsteyk](https://github.com/mrsteyk)

![Release Build](https://github.com/taskinoz/RSPNVPK/actions/workflows/ci.yml/badge.svg)

## About

RSPNVPK is a command line VPK repacking tool for Titanfall 1 and 2 (mainly 2). It can repack an existing VPK patch, pack a brand new directory of files into a VPK, unpack existing VPKs and optionally compress with lzham.

## Usage

View all command arguments with `-h`.

### Repack a patch

To repack a VPK you need to run RSPNVPK with the `englishclient_<VPKNAME>.bsp.pak000_dir.vpk` and a folder with the same name without the `.vpk` extension.

```
RSPNVPK englishclient_<VPKNAME>.bsp.pak000_dir.vpk
```

This will give you a `englishclient_<VPKNAME>.bsp.pak000_dir.vpk` and a `client_<VPKNAME>.bsp.pak000_228.vpk`.

- `-n <number>` - Change the VPK number from the default 228
- `-s` / `/s` - Repack without the backup warning
- `-b` - Create a backup of the `englishclient` VPK with the extension `.backup`
- `-o` - Select an output directory (defaults to the current directory)
- `-z` / `/z` - Compress the packed files with lzham
- `.vpkignore` - List files here so they don't accidentally get packed with your VPK

### Create a new VPK

Pack a directory of files into a brand new VPK:

```
RSPNVPK -c <name> -d <directory> [-o output] [-n number] [-z] [-s]
```

`<name>` is the new VPK name, e.g. `-c englishclient_myvpk.bsp.pak000_dir.vpk` or a plain name like `-c myvpk` which becomes `englishclient_myvpk.bsp.pak000_dir.vpk`. This produces an `englishclient_<name>.bsp.pak000_dir.vpk` and a `client_<name>.bsp.pak000_000.vpk`. Use `-z` to compress with lzham.

### Unpack a VPK

Extract a VPK (and all of its archives) to a directory:

```
RSPNVPK -x <file> [-o output]
```

`<file>` can be a `*_dir.vpk` or one of its `pak000_NNN.vpk` archive files. Archives are looked for next to the directory file. `-o` chooses where the files are extracted (defaults to the current directory).

## Related tools

- https://github.com/harmonytf/HarmonyVPKTool
- https://github.com/barnabwhy/TFVPKTool
- https://github.com/pg9182/tf2vpk
- https://github.com/Unordinal/UnoVPKTool
- https://github.com/Mauler125/vpk_qt
- https://github.com/r-ex/r5vpk

## Notes

- lzham compression uses the `lzham_alpha` build (the 6 argument API, as used by r5vpk) bundled in `LzhamWrapper` — the game's compressed data will not decompress with the newer 7 argument `lzham` final release.
- The `vpk` directory in the repo root has an example of 2 archives of VPKs, it is ignored by git because the files are too large to host on GitHub.
