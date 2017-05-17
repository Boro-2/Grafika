using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Imaging;

namespace PGR_Polotonovani
{
    public class DoubleEvArgs : EventArgs
    {
        public DoubleEvArgs(double d)
        {
            this.Prog = (int)d;
        }
        public int Prog { get; private set; }
    }
    public class BitmapEventArgs : EventArgs
    {
        public BitmapEventArgs(Bitmap b)
        {
            this.B = b;
        }
        public Bitmap B { get; private set; }
    }
    class ImageEditor
    {
        Bitmap b;
        //public Bitmap bmpOut;
        public Bitmap BMPIN { set { b = value; } }
        public Mutex m;
        double progress = 0;
        public int Progress { get { return (int)progress; } }

        public delegate void DoneEventHandler(object source, DoubleEvArgs args);
        public event DoneEventHandler Done;

        public delegate void FinalDoneEventHandler(object source, BitmapEventArgs args);
        public event FinalDoneEventHandler FinalDone;

        protected virtual void OnDone(double d)
        {
            Done?.Invoke(this, new DoubleEvArgs(d));
        }
        protected virtual void OnFinalDone(Bitmap b)
        {
            FinalDone?.Invoke(this, new BitmapEventArgs(b));
        }

        public ImageEditor()
        {
            m=new Mutex();            
        }

        public void BlacktoWhite_whiteToGray(Bitmap bo)
        {

            Task t = new Task(() =>
            {
                Random r = new Random();
                Bitmap b = (Bitmap)bo.Clone();
                Bitmap bmpOut = new Bitmap(b.Width, b.Height);
                Color px;
                double inty;// intensity
                Color colSet;
                for (int i = 0; i < b.Height; i++)
                {
                    for (int j = 0; j < b.Width; j++)
                    {
                        px = b.GetPixel(j, i);
                        inty = (0.3 * px.R + 0.59 * px.G + 0.11 * px.B);// 0-255 => 0-1
                        if (inty > 127)
                            colSet = Color.Black;
                        else
                            colSet = Color.LightGray;
                        bmpOut.SetPixel(j, i, colSet);



                    }
                    progress = (double)i / b.Height * 100;
                    OnDone(progress);

                }
                OnFinalDone(bmpOut);

            });
            t.Start();

        }
        public void BandW(Bitmap bo)
        {

            Task t = new Task(() =>
            {
                Random r = new Random();
                Bitmap b = (Bitmap)bo.Clone();
                Bitmap bmpOut = new Bitmap(b.Width, b.Height);
                Color px;
                double inty;// intensity
                Color colSet;
                for (int i = 0; i < b.Height; i++)
                {
                    for (int j = 0; j < b.Width; j++)
                    {
                        px = b.GetPixel(j, i);
                        inty = (0.3 * px.R + 0.59 * px.G + 0.11 * px.B);// 0-255 => 0-1
                        if(inty>127)
                            colSet = Color.White;
                        else
                            colSet = Color.Black;
                        bmpOut.SetPixel(j, i, colSet);



                    }
                    progress = (double)i / b.Height * 100;
                    OnDone(progress);

                }
                OnFinalDone(bmpOut);

            });
            t.Start();

        }
        public void HalfTone(Bitmap bo, int ppd)
        {
           
            Task t = new Task(() =>
            {
                Random r = new Random();
                Bitmap b = (Bitmap)bo.Clone();
                Bitmap bmpOut = new Bitmap(b.Width*ppd, b.Height*ppd);
                Color px;
                double inty;// intensity
                Color colSet;
                for (int i = 0; i < b.Height; i++)
                {
                    for (int j = 0; j < b.Width; j++)
                    {
                        px = b.GetPixel(j, i);
                        inty = (0.3 * px.R + 0.59 * px.G + 0.11 * px.B)/255;// 0-255 => 0-1
                        for (int l = 0; l < ppd; l++)
                        {
                            for (int k = 0; k < ppd; k++)
                            {
                                if (r.NextDouble() <= inty)//bigger inty => more whites
                                    colSet = Color.White;
                                else
                                    colSet = Color.Black;

                                bmpOut.SetPixel(ppd*j + k, ppd*i + l, colSet);

                            }
                        }
                        
                       

                    }
                    progress = (double)i / b.Height *100;
                    OnDone(progress);
                    
                }
                OnFinalDone(bmpOut);

            });
            t.Start();
            
        }

        public void Dithering(Bitmap bo, int[,] matrix, bool randomMid)
        {
            
            Task t = new Task(() =>
            {
                OnDone(0);
                Bitmap b = (Bitmap)bo.Clone();
                double[,] vals;
                Random r = new Random();
                Bitmap bmpOut = new Bitmap(bo.Width, bo.Height);
                //Clear(bmpOut);
                vals = new double[bo.Height, bo.Width];
                Color px;
                Color colSet;
                double err;
                int mlength = matrix.GetLength(1);
                int[] firstRow = matrix.Cast<int>().Take(mlength).ToArray();
                int posX = firstRow.ToList().IndexOf(-1);
                int before = posX;
                int after = mlength - 1 - posX;
                int under = matrix.GetLength(0) - 1;
                int div = (from int xs in matrix where xs > 0 select xs).Sum();

                for (int i = 0; i < vals.GetLength(0); i++)//Height
                {
                    for (int j = 0; j < vals.GetLength(1); j++)//Width
                    {
                        px = b.GetPixel(j,i);
                        vals[i, j] += 0.3 * px.R + 0.59 * px.G + 0.11 * px.B;//max 255
                    }
                    OnDone(((double)i / vals.GetLength(0) * 100) / 2);

                }
                for (int i = 0; i < vals.GetLength(0); i++)//Height
                {
                    for (int j = 0; j < vals.GetLength(1); j++)//Width
                    {
                        int mid = randomMid ? 127 + r.Next(-100, 100) : 127;
                        if (mid <= vals[i, j]){
                            colSet = Color.White;
                            err = (vals[i, j]-255);
                        }
                        else{
                            colSet = Color.Black;
                            err = (vals[i, j]);
                        }

                        bmpOut.SetPixel(j, i, colSet);
                        if (i + under < vals.GetLength(0) && j + after < vals.GetLength(1) && j > before)//diffuse err if possible
                            for (int m = 0; m < matrix.GetLength(0); m++)
                                for (int n = 0; n < mlength; n++)
                                    vals[i + m, j - posX + n] += err * matrix[m, n] / div;

                    }
                    OnDone(((double)i / vals.GetLength(0) * 100) / 2 + 50);
                }                
                OnFinalDone(bmpOut);
            });
            t.Start();

        }

        private void Clear(Bitmap bmpOut)
        {
            for (int i = 0; i < bmpOut.Width; i++)
            {
                for (int j = 0; j < bmpOut.Height; j++)
                {
                    bmpOut.SetPixel(i, j, Color.Black);
                }
            }
        }
        public void BlackHalfToneMultiTask(Bitmap bo, int ppd) //inefficient 
        {
            Random r = new Random();
            Bitmap b = (Bitmap)bo.Clone();
            Bitmap bmpOut = new Bitmap(b.Width * ppd, b.Height * ppd);
            double prog1 = 0;
            double prog2 = 0;
            int height = b.Height;
            int width = b.Width;
            Task t1 = new Task(() =>
            {
                Color px;
                double inty;// intensity
                Color colSet;

                for (int i = 0; i < height; i += 2)
                {
                    for (int j = 0; j < width; j++)
                    {
                        m.WaitOne();
                        px = b.GetPixel(j, i);
                        m.ReleaseMutex();
                        inty = (0.3 * px.R + 0.59 * px.G + 0.11 * px.B) / 255;//max 255
                        for (int l = 0; l < ppd; l++)
                        {
                            for (int k = 0; k < ppd; k++)
                            {
                                if (r.NextDouble() <= inty)
                                    colSet = Color.White;
                                else
                                    colSet = Color.Black;
                                m.WaitOne();
                                bmpOut.SetPixel(ppd * j + k, ppd * i + l, colSet);
                                m.ReleaseMutex();
                            }
                        }
                    }
                    prog1 = (double)i / height * 100;
                    OnDone((prog1 + prog2) / 2);

                }
                Thread.CurrentThread.Abort();
            });
            t1.Start();
            Task t2 = new Task(() =>
            {
                Color px;
                double inty;// intensity
                Color colSet;

                for (int i = 1; i < height; i += 2)
                {
                    for (int j = 0; j < width; j++)
                    {
                        m.WaitOne();
                        px = b.GetPixel(j, i);
                        m.ReleaseMutex();
                        inty = (0.3 * px.R + 0.59 * px.G + 0.11 * px.B) / 255;//max 255
                        for (int l = 0; l < ppd; l++)
                        {
                            for (int k = 0; k < ppd; k++)
                            {
                                if (r.NextDouble() <= inty)
                                    colSet = Color.White;
                                else
                                    colSet = Color.Black;
                                m.WaitOne();
                                bmpOut.SetPixel(ppd * j + k, ppd * i + l, colSet);
                                m.ReleaseMutex();
                            }
                        }
                    }
                    prog2 = (double)i / height * 100;
                }
                Thread.CurrentThread.Abort();
            });
            t2.Start();
            Task t = new Task(() =>
            {
                Thread.Sleep(100);
                while (t1.Status == TaskStatus.Running || t2.Status == TaskStatus.Running)
                {
                    Thread.Sleep(100);
                    OnDone((prog1 + prog2) / 2);
                }
                OnDone(100);
            });
            t.Start();

        }

    }

}
