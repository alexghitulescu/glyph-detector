using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace GlyphTest
{
    public partial class Form1 : Form
    {
        Processor processor;

        public Form1()
        {
            InitializeComponent();

            processor = new Processor();
            processor.PostAction += new PostProcess(onProcess);

            initVideo();
        }

        private void initVideo()
        {
            // enumerate video devices
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            // create video source
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            // set NewFrame event handler
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            // start the video source
            videoSource.Start();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // get new frame
            Bitmap bitmap = eventArgs.Frame;

            Bitmap imediateDraw = new Bitmap(bitmap, new Size(pictureBox2.Width, pictureBox2.Height));

            pictureBox2.Invoke((Action)delegate()
            {
                if (pictureBox2.Image != null)
                {
                    pictureBox2.Image.Dispose();
                }

                pictureBox2.Image = imediateDraw;
            });

            Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(bitmap, new Point(0, 0));
            }

            processor.Image = clone;

            bitmap.Dispose();
        }

        private void onProcess(Bitmap image)
        {
            pictureBox1.Invoke((Action)delegate()
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                pictureBox1.Image = (Bitmap) image.Clone();
            });
        }
    }
}
