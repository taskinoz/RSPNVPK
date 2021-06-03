using System;
using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Compression
{
    public class CompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public CompressionHandle() : base(true) { }

        public uint Finish()
        {
            handle = IntPtr.Zero;
            return Lzham.CompressDeinit(handle);
        }

        protected override bool ReleaseHandle()
        {
            Finish();
            return true;
        }
    }
}