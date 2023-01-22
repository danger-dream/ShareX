using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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

        public static bool Clear()
        {
            try
            {
                IDataObject data = new DataObject();
                CopyData(data, false);
            }
            catch
            {
                // ignored
            }

            return false;
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

        public static bool CopyImage(Image img)
        {
            if (img == null) return false;
            try
            {
                return CopyImageAlternative2(img);

                //return CopyImageDefault(img);
            }
            catch
            {
                // ignored
            }
            return false;
        }

        private static bool CopyImageDefault(Image img)
        {
            IDataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.Bitmap, true, img);

            return CopyData(dataObject);
        }

        public static byte[] GetImageData(Bitmap sourceImage, out int stride)
        {
            BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, sourceImage.PixelFormat);
            stride = sourceData.Stride;
            byte[] data = new byte[stride * sourceImage.Height];
            Marshal.Copy(sourceData.Scan0, data, 0, data.Length);
            sourceImage.UnlockBits(sourceData);
            return data;
        }

        private static void WriteIntToByteArray(byte[] data, int startIndex, int bytes, bool littleEndian, uint value)
        {
            var lastByte = bytes - 1;
            for (var index = 0; index < bytes; index++)
            {
                var offs = startIndex + (littleEndian ? index : lastByte - index);
                data[offs] = (byte)(value >> (8 * index) & 0xFF);
            }
        }

        private static byte[] ConvertToDib(Image image)
        {
            byte[] bm32BData;
            var width = image.Width;
            var height = image.Height;
            using (var bm32B = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb))
            {
                using (var gr = Graphics.FromImage(bm32B))
                    gr.DrawImage(image, new Rectangle(0, 0, bm32B.Width, bm32B.Height));
                bm32B.RotateFlip(RotateFlipType.Rotate180FlipX);
                bm32BData = GetImageData(bm32B, out _);
            }
            const int hdrSize = 0x28;
            var fullImage = new byte[hdrSize + 12 + bm32BData.Length];
            //Int32 biSize;
            WriteIntToByteArray(fullImage, 0x00, 4, true, hdrSize);
            //Int32 biWidth;
            WriteIntToByteArray(fullImage, 0x04, 4, true, (uint)width);
            //Int32 biHeight;
            WriteIntToByteArray(fullImage, 0x08, 4, true, (uint)height);
            //Int16 biPlanes;
            WriteIntToByteArray(fullImage, 0x0C, 2, true, 1);
            //Int16 biBitCount;
            WriteIntToByteArray(fullImage, 0x0E, 2, true, 32);
            WriteIntToByteArray(fullImage, 0x10, 4, true, 3);
            //Int32 biSizeImage;
            WriteIntToByteArray(fullImage, 0x14, 4, true, (uint)bm32BData.Length);
            // These are all 0. Since .net clears new arrays, don't bother writing them.

            WriteIntToByteArray(fullImage, hdrSize + 0, 4, true, 0x00FF0000);
            WriteIntToByteArray(fullImage, hdrSize + 4, 4, true, 0x0000FF00);
            WriteIntToByteArray(fullImage, hdrSize + 8, 4, true, 0x000000FF);
            Array.Copy(bm32BData, 0, fullImage, hdrSize + 12, bm32BData.Length);
            return fullImage;
        }


        private static bool CopyImageAlternative2(Image img)
        {
            using (var bmpNonTransparent = img.CreateEmptyBitmap(PixelFormat.Format24bppRgb))
            using (var msPng = new MemoryStream())
            using (var msDib = new MemoryStream())
            {
                IDataObject dataObject = new DataObject();

                using (var g = Graphics.FromImage(bmpNonTransparent))
                {
                    g.Clear(Color.White);
                    g.DrawImage(img, 0, 0, img.Width, img.Height);
                }

                dataObject.SetData(DataFormats.Bitmap, true, bmpNonTransparent);

                img.Save(msPng, ImageFormat.Png);
                dataObject.SetData("PNG", false, msPng);

                var dibData = ConvertToDib(img);
                msDib.Write(dibData, 0, dibData.Length);
                dataObject.SetData(DataFormats.Dib, false, msDib);
                return CopyData(dataObject);
            }
        }

        public static string GetText(bool checkContainsText = false)
        {
            try
            {
                lock (ClipboardLock)
                {
                    if (!checkContainsText || Clipboard.ContainsText())
                    {
                        return Clipboard.GetText();
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}
