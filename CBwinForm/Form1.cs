using CBwinForm.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
            //chart1.ChartAreas[0].AxisX.Interval = 15;
            chart1.Series[0].IsVisibleInLegend = false;

        }

        #region Open/Save
        
        private void OpenImage_ContextClick(object sender, EventArgs e)
        {
            if(processedImage == null)
            {
                OpenFileDialog openFile = new OpenFileDialog();

                openFile.Filter = "Image Files(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        processedImage = new SimpleImageProcessor(new Bitmap(openFile.FileName));
                        pictureBox1.Image = processedImage.Image;
                        var test = SimpleImageProcessor.BitmapToColorByteImage(new Bitmap(pictureBox1.Image));
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

                openFile.Filter = "Image Files(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*";

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
            saveFile.Filter = "Image Files(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png";
            //saveFile.FileName = "сигма=" + CoefNumeric.Value + ", к=" + label1.Text.Substring(0, 8);

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

                label1.Text = processedImage.coef.ToString();
            }

        }

        private void exportChartAsImage_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPeg Image|*.jpg";
            saveFileDialog.Title = "Save Chart As Image File";
            saveFileDialog.FileName = "k=" + label1.Text.Substring(0,8);

            DialogResult result = saveFileDialog.ShowDialog();
            saveFileDialog.RestoreDirectory = true;

            if (result == DialogResult.OK && saveFileDialog.FileName != "")
            {
                try
                {
                    if (saveFileDialog.CheckPathExists)
                    {
                        if (saveFileDialog.FilterIndex == 2)
                        {
                            chart1.SaveImage(saveFileDialog.FileName, ChartImageFormat.Jpeg);
                        }
                        else if (saveFileDialog.FilterIndex == 1)
                        {
                            chart1.SaveImage(saveFileDialog.FileName, ChartImageFormat.Png);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Given Path does not exist");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void returnOriginalImageOnBitmap_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = processedImage.SetOriginalImage();

                chart1.Series[0].Points.Clear();

                label1.Text = "0";
            }
        }

        private void CloseApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
