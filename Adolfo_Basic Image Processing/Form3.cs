using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Adolfo_Basic_Image_Processing
{
    public partial class Form3 : Form
    {
        Bitmap imageB, imageA, colorgreen;

        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            pictureBox1.Image = imageB;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            pictureBox2.Image = imageA;
        }

        private void openFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {
            imageB = new Bitmap(openFileDialog1.FileName);
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            imageA = new Bitmap(openFileDialog2.FileName);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 mainForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (mainForm != null)
            {
                mainForm.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Color mygreen = Color.FromArgb(0, 255, 0);
            int greygreen = (mygreen.R + mygreen.G + mygreen.B) / 3;
            int threshold = 150;
            int margin = 50;

            colorgreen = new Bitmap(imageB.Width, imageB.Height);
            Bitmap resizedBackground = new Bitmap(imageA, new Size(imageB.Width, imageB.Height));

            for (int x = 0; x < imageB.Width; x++)
            {
                for (int y = 0; y < imageB.Height; y++)
                {

                    Color pixelColor = imageB.GetPixel(x, y);
                    Color backpixel = resizedBackground.GetPixel(x, y);
                    if (pixelColor.G > threshold && pixelColor.G > pixelColor.R + margin && pixelColor.G > pixelColor.B + margin)
                    {
                        colorgreen.SetPixel(x, y, backpixel);
                    }
                    else
                    {
                        colorgreen.SetPixel(x, y, pixelColor);
                    }
                }
            }
            pictureBox3.Image = colorgreen;
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
