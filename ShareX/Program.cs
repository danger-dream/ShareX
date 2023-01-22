using System;

namespace ShareX
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                using (var form = new RegionCaptureForm())
                {
                    form.ShowDialog();
                }
            }
            catch
            {
                ClipboardHelpers.CopyText("ocr:empty");
                // ignored
            }
        }
    }
}