using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCamLib;

namespace Adolfo_Basic_Image_Processing
{
    public partial class Form2 : Form
    {
        private Timer webcamTimer;
        private bool isWebcamActive = false;
        private int currentFilter = 0; // 0: copy, 1: grayscale, 2: invert, 3: sepia, 4: histogram...
        private Bitmap webcamFrame;
        private WebCamLib.Device webcam;

        public Form2()
        {
            InitializeComponent();
        }

        // Used by Form1 to pass the chosen image
        public void SetImage(Image image)
        {
            pictureBox1.Image = image;
            pictureBox2.Image = null;
        }

        // ---------------- FILTER HELPERS ----------------
        private Bitmap ApplyGrayscale(Bitmap b)
        {
            Bitmap newB = new Bitmap(b.Width, b.Height);
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
            return newB;
        }

        private Bitmap ApplyInvert(Bitmap b)
        {
            Bitmap newB = new Bitmap(b.Width, b.Height);
            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    Color pixelColor = b.GetPixel(x, y);
                    Color inverted = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
                    newB.SetPixel(x, y, inverted);
                }
            }
            return newB;
        }

        private Bitmap ApplySepia(Bitmap b)
        {
            Bitmap newB = new Bitmap(b.Width, b.Height);
            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    Color pixelColor = b.GetPixel(x, y);
                    int tr = (int)(0.393 * pixelColor.R + 0.769 * pixelColor.G + 0.189 * pixelColor.B);
                    int tg = (int)(0.349 * pixelColor.R + 0.686 * pixelColor.G + 0.168 * pixelColor.B);
                    int tb = (int)(0.272 * pixelColor.R + 0.534 * pixelColor.G + 0.131 * pixelColor.B);
                    newB.SetPixel(x, y, Color.FromArgb(
                        Math.Min(255, tr),
                        Math.Min(255, tg),
                        Math.Min(255, tb)
                    ));
                }
            }
            return newB;
        }

        private Bitmap ApplyHistogram(Bitmap b)
        {
            int[] histogram = new int[256];
            for (int y = 0; y < b.Height; y++)
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

            for (int x = 0; x < histWidth; x++)
            {
                int histValue = histogram[x];
                int histBarHeight = (int)((histValue / (float)max) * histHeight);
                for (int y = histHeight - 1; y >= histHeight - histBarHeight; y--)
                {
                    histImage.SetPixel(x, y, Color.Black);
                }
            }
            return histImage;
        }

        private Bitmap ApplyFilter(Bitmap b, int filterType)
        {
            switch (filterType)
            {
                case 1: return ApplyGrayscale(b);
                case 2: return ApplyInvert(b);
                case 3: return ApplySepia(b);
                case 4: return ApplyHistogram(b);
                case 5: return ApplyConvolution(b, GaussianKernel);
                case 6: return ApplyConvolution(b, SharpenKernel);
                case 7: return ApplyConvolution(b, EdgeKernel);
                case 8: return ApplyConvolution(b, SmoothKernel);
                case 9: return ApplyConvolution(b, MeanRemovalKernel);
                case 10: return ApplyConvolution(b, EmbossKernel);
                default: return (Bitmap)b.Clone();
            }
        }


        // ---------------- FILTER BUTTONS ----------------
        private void button2_Click(object sender, EventArgs e) => ApplyAndShow(0); // Copy
        private void button3_Click(object sender, EventArgs e) => ApplyAndShow(1); // Grayscale
        private void button4_Click(object sender, EventArgs e) => ApplyAndShow(2); // Invert
        private void button6_Click(object sender, EventArgs e) => ApplyAndShow(3); // Sepia
        private void button5_Click(object sender, EventArgs e) => ApplyAndShow(4); // Histogram
        private void button9_Click(object sender, EventArgs e) => ApplyAndShow(5); // Gaussian
        private void button10_Click(object sender, EventArgs e) => ApplyAndShow(6); // Sharpen
        private void button11_Click(object sender, EventArgs e) => ApplyAndShow(7); // Edge

        private void button12_Click(object sender, EventArgs e) => ApplyAndShow(8); // Smooth
        private void button13_Click(object sender, EventArgs e) => ApplyAndShow(9); // Mean Removal
        private void button14_Click(object sender, EventArgs e) => ApplyAndShow(10); // Emboss

        private void ApplyAndShow(int filterType)
        {
            if (pictureBox1.Image == null) return;
            Bitmap src = new Bitmap(pictureBox1.Image);
            pictureBox2.Image = ApplyFilter(src, filterType);
            currentFilter = filterType;
        }

        // ---------------- SAVE ----------------
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
                    var ext = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();
                    var format = System.Drawing.Imaging.ImageFormat.Png;
                    if (ext == ".jpg" || ext == ".jpeg") format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    else if (ext == ".bmp") format = System.Drawing.Imaging.ImageFormat.Bmp;

                    pictureBox2.Image.Save(saveDialog.FileName, format);
                    MessageBox.Show("Photo saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ---------------- BACK ----------------
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 mainForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (mainForm != null) mainForm.Show();
        }

        // ---------------- WEBCAM ----------------
        private void button8_Click(object sender, EventArgs e)
        {
            if (!isWebcamActive)
            {
                var devices = WebCamLib.DeviceManager.GetAllDevices();
                if (devices.Length == 0)
                {
                    MessageBox.Show("No webcam devices found!");
                    return;
                }

                webcam = devices[0];
                webcam.ShowWindow(pictureBox1);

                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

                StartWebcam();
                button8.Text = "📁 Switch to Upload Mode";
            }
            else
            {
                StopWebcam();
                using (OpenFileDialog openDialog = new OpenFileDialog())
                {
                    openDialog.Title = "Open Image";
                    openDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            Image img = Image.FromFile(openDialog.FileName);
                            pictureBox1.Image = img;
                            pictureBox2.Image = null;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error loading image: " + ex.Message);
                        }
                    }
                }

                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                button8.Text = "📷 Switch to Webcam";
            }
        }

        private void StartWebcam()
        {
            if (webcamTimer == null)
            {
                webcamTimer = new Timer();
                webcamTimer.Interval = 100;
                webcamTimer.Tick += WebcamTimer_Tick;
            }
            isWebcamActive = true;
            webcamTimer.Start();
        }

        private void StopWebcam()
        {
            isWebcamActive = false;
            if (webcamTimer != null) webcamTimer.Stop();
            if (webcam != null)
            {
                webcam.Stop();
                webcam = null;
            }
        }

        private void WebcamTimer_Tick(object sender, EventArgs e)
        {
            if (webcam == null) return;
            webcam.Sendmessage();
            IDataObject data = Clipboard.GetDataObject();
            if (data == null || !data.GetDataPresent("System.Drawing.Bitmap")) return;

            Bitmap bmp = data.GetData("System.Drawing.Bitmap", true) as Bitmap;
            if (bmp == null) return;

            webcamFrame = (Bitmap)bmp.Clone();
            pictureBox1.Image = webcamFrame;
            pictureBox2.Image = ApplyFilter(webcamFrame, currentFilter);
        }

        // ---------------- CONVOLUTION KERNELS ----------------
        private readonly double[,] GaussianKernel = new double[,]
        {
            { 1, 2, 1 },
            { 2, 4, 2 },
            { 1, 2, 1 }
        };

        private readonly double[,] SharpenKernel = new double[,]
       {
            {  0, -1,  0 },
            { -1,  5, -1 },
            {  0, -1,  0 }
       };

        private readonly double[,] EdgeKernel = new double[,]
        {
            { -1, -1, -1 },
            { -1,  8, -1 },
            { -1, -1, -1 }
        };

        // ---------------- EXTRA CONVOLUTION KERNELS ----------------
        private readonly double[,] SmoothKernel = new double[,]
        {
             { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        private readonly double[,] MeanRemovalKernel = new double[,]
        {
            { -1, -1, -1 },
            { -1,  9, -1 },
            { -1, -1, -1 }
        };

        private readonly double[,] EmbossKernel = new double[,]
        {
            { -1, -1,  0 },
            { -1,  0,  1 },
            {  0,  1,  1 }
        };


        private Bitmap ApplyConvolution(Bitmap src, double[,] kernel)
        {
            int w = src.Width, h = src.Height;
            Bitmap output = new Bitmap(w, h);
            int k = kernel.GetLength(0) / 2;
            double sum = kernel.Cast<double>().Sum();
            if (sum == 0) sum = 1;

            for (int y = k; y < h - k; y++)
            {
                for (int x = k; x < w - k; x++)
                {
                    double r = 0, g = 0, b = 0;
                    for (int ky = -k; ky <= k; ky++)
                        for (int kx = -k; kx <= k; kx++)
                        {
                            Color c = src.GetPixel(x + kx, y + ky);
                            double weight = kernel[ky + k, kx + k];
                            r += c.R * weight;
                            g += c.G * weight;
                            b += c.B * weight;
                        }

                    int rr = Math.Min(255, Math.Max(0, (int)(r / sum)));
                    int gg = Math.Min(255, Math.Max(0, (int)(g / sum)));
                    int bb = Math.Min(255, Math.Max(0, (int)(b / sum)));
                    output.SetPixel(x, y, Color.FromArgb(rr, gg, bb));
                }
            }
            return output;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
