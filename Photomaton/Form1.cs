using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Photomaton
{
    public partial class Form1 : Form
    {
        private VideoCaptureDevice videoSource;
        private FilterInfoCollection videoDevices;
        private Bitmap previousFrame;

        public Form1()
        {
            InitializeComponent();

            // Initialize webcam
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.VideoResolution = videoSource.VideoCapabilities[16];
                videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
                videoSource.Start();
            }
            else
            {
                MessageBox.Show("Aucune webcam trouvée!");
            }

            takePictureButton.Click += TakePictureButton_Click;
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Update the previewPictureBox with the current webcam frame
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

            // Dispose the previous frame
            previewPictureBox.Image?.Dispose();

            // Update the previewPictureBox
            previewPictureBox.Image = bitmap;
        }

        private void TakePictureButton_Click(object sender, EventArgs e)
        {
            // Stop the webcam
            videoSource.SignalToStop();
            videoSource.WaitForStop();

            // Display a popup asking the user if they want to keep the picture or take a new one
            DialogResult result = MessageBox.Show("Voulez-vous garder cette photo ?", "Photomaton", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                // Save the picture to the user's "Pictures" folder
                string picturesFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                string fileName = $"{DateTime.Now:yyyyMMddHHmmss}.png"; // Unique filename based on timestamp

                // Save the picture with a unique filename in the "Pictures" folder
                string filePath = System.IO.Path.Combine(picturesFolderPath, fileName);
                previewPictureBox.Image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                MessageBox.Show($"Photo enregistrée avec succès:\n{filePath}");
            }

            // Restart the webcam for a new session
            videoSource.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop the webcam if it is running
            if (videoSource != null && videoSource.IsRunning)
            {
                // Unsubscribe from the NewFrame event before stopping the webcam
                videoSource.NewFrame -= new NewFrameEventHandler(VideoSource_NewFrame);

                // Stop the webcam and wait for it to stop
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource = null;  // Set to null to ensure it is properly disposed
            }

            // Dispose of the previous frame bitmap
            previousFrame?.Dispose();
        }
    }
}