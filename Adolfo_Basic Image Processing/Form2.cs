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
        private int currentFilter = 0; // 0: copy, 1: grayscale, 2: invert, 3: sepia, 4: histogram
        private Bitmap webcamFrame;
        private WebCamLib.Device webcam;
        private Image bmap;

        public Form2()
        {
            InitializeComponent();
        }

        public void SetImage(Image image)
        {
            pictureBox1.Image = image;
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
                    tr = Math.Min(255, tr);
                    tg = Math.Min(255, tg);
                    tb = Math.Min(255, tb);
                    newB.SetPixel(x, y, Color.FromArgb(tr, tg, tb));
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
                default: return (Bitmap)b.Clone(); // copy
            }
        }

        // ---------------- BUTTONS ----------------
        private void button2_Click(object sender, EventArgs e) => ApplyAndShow(0); // copy
        private void button3_Click(object sender, EventArgs e) => ApplyAndShow(1); // grayscale
        private void button4_Click(object sender, EventArgs e) => ApplyAndShow(2); // invert
        private void button6_Click(object sender, EventArgs e) => ApplyAndShow(3); // sepia
        private void button5_Click(object sender, EventArgs e) => ApplyAndShow(4); // histogram

        private void ApplyAndShow(int filterType)
        {
            if (pictureBox1.Image == null) return;
            Bitmap src = new Bitmap(pictureBox1.Image);
            pictureBox2.Image = ApplyFilter(src, filterType);
            currentFilter = filterType; // update filter for live webcam
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
            if (!isWebcamActive) // Webcam is OFF -> Start webcam
            {
                var devices = WebCamLib.DeviceManager.GetAllDevices();
                if (devices.Length == 0)
                {
                    MessageBox.Show("No webcam devices found!");
                    return;
                }

                webcam = devices[0];
                webcam.ShowWindow(pictureBox1); // show live preview

                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

                StartWebcam();

                button8.Text = "Switch to Uploading Files";
            }
            else // Webcam is ON -> Stop webcam and open file dialog
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
                            pictureBox2.Image = null; // clear processed image until user applies a filter
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error loading image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }


                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

                button8.Text = "Switch to Webcam";

            }
        }


        private void StartWebcam()
        {
            if (webcamTimer == null)
            {
                webcamTimer = new Timer();
                webcamTimer.Interval = 100; // ~10 FPS
                webcamTimer.Tick += WebcamTimer_Tick;

            }
            isWebcamActive = true;
            webcamTimer.Start();
        }

        private void StopWebcam()
        {
            isWebcamActive = false;

            if (webcamTimer != null)
                webcamTimer.Stop();

            if (webcam != null)
            {
                webcam.Stop();
                webcam = null;
            }
        }


        private void WebcamTimer_Tick(object sender, EventArgs e)
        {
            if (webcam == null) return;

            // Ask webcam to copy frame into clipboard
            webcam.Sendmessage();

            IDataObject data = Clipboard.GetDataObject();
            if (data == null || !data.GetDataPresent("System.Drawing.Bitmap"))
                return;

            Bitmap bmp = data.GetData("System.Drawing.Bitmap", true) as Bitmap;
            if (bmp == null) return;

            webcamFrame = (Bitmap)bmp.Clone(); // safe copy
            pictureBox1.Image = webcamFrame;   // raw feed
            pictureBox2.Image = ApplyFilter(webcamFrame, currentFilter); // filtered feed
        }


        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
