# RSPNVPK

**Author:** [mrsteyk](https://github.com/mrsteyk)

![Release Build](https://github.com/taskinoz/RSPNVPK/actions/workflows/ci.yml/badge.svg)

## About

RSPNVPK is a command line VPK repacking tool for Titanfall 2.

## Usage

To repack a VPK you need to run RSPNVPK with the `englishclient_<VPKNAME>.bsp.pak000_dir.vpk` and a folder with the same name without the `.vpk` extension.

```
RSPNVPK englishclient_<VPKNAME>.bsp.pak000_dir.vpk
```

View RSPNVPK command arguments with `-h`

This will give you a `englishclient_<VPKNAME>.bsp.pak000_dir.vpk` and a `client_<VPKNAME>.bsp.pak000_228.vpk`.

If you dont want to change the VPK number to something other than 228 you can use `-n <number>`

To repack without the backup warning you can use `-s` or `/s`

Create a backup with `-b` or type `y` in the console when it asks about a backup.

Support for `.vpkignore` so files don't accidentally get packed with your VPK
