using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RSPNVPK
{
    public static class Extensions
    {
        public static string ReadNTString(this BinaryReader reader)
        {
            var vs = new List<byte>(128);
            var b = reader.ReadByte();
            while (b != 0)
            {
                vs.Add(b);
                b = reader.ReadByte();
            }

            return Encoding.ASCII.GetString(vs.ToArray());
        }

        public static void WriteSourceString(this BinaryWriter writer, string s, bool useNullTerminator = true)
        {
            writer.Write(Encoding.UTF8.GetBytes(s));

            if (useNullTerminator)
                writer.Write((byte)0x00);
        }
    }
}
