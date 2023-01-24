using System;
using System.IO;
using System.Windows.Forms;

namespace ShareX
{
    public static class Helpers
    {
        public static readonly Version OSVersion = Environment.OSVersion.Version;

        public static bool IsWindowsVistaOrGreater()
        {
            return OSVersion.Major >= 6;
        }

        public static Cursor CreateCursor(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return new Cursor(ms);
            }
        }
    }
}