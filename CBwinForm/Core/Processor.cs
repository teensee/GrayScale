using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CBwinForm.Core
{
    public class ImageProcessor
    {
        public Bitmap Image { get; set; }

        public ImageProcessor(Bitmap image)
        {
            Image = image;
        }

        public Bitmap SetGrayScale()
        {
            Rectangle rect = new Rectangle(0, 0, Image.Width, Image.Height);

            BitmapData bitData = Image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            IntPtr intPtr = bitData.Scan0;

            int numBytes = Image.Width * Image.Height * 3;
            byte[] rgbValues = new byte[numBytes];

            Marshal.Copy(intPtr, rgbValues, 0, numBytes);

            for (int i = 0; i < numBytes; i += 3)
            {
                double val = rgbValues[i] * 0.299 + rgbValues[i + 1] * 0.587 + rgbValues[i + 2] * 0.114;
                byte color_b = 0;

                color_b = Convert.ToByte(val);

                rgbValues[i] = rgbValues[i + 1] = rgbValues[i + 2] = color_b;
            }

            Marshal.Copy(rgbValues, 0, intPtr, numBytes);

            Image.UnlockBits(bitData);

            return Image;
        }

        public Bitmap ChangeBrightness()
        {
            Rectangle rect = new Rectangle(0, 0, Image.Width, Image.Height);

            BitmapData bitData = Image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            IntPtr intPtr = bitData.Scan0;


            int numBytes = bitData.Stride * bitData.Height;
            byte[] rgbValues = new byte[numBytes];

            Marshal.Copy(intPtr, rgbValues, 0, numBytes);

            double coef = 0.012;
            byte newbrightness = 0;
            for (int i = 0; i < numBytes; i++)
            {
                var z = Math.Pow(rgbValues[i], 2) * coef;

                if (z > 255) z = 255;

                newbrightness = Convert.ToByte(z);

                rgbValues[i] = newbrightness;
            }

            Marshal.Copy(rgbValues, 0, intPtr, numBytes);
            Image.UnlockBits(bitData);

            return Image;
        }

    }
}
