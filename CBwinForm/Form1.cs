using CBwinForm.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CBwinForm
{
    public partial class Form1 : Form
    {
        SimpleImageProcessor processedImage;

        public Form1()
        {
            InitializeComponent();

            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Interval = 50;
            chart1.Series[0].IsVisibleInLegend = false;

        }

        #region Open/Save

        private void OpenImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    processedImage = new SimpleImageProcessor(new Bitmap(openFile.FileName));
                    pictureBox1.Image = processedImage.Image;
                }
                catch
                {
                    MessageBox.Show("Error");
                }
            }
        }

        private void SaveImage_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG";

            ImageFormat format = ImageFormat.Png;

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(saveFile.FileName);

                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    default:
                        break;
                }

                pictureBox1.Image.Save(saveFile.FileName, format);
            }
            saveFile.Dispose();
        }

        private void OpenImage_ContextClick(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    processedImage = new SimpleImageProcessor(new Bitmap(openFile.FileName));
                    pictureBox1.Image = processedImage.Image;
                }
                catch
                {
                    MessageBox.Show("Error");
                }
            }
        }

        private void SaveImage_ContextClick(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG";

            ImageFormat format = ImageFormat.Png;

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(saveFile.FileName);

                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    default:
                        break;
                }

                pictureBox1.Image.Save(saveFile.FileName, format);
            }
            saveFile.Dispose();
        }

        #endregion

        private void GrayScaleFilter_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            if (pictureBox1.Image == null)
                throw new ArgumentNullException("Заполните первый pictureBox");

            pictureBox1.Image = processedImage.SetGrayScale();

            for (int i = 0; i < 256; i++)
                chart1.Series[0].Points.AddXY(i, processedImage.FrequencyX[i]);

            CoefNumeric.Value = CoefNumeric.Minimum = (int)processedImage.SigmaY;
            
        }

        private void ChangeBrightness_Click(object sender, EventArgs e)
        {
            //pictureBox1.Image = processedImage.SetNewBrightness();
        }

        private void CloseApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CoefNumeric_MouseUp(object sender, MouseEventArgs e)
        {
            var numValue = (int)((NumericUpDown)sender).Value;

            var newCoef = processedImage.CalculateNewCoef(numValue);

            pictureBox1.Image = processedImage.SetNewBrightness(newCoef);

            pictureBox2.Image = processedImage.GrayScaleImage;

        }
    }
}
