using CBwinForm.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CBwinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        ImageProcessor processedImage;

        private void OpenImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();


            openFile.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    processedImage = new ImageProcessor(new Bitmap(openFile.FileName));
                    pictureBox1.Image = processedImage.Image;
                }
                catch
                {
                    MessageBox.Show("Error");
                }
            }
        }

        private void GrayScaleFilter_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                throw new ArgumentNullException("Заполните первый pictureBox");

            pictureBox2.Image = processedImage.SetGrayScale();
        }

        private void ChangeBrightness_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = processedImage.ChangeBrightness();
        }

    }
}
