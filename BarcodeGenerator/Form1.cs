using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ZXing;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Collections.Generic;

namespace BarcodeGenerator
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Timer scanTimer;

        public Form1()
        {
            InitializeComponent();
            LoadCameraDevices();
        }

        private void LoadCameraDevices()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            picBarcode.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Masukkan teks untuk barcode!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = picBarcode.Width,
                    Height = picBarcode.Height
                }
            };

            picBarcode.Image = barcodeWriter.Write(txtBarcode.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (picBarcode.Image == null)
            {
                MessageBox.Show("Generate barcode terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                Title = "Simpan Barcode"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                picBarcode.Image.Save(saveDialog.FileName, ImageFormat.Png);
                MessageBox.Show("Barcode berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "PNG Image|*.png",
                Title = "Pilih Barcode"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                picBarcode.Image = Image.FromFile(openDialog.FileName);
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (picBarcode.Image == null)
            {
                MessageBox.Show("Load atau ambil gambar barcode terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode((Bitmap)picBarcode.Image);

            if (result != null)
            {
                txtBarcode.Text = result.Text;
                MessageBox.Show("Barcode berhasil dipindai!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Barcode tidak dikenali!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStartCamera_Click(object sender, EventArgs e)
        {
            if (videoSource != null)
            {
                videoSource.Start();
                scanTimer = new Timer();
                scanTimer.Interval = 1000;
                scanTimer.Tick += ScanTimer_Tick;
                scanTimer.Start();
            }
            else
            {
                MessageBox.Show("Kamera tidak ditemukan!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            if (picBarcode.Image != null)
            {
                BarcodeReader barcodeReader = new BarcodeReader
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options = new ZXing.Common.DecodingOptions
                    {
                        TryHarder = true, // Meningkatkan sensitivitas pemindaian
                        PossibleFormats = new List<BarcodeFormat>
            {
                BarcodeFormat.CODE_128,
                BarcodeFormat.CODE_39,
                BarcodeFormat.CODE_93,
                BarcodeFormat.ITF,
                BarcodeFormat.EAN_13,
                BarcodeFormat.EAN_8,
                BarcodeFormat.UPC_A,
                BarcodeFormat.UPC_E,
                BarcodeFormat.QR_CODE
            }
                    }
                };
                Result result = barcodeReader.Decode((Bitmap)picBarcode.Image);

                if (result != null)
                {
                    txtBarcode.Text = result.Text;
                    scanTimer.Stop();
                    videoSource.SignalToStop();
                    MessageBox.Show("Barcode berhasil dipindai!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBarcode.Clear(); // Menghapus teks di TextBox
            picBarcode.Image = null; // Menghapus gambar barcode dari PictureBox
        }
    }
}
