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
    public partial class Form2 : Form
    {
        Bitmap b;
        Bitmap newB;
        public Form2()
        {
            InitializeComponent();
        }

        public void SetImage(Image image)
        {
            pictureBox1.Image = image;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            b = new Bitmap(pictureBox1.Image);
            newB = new Bitmap(b.Width, b.Height);
            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    Color pixelColor = b.GetPixel(x, y);
                    newB.SetPixel(x, y, pixelColor);
                }
            }
            pictureBox2.Image = newB;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            b = new Bitmap(pictureBox1.Image);
            newB = new Bitmap(b.Width, b.Height);

            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    Color pixelColor = b.GetPixel(x, y);
                    int grey = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                    Color greyColor = Color.FromArgb(grey, grey, grey);
                    newB.SetPixel(x, y, greyColor);
                }
            }

            pictureBox2.Image = newB;
        }

        public static Color InvertColor(Color color)
        {
            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            b = new Bitmap(pictureBox1.Image);
            newB = new Bitmap(b.Width, b.Height);

            for(int x = 0; x < b.Width; x++)
            {
                for(int y = 0; y < b.Height; y++)
                {
                    Color pixelColor = b.GetPixel(x, y);
                    Color invertedColor = InvertColor(pixelColor);
                    newB.SetPixel(x, y, invertedColor);
                }
            }
                pictureBox2.Image = newB;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            b = new Bitmap(pictureBox1.Image);
            int[]histogram = new int[256];

            for(int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    Color pixelColor = b.GetPixel(x, y);
                    int grey = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                    histogram[grey]++;
                }
            }

            int histHeight = 100;
            int histWidth = 256;
            Bitmap histImage = new Bitmap(histWidth, histHeight);
            int max = histogram.Max();
            for(int x = 0; x < histWidth; x++)
            {
                int histValue = histogram[x];
                int histBarHeight = (int)((histValue / (float)max) * histHeight);
                for(int y = histHeight - 1; y >= histHeight - histBarHeight; y--)
                {
                    histImage.SetPixel(x, y, Color.Black);
                }
            }

            pictureBox2.Image = histImage;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            b = new Bitmap(pictureBox1.Image);
            newB = new Bitmap(b.Width, b.Height);

            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    Color pixelColor = b.GetPixel(x, y);
                    int tr = (int)(0.393 * pixelColor.R + 0.769 * pixelColor.G + 0.189 * pixelColor.B);
                    int tg = (int)(0.349 * pixelColor.R + 0.686 * pixelColor.G + 0.168 * pixelColor.B);
                    int tb = (int)(0.272 * pixelColor.R + 0.534 * pixelColor.G + 0.131 * pixelColor.B);
                    tr = Math.Min(255, tr);
                    tg = Math.Min(255, tg);
                    tb = Math.Min(255, tb);
                    Color sepiaColor = Color.FromArgb(tr, tg, tb);
                    newB.SetPixel(x, y, sepiaColor);
                }
            }
            pictureBox2.Image = newB;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("There is no processed image to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Save Processed Photo";
                saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                saveDialog.DefaultExt = "png";
                saveDialog.FileName = "ProcessedPhoto";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Determine the format based on extension
                        var ext = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                        if (ext == ".jpg" || ext == ".jpeg") format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (ext == ".bmp") format = System.Drawing.Imaging.ImageFormat.Bmp;

                        pictureBox2.Image.Save(saveDialog.FileName, format);
                        MessageBox.Show("Photo saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving photo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 mainForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (mainForm != null)
            {
                mainForm.Show();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
