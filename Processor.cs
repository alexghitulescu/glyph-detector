using AForge;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Vision.GlyphRecognition;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace GlyphTest
{
    public delegate void PostProcess(Bitmap bitmap);

    class Processor
    {
        private GlyphDatabase database;
        private GlyphRecognizer recogniser;

        private BaseResizeFilter resizeFilter;
        private Sharpen sharpenFilter;

        private Object lockObj = new Object();
        private Bitmap image;

        private bool running = true;
        private AutoResetEvent autoEvent;

        public Bitmap Image
        {
            private get
            {
                lock (lockObj)
                {
                    return image;
                }
            }

            set
            {
                lock (lockObj)
                {
                    image = value;
                }

                autoEvent.Set();
            }
        }

        public PostProcess PostAction;

        public Processor()
        {
            initGlyphs();

            // resizeFilter = new ResizeNearestNeighbor(1920, 1080);
            // resizeFilter = new ResizeBicubic(1920, 1080);
            resizeFilter = new ResizeBilinear(1920, 1080);
            sharpenFilter = new Sharpen();

            autoEvent = new AutoResetEvent(false);

            Thread thread = new Thread(process);
            thread.Start();
        }

        private void initGlyphs()
        {
            database = new GlyphDatabase(5);

            Glyph glyph1 = new Glyph("1    ", new byte[,] { {0, 0, 0, 0, 0},
                                                            {0, 0, 1, 1, 0},
                                                            {0, 1, 0, 1, 0},
                                                            {0, 0, 1, 0, 0},
                                                            {0, 0, 0, 0, 0} });

            Glyph glyph2 = new Glyph("2    ", new byte[,] { {0, 0, 0, 0, 0},
                                                            {0, 1, 0, 1, 0},
                                                            {0, 1, 1, 1, 0},
                                                            {0, 0, 1, 0, 0},
                                                            {0, 0, 0, 0, 0} });
            database.Add(glyph1);
            database.Add(glyph2);

            recogniser = new GlyphRecognizer(database);
        }

        private void process()
        {
            while (running)
            {
                while (Image == null)
                {
                    if (!running)
                    {
                        return;
                    }
                    autoEvent.WaitOne();
                }

                Bitmap tempImg;
                lock (lockObj)
                {
                    tempImg = Image;
                    Image = null;
                }

                try
                {
                    var temp = tempImg.RawFormat;
                }
                catch(Exception)
                {
                    continue;
                }

                Console.WriteLine("width: " + tempImg.Width + " ; height: " + tempImg.Height);

                // Bitmap bigger = new Bitmap(tempImg, new Size(tempImg.Width, tempImg.Height));
                // Bitmap bigger = new Bitmap(tempImg, new Size(1920, 1080));
                Bitmap bigger = resizeFilter.Apply(tempImg);
                bigger = sharpenFilter.Apply(bigger);

                List<ExtractedGlyphData> glyphDataList = recogniser.FindGlyphs(bigger);

                using (Graphics g = Graphics.FromImage(bigger))
                {
                    foreach (ExtractedGlyphData glyphData in glyphDataList)
                    {
                        List<IntPoint> glyphPoints = (glyphData.RecognizedGlyph == null) ? glyphData.Quadrilateral : glyphData.RecognizedQuadrilateral;

                        Pen pen = new Pen(Color.Red, 3);

                        // highlight border
                        g.DrawPolygon(pen, ToPointsArray(glyphPoints));
                    }
                }

                if (PostAction != null)
                {
                    PostAction(bigger);
                }
            }
        }

        #region Helper methods
        // Convert list of AForge.NET framework's points to array of .NET's points
        private static System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
        {
            int count = points.Count;
            System.Drawing.Point[] pointsArray = new System.Drawing.Point[count];

            for (int i = 0; i < count; i++)
            {
                pointsArray[i] = new System.Drawing.Point(points[i].X, points[i].Y);
            }

            return pointsArray;
        }
        #endregion
    }
}
