using CBwinForm.DataModels;
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
        /// Тоже разрешение пикчи,но храним в <see cref="Rectangle"/> 
        /// для дальнейшего использования в <see cref="BitmapData"/>
        /// </summary>
        public Rectangle Rect { get; set; }

        /// <summary>
        /// Разрешение пикчи
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

        /// <summary>
        /// Массив с "оригинальным" цветным изображением
        /// </summary>
        private byte[] originalByteImage;

        /// <summary>
        /// Массив хранящий "оригинальную" чб картинку
        /// </summary>
        private byte[] originalGrayScaleImage;

        /// <summary>
        /// Массив с которым работает прога
        /// </summary>
        private byte[] grayScaledByteArray;

        /// <summary>
        /// вычисляемое значение для чб R*0.299 + G*0.587 + B*0.114 
        /// </summary>
        private double val;

        /// <summary>
        /// Store converted <see cref="val"/> in byte 
        /// </summary>
        private byte color_b;

        private double sigmaY;

        public double coef { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="image"></param>
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
            //Занимаемся чисткой
            Array.Clear(FrequencyX, 0, FrequencyX.Length);

            for (int i = 0; i < originalByteImage.Length; i += 3)
            {
                val = originalByteImage[i] * 0.299 + originalByteImage[i + 1] * 0.587 + originalByteImage[i + 2] * 0.114;
                color_b = 0;

                color_b = Convert.ToByte(val);

                //Распределяемся по ячейкам,Господа! 
                FrequencyX[color_b]++;

                grayScaledByteArray[i] = grayScaledByteArray[i + 1] = grayScaledByteArray[i + 2] = color_b;
                originalGrayScaleImage[i] = originalGrayScaleImage[i + 1] = originalGrayScaleImage[i + 2] = color_b;
            }

            CalculateSigmaY();

            return ByteArrayToBitmap(grayScaledByteArray);
        }

        /// <summary>
        /// Считаем дисперсию или
        /// SigmaY = sqrt[(y^2) - (y)^2]
        /// [] - скобки заменяют обычные в математике ()
        /// а () показывают что это "среднее" из-за отсутсвия возможности вставки формулой
        /// </summary>
        public void CalculateSigmaY()
        {
            //Дропаем оба значения в ноль :)
            double yFirst = 0.0f;
            double ySecond = 0.0f;

            //double test = 0.0f;
            for (int i = 0; i < FrequencyX.Length; i++)
            {
                //Распределение пикселей
                //Значение из массива Freq делится на разрешение
                ColorAllocation[i] = FrequencyX[i] / (double)Resolution;

                //Для первого считается так: m[x^2] = 0^2*p0 + 1^2*p1 + 2^2*p2 ... n^2*pN;
                yFirst += i * i * ColorAllocation[i];

                //Второй y считается: m^2[x] = (0*p0 + 1*p1 + ... n*pN)^2
                ySecond += i * ColorAllocation[i];

                //Коммент на удаление
                //test += ColorAllocation[i];
                //Эта переменная нужна для того,чтобы убедиться что расчет идет +- верно
                //В моем случае погрешность вроде бы не большая,но нужно будет убедиться в последствии 
                //test == 1, у меня 1.0000000000000007
            }

            ySecond *= ySecond;

            SigmaY = Math.Sqrt(yFirst - ySecond);
        }

        #endregion

        #region Set new brightness

        public Bitmap SetNewBrightness(double cf)
        {
            //Копируем "оригинальное" чб во временный массив
            Array.Copy(originalGrayScaleImage, grayScaledByteArray, grayScaledByteArray.Length);

            //Очищаем массив с частотой пикселей
            Array.Clear(FrequencyX, 0, FrequencyX.Length);

            //Обнуляем значение яркости
            byte newbrightness = 0;

            //Поехали..
            for (int i = 0; i < grayScaledByteArray.Length; i += 3)
            {
                //z = k*y^2
                var z = cf * Math.Pow(grayScaledByteArray[i], 2);

                //Если z > 255,то..
                if (z > 255) z = 255;

                //Приводим к типу байт
                newbrightness = Convert.ToByte(z);

                //Записываем значение в массив частот
                FrequencyX[newbrightness]++;

                //Обновляем 3 канала
                grayScaledByteArray[i] = grayScaledByteArray[i + 1] = grayScaledByteArray[i + 2] = newbrightness;
            }

            //Возвращаем результат
            return ByteArrayToBitmap(grayScaledByteArray);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ScrollBarSelectedValue"></param>
        /// <returns></returns>
        public double CalculateNewCoef(int ScrollBarSelectedValue)
        {
            double SigmaZ = ScrollBarSelectedValue;

            double yFirst = 0.0f;
            double ySecond = 0.0f;

            for (int i = 0; i < FrequencyX.Length; i++)
            {
                yFirst += Math.Pow(i, 4) * ColorAllocation[i];
                ySecond += Math.Pow(i, 2) * ColorAllocation[i];
            }

            ySecond *= ySecond;

            var despersia = Math.Sqrt(yFirst - ySecond);

            coef = SigmaZ / despersia;

            return coef;
        }

        #endregion 

        public Bitmap SetOriginalImage()
        {
            return ByteArrayToBitmap(originalByteImage);
        }

        #region Private Helpers

        private void BitmapToByteArray(Bitmap bmp)
        {
            Rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData bmpData = bmp.LockBits(Rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Rect.Width * bmpData.Height * 3;

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

            Marshal.Copy(array, 0, int0, array.Length);

            Image.UnlockBits(bmpData);

            return Image;
        }

        #endregion

        public static unsafe ColorByteImage BitmapToColorByteImage(Bitmap B)
        {
            int W = B.Width, H = B.Height;
            ColorByteImage res = new ColorByteImage(W, H);

            if (B.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                Color[] pi = B.Palette.Entries;
                byte[] pal = new byte[1024];
                for (int i = 0; i < pi.Length; i++)
                {
                    Color C = pi[i];
                    pal[i * 4] = C.B;
                    pal[i * 4 + 1] = C.G;
                    pal[i * 4 + 2] = C.R;
                    pal[i * 4 + 3] = C.A;
                }

                LockBitmapInfo lbi = LockBitmap(B, PixelFormat.Format8bppIndexed, 1);
                
                try
                {
                    for (int j = 0; j < H; j++)
                        for (int i = 0; i < W; i++)
                        {
                            int c = lbi.data[lbi.linewidth * j + i];
                            byte b = pal[c * 4];
                            byte g = pal[c * 4 + 1];
                            byte r = pal[c * 4 + 2];
                            res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
                        }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }
            else
            {
                LockBitmapInfo lbi = LockBitmap(B);
                try
                {
                    for (int j = 0; j < H; j++)
                        for (int i = 0; i < W; i++)
                        {
                            byte b = lbi.data[lbi.linewidth * j + i * 4];
                            byte g = lbi.data[lbi.linewidth * j + i * 4 + 1];
                            byte r = lbi.data[lbi.linewidth * j + i * 4 + 2];
                            res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
                        }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }

            return res;
        }

        public static unsafe GrayscaleByteImage BitmapToGrayscaleByteImage(Bitmap B)
        {
            int W = B.Width, H = B.Height;
            GrayscaleByteImage res = new GrayscaleByteImage(W, H);

            if (B.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                Color[] pi = B.Palette.Entries;
                byte[] pal = new byte[1024];
                for (int i = 0; i < pi.Length; i++)
                {
                    Color C = pi[i];
                    pal[i * 4] = C.B;
                    pal[i * 4 + 1] = C.G;
                    pal[i * 4 + 2] = C.R;
                    pal[i * 4 + 3] = C.A;
                }

                LockBitmapInfo lbi = LockBitmap(B, PixelFormat.Format8bppIndexed, 1);
                try
                {
                    for (int j = 0; j < H; j++)
                        for (int i = 0; i < W; i++)
                        {
                            int c = lbi.data[lbi.linewidth * j + i];
                            int b = pal[c * 4];
                            int g = pal[c * 4 + 1];
                            int r = pal[c * 4 + 2];
                            res[i, j] = (byte)(0.114f * b + 0.587f * g + 0.299f * r);
                        }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }
            else
            {
                LockBitmapInfo lbi = LockBitmap(B);
                try
                {
                    for (int j = 0; j < H; j++)
                        for (int i = 0; i < W; i++)
                        {
                            int b = lbi.data[lbi.linewidth * j + i * 4];
                            int g = lbi.data[lbi.linewidth * j + i * 4 + 1];
                            int r = lbi.data[lbi.linewidth * j + i * 4 + 2];
                            res[i, j] = (byte)(0.114f * b + 0.587f * g + 0.299f * r);
                        }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }

            return res;
        }

        public static unsafe void UnlockBitmap(LockBitmapInfo lbi)
        {
            lbi.B.UnlockBits(lbi.bitmapData);
            lbi.bitmapData = null;
            lbi.data = null;
        }

        public unsafe struct LockBitmapInfo
        {
            public Bitmap B;
            public int linewidth;
            public BitmapData bitmapData;
            public byte* data;
            public int Width, Height;
        }

        public static LockBitmapInfo LockBitmap(Bitmap B)
        {
            return LockBitmap(B, PixelFormat.Format32bppRgb, 4);
        }

        public static LockBitmapInfo LockBitmapWithAlpha(Bitmap B)
        {
            return LockBitmap(B, PixelFormat.Format32bppArgb, 4);
        }

        public static unsafe LockBitmapInfo LockBitmap(Bitmap B, PixelFormat pf, int pixelsize)
        {
            LockBitmapInfo lbi;
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = B.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.X,
                (int)boundsF.Y,
                (int)boundsF.Width,
                (int)boundsF.Height);
            lbi.B = B;
            lbi.Width = (int)boundsF.Width;
            lbi.Height = (int)boundsF.Height;
            lbi.bitmapData = B.LockBits(bounds, ImageLockMode.ReadWrite, pf);
            lbi.linewidth = lbi.bitmapData.Stride;
            lbi.data = (byte*)(lbi.bitmapData.Scan0.ToPointer());
            return lbi;
        }

        public static unsafe Bitmap ImageToBitmap(GrayscaleByteImage image)
        {
            Bitmap B = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);

            LockBitmapInfo lbi = LockBitmap(B);
            try
            {
                for (int j = 0; j < image.Height; j++)
                    for (int i = 0; i < image.Width; i++)
                    {
                        byte c = image[i, j];
                        lbi.data[lbi.linewidth * j + i * 4] = c;
                        lbi.data[lbi.linewidth * j + i * 4 + 1] = c;
                        lbi.data[lbi.linewidth * j + i * 4 + 2] = c;
                    }
            }
            finally
            {
                UnlockBitmap(lbi);
            }

            return B;

        }
    }
}
