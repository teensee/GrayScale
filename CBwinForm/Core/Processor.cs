using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CBwinForm.Core
{
    public class SimpleImageProcessor
    {
        #region Public Properties

        /// <summary>
        /// Bitmap object 
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// Gray Scale vanila image
        /// </summary>
        public Bitmap GrayScaleImage { get; set; }

        /// <summary>
        /// Rectangle of our image
        /// </summary>
        public Rectangle Rect { get; set; }

        /// <summary>
        /// Width*Height
        /// </summary>
        public int Resolution { get; set; }

        /// <summary>
        /// Частота расределения пикселей
        /// </summary>
        public int[] FrequencyX { get; set; } = new int[256];

        /// <summary>
        /// 
        /// </summary>
        public double[] ColorAllocation { get; set; } = new double[256];

        public double SigmaY
        {
            get => sigmaY;
            set
            {
                if (sigmaY == value)
                    return;

                sigmaY = value;
            }
        }

        #endregion

        #region Private Members

        private byte[] originalByteImage;

        private byte[] grayScaledByteArray;

        private byte[] originalGrayScaleImage;

        /// <summary>
        /// Store R*0.299 + G*0.587 + B*0.114 
        /// </summary>
        private double val;

        /// <summary>
        /// Store converted <see cref="val"/> in byte 
        /// </summary>
        private byte color_b;

        private double sigmaY;

        private double coef;

        #endregion

        #region Constructor

        public SimpleImageProcessor(Bitmap image)
        {
            Image = image;
            BitmapToByteArray(Image);

            Rect = new Rectangle(0, 0, Image.Width, Image.Height);
            Resolution = Image.Width * Image.Height;
        }

        #endregion

        #region Glay Scale

        /// <summary>
        /// Sets gray-scale filter 
        /// </summary>
        /// <returns></returns>
        public Bitmap SetGrayScale()
        {
            #region test
            //BitmapData bitData = Image.LockBits(Rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //IntPtr intPtr = bitData.Scan0;

            //int numBytes = bitData.Height * bitData.Stride;
            //byte[] rgbValues = new byte[numBytes];

            //Marshal.Copy(intPtr, rgbValues, 0, numBytes);

            //for (int i = 0; i < numBytes; i += 3)
            //{
            //    val = rgbValues[i] * 0.299 + rgbValues[i + 1] * 0.587 + rgbValues[i + 2] * 0.114;
            //    color_b = 0;

            //    color_b = Convert.ToByte(val);

            //    //Частота 
            //    FrequencyX[color_b]++;

            //    rgbValues[i] = rgbValues[i + 1] = rgbValues[i + 2] = color_b;
            //}

            //Marshal.Copy(rgbValues, 0, intPtr, numBytes);

            //Image.UnlockBits(bitData);

            //CalculateSigmaY();

            //return Image; 
            #endregion

            for (int i = 0; i < originalByteImage.Length; i += 3)
            {
                val = originalByteImage[i] * 0.299 + originalByteImage[i + 1] * 0.587 + originalByteImage[i + 2] * 0.114;
                color_b = 0;

                color_b = Convert.ToByte(val);

                //Частота 
                FrequencyX[color_b]++;

                grayScaledByteArray[i] = grayScaledByteArray[i + 1] = grayScaledByteArray[i + 2] = color_b;
                originalGrayScaleImage[i] = originalGrayScaleImage[i + 1] = originalGrayScaleImage[i + 2] = color_b;
            }

            CalculateSigmaY();

            return ByteArrayToBitmap(grayScaledByteArray);
        }

        #endregion

        public Bitmap SetNewBrightness(double cf)
        {
            #region Test
            //var calculatedCoef = cf;

            //BitmapData bitData = Image.LockBits(Rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //IntPtr intPtr = bitData.Scan0;

            //int numBytes = bitData.Stride * bitData.Height;
            //byte[] rgbValues = new byte[numBytes];

            //Marshal.Copy(intPtr, rgbValues, 0, numBytes);

            //byte newbrightness = 0;
            //for (int i = 0; i < numBytes; i += 3)
            //{
            //    var z = Math.Pow(rgbValues[i], 2) * calculatedCoef;

            //    if (z > 255) z = 255;

            //    newbrightness = Convert.ToByte(z);

            //    rgbValues[i] = rgbValues[i + 1] = rgbValues[i + 2] = newbrightness;
            //}

            //Marshal.Copy(rgbValues, 0, intPtr, numBytes);
            //Image.UnlockBits(bitData);

            //return Image;
            #endregion

            Array.Copy(originalGrayScaleImage, grayScaledByteArray, grayScaledByteArray.Length);

            byte newbrightness = 0;
            for (int i = 0; i < grayScaledByteArray.Length; i += 3)
            {
                var z = Math.Pow(grayScaledByteArray[i], 2) * cf;

                if (z > 255) z = 255;

                newbrightness = Convert.ToByte(z);

                grayScaledByteArray[i] = grayScaledByteArray[i + 1] = grayScaledByteArray[i + 2] = newbrightness;
            }

            return ByteArrayToBitmap(grayScaledByteArray);
        }

        public void CalculateSigmaY()
        {
            double firstTest = 0.0f;
            double secondTest = 0.0f;

            double test = 0.0f;
            for (int i = 0; i < FrequencyX.Length; i++)
            {
                ColorAllocation[i] = FrequencyX[i] / (double)Resolution;

                firstTest += Math.Pow(i, 2) * ColorAllocation[i];
                secondTest += i * ColorAllocation[i];

                test += ColorAllocation[i];
            }

            secondTest *= secondTest;

            SigmaY = Math.Sqrt((firstTest - secondTest));
        }

        public double CalculateNewCoef(int ScrollBarSelectedValue)
        {
            double SigmaZ = ScrollBarSelectedValue;

            double firstVal = 0.0f;
            double SecondVal = 0.0f;

            for (int i = 0; i < FrequencyX.Length; i++)
            {
                //ColorAllocation[i] = FrequencyX[i] / (double)Resolution;
                firstVal += Math.Pow(i, 4) * ColorAllocation[i];
                SecondVal += Math.Pow(i, 2) * ColorAllocation[i];
            }

            SecondVal *= SecondVal;

            var LastCoef = Math.Sqrt(firstVal - SecondVal);

            coef = SigmaZ / LastCoef;

            return coef;
        }

        #region Private Helpers

        private void BitmapToByteArray(Bitmap bmp)
        {
            Rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData bmpData = bmp.LockBits(Rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            IntPtr ptr = bmpData.Scan0;

            int bytes = bmpData.Stride * bmpData.Height;

            originalByteImage = new byte[bytes];
            grayScaledByteArray = new byte[bytes];
            originalGrayScaleImage = new byte[bytes];

            Marshal.Copy(ptr, originalByteImage, 0, bytes);

            bmp.UnlockBits(bmpData);

        }

        private Bitmap ByteArrayToBitmap(byte[] array)
        {
            var bmpData = Image.LockBits(
                Rect,
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            IntPtr int0 = bmpData.Scan0;

            Marshal.Copy(grayScaledByteArray, 0, int0, grayScaledByteArray.Length);

            Image.UnlockBits(bmpData);

            return Image;
        }

        #endregion
    }
}
