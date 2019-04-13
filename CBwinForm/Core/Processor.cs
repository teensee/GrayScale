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

        private double coef;

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

            for (int i = 0; i < originalByteImage.Length -1; i += 3)
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

            SigmaY = Math.Sqrt((yFirst - ySecond));
        }

        #endregion

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
                var z = Math.Pow(grayScaledByteArray[i], 2) * cf;

                //Если z > 255,то..
                if (z > 255) z = 255;

                //Приводим к типу байт
                newbrightness = Convert.ToByte(z);

                //Записываем значение в массив частот
                FrequencyX[newbrightness]++;

                //Обновляем 3 канала
                grayScaledByteArray[i] = grayScaledByteArray[i + 1] = grayScaledByteArray[i + 2] = newbrightness;
            }

            //Ретерн:3
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

            Marshal.Copy(grayScaledByteArray, 0, int0, grayScaledByteArray.Length);

            Image.UnlockBits(bmpData);

            return Image;
        }

        #endregion
    }
}
