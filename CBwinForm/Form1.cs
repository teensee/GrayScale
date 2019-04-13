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
            chart1.ChartAreas[0].AxisX.Interval = 25;
            chart1.Series[0].IsVisibleInLegend = false;

        }

        #region Open/Save
        
        private void OpenImage_ContextClick(object sender, EventArgs e)
        {
            if(processedImage == null)
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
            else
            {
                processedImage = null;
                pictureBox1.Image = null;
                CoefNumeric.Value = CoefNumeric.Minimum;
                chart1.Series[0].Points.Clear();

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
            if (pictureBox1.Image == null)
                throw new ArgumentNullException("Заполните первый pictureBox");

            pictureBox1.Image = processedImage.SetGrayScale();

            chart1.Series[0].Points.Clear();

            for (int i = 0; i < 256; i++)
                chart1.Series[0].Points.AddXY(i, processedImage.FrequencyX[i]);

            CoefNumeric.Value = CoefNumeric.Minimum = (int)processedImage.SigmaY;
            
        }

        private void CoefNumeric_MouseUp(object sender, MouseEventArgs e)
        {
            if(pictureBox1.Image != null)
            {
                //Забираем значение из нумерика
                var numValue = (int)((NumericUpDown)sender).Value;

                //Считаем новый кэф
                var newCoef = processedImage.CalculateNewCoef(numValue);

                pictureBox1.Image = processedImage.SetNewBrightness(newCoef);

                chart1.Series[0].Points.Clear();

                for (int i = 0; i < 256; i++)
                    chart1.Series[0].Points.AddXY(i, processedImage.FrequencyX[i]);
            }

        }

        private void CloseApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
