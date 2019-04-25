using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CBwinForm.DataModels
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorBytePixel
    {
        public byte b, g, r, a;
    }

    public class ColorByteImage
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public readonly ColorBytePixel[] rawdata;

        public ColorByteImage(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            rawdata = new ColorBytePixel[Width * Height];
        }

        public ColorByteImage(Rectangle rect)
        {
            this.Width = rect.Width;
            this.Height = rect.Height;
            rawdata = new ColorBytePixel[Width * Height];
        }

        public ColorBytePixel this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    throw new IndexOutOfRangeException(string.Format("Trying to access pixel ({0}, {1}) in {2}x{3} image", x, y, Width, Height));
                return rawdata[y * Width + x];
            }
            set
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    throw new IndexOutOfRangeException(string.Format("Trying to access pixel ({0}, {1}) in {2}x{3} image", x, y, Width, Height));
                rawdata[y * Width + x] = value;
            }
        }
    }
}
