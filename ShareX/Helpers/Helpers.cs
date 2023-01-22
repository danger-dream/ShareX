using System;
using System.IO;
using System.Windows.Forms;

namespace ShareX
{
    public static class Helpers
    {
        public static readonly Version OSVersion = Environment.OSVersion.Version;

        public static bool IsWindowsVista()
        {
            return OSVersion.Major == 6;
        }

        public static bool IsWindowsVistaOrGreater()
        {
            return OSVersion.Major >= 6;
        }

        public static bool IsWindows7()
        {
            return OSVersion.Major == 6 && OSVersion.Minor == 1;
        }

        public static bool IsWindows10OrGreater(int build = -1)
        {
            return OSVersion.Major >= 10 && OSVersion.Build >= build;
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