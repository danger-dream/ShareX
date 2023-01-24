using System;
using System.Windows.Forms;

namespace ShareX
{
    public static class ClipboardHelpers
    {
        private const int RetryTimes = 20;
        private const int RetryDelay = 100;

        private static readonly object ClipboardLock = new object();

        private static bool CopyData(IDataObject data, bool copy = true)
        {
            if (data == null) return false;
            lock (ClipboardLock)
            {
                Clipboard.SetDataObject(data, copy, RetryTimes, RetryDelay);
            }
            return true;

        }

        public static bool CopyText(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            try
            {
                IDataObject data = new DataObject();
                string dataFormat;

                if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 5)
                {
                    dataFormat = DataFormats.Text;
                }
                else
                {
                    dataFormat = DataFormats.UnicodeText;
                }

                data.SetData(dataFormat, false, text);
                return CopyData(data);
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }
}
