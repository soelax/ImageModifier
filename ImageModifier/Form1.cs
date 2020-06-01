using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageModifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image img = Image.FromFile(@"C:\temp\out4.bmp");
            //byte[] imgContent = imageToByteArray(img);
            Bitmap bmp = new Bitmap(img);
            //ToBW(bmp);
            Bitmap modifBmp = Convolve(bmp, 1);
            modifBmp.Save(@"C:\temp\out5.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

        
        private Bitmap Convolve(Bitmap bmp, int windowSize)
        {
            Bitmap result = new Bitmap(bmp.Width, bmp.Height);
            Dictionary<Point, Color> origImg = new Dictionary<Point, Color>();
            int pixelsChanged = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int sumR = 0, sumG = 0, sumB = 0;

                    int xMin = x - windowSize, xMax = x + windowSize;
                    int yMin = y - windowSize, yMax = y + windowSize;

                    if (xMin < 0) xMin = 0;
                    if (yMin < 0) yMin = 0;
                    if (xMax >= bmp.Width) xMax = bmp.Width - 1;
                    if (yMax >= bmp.Height) yMax = bmp.Height - 1;

                    int windowPix = (xMax - xMin + 1) * (yMax - yMin + 1);

                    for (int xRun = xMin; xRun <= xMax; xRun++)
                        for (int yRun = yMin; yRun <= yMax; yRun++)
                        {
                            Point p = new Point(xRun, yRun);
                            Color pix;
                            if (!origImg.TryGetValue(p, out pix))
                            {
                                pix = bmp.GetPixel(xRun, yRun);
                                origImg.Add(p, pix);
                            }
                            sumR += pix.R;
                            sumG += pix.G;
                            sumB += pix.B;
                        }

                    int newR = sumR / windowPix;
                    int newG = sumG / windowPix;
                    int newB = sumB / windowPix;
                    Color newColor = Color.FromArgb(newR, newG, newB);
                    
                    Color origPix;
                    if(!origImg.TryGetValue(new Point(x, y), out origPix))
                    {
                        origPix = bmp.GetPixel(x, y);
                        origImg.Add(new Point(x, y), origPix);
                    }

                    const int maxDiff = 2;
                    if (Math.Abs(origPix.R - newColor.R) > maxDiff ||
                        Math.Abs(origPix.G - newColor.G) > maxDiff ||
                        Math.Abs(origPix.B - newColor.B) > maxDiff)
                    {
                        pixelsChanged++;
                        result.SetPixel(x, y, newColor); // only update is we consider it having changed
                    }
                    else
                    {
                        result.SetPixel(x, y, origPix);
                    }
                }
                System.Diagnostics.Debugger.Log(0, "", $"Column {x} of {bmp.Width} completed\n");
            }
            System.Diagnostics.Debugger.Log(0, "", $"{pixelsChanged} pixels changed in total (of {bmp.Width * bmp.Height})\n");

            return result;
        }

        private void ToBW(Bitmap bmp)
        {
            for(int x = 0; x < bmp.Width; x++)
                for(int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(x, y);
                    int avg = (c.R + c.G + c.B) / 3;
                    bmp.SetPixel(x, y, Color.FromArgb(avg, avg, avg));

                }
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
        }

        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
    }
}
