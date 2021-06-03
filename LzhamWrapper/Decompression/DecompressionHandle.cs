using System;
using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Decompression
{
    public class DecompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public DecompressionHandle() : base(true) { }

        public uint Finish()
        {
            handle = IntPtr.Zero;
            return Lzham.DecompressDeinit(handle);
        }

        protected override bool ReleaseHandle()
        {
            Finish();
            return true;
        }
    }
}