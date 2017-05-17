using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PGR_Polotonovani
{
    public partial class Form1 : Form
    {
        ImageEditor ie;
        List<int> modes= new List<int>() { 0,1,2,3,4  };
        Bitmap originalBitmap;
        int[,] matrix;
        string matrixName = "none";
        public Form1()
        {
            InitializeComponent();
            foreach (var i in modes)
                listBox1.Items.Add((PictureBoxSizeMode)i);
            try
            {
                pictureBox1.Image = Image.FromFile("land.jpg");
                originalBitmap = (Bitmap)Image.FromFile("land.jpg");
            }
            catch (Exception)
            {
                ;
            }
            ie = new ImageEditor();
            ie.Done += OnDone;
            ie.FinalDone += OnFinalDone;

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
            originalBitmap= new Bitmap(openFileDialog1.FileName);
            trackBar1.Value = 50;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        public void OnDone(object source, DoubleEvArgs args)
        {
            Invoke(new MethodInvoker(() => {
                toolStripProgressBar1.Value = args.Prog;
            }));
                    
        }
        public void OnFinalDone(object source, BitmapEventArgs args)
        {
            this.Invoke(new MethodInvoker(() => {
                toolStripProgressBar1.Value = 100;
                pictureBox1.Image = args.B;
                pictureBox1.Invalidate();
            }));

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = (PictureBoxSizeMode) listBox1.SelectedItem;
            if (pictureBox1.SizeMode != PictureBoxSizeMode.AutoSize)
                pictureBox1.Dock = DockStyle.Fill;
            else
                pictureBox1.Dock = DockStyle.None;
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Bitmap bmp = originalBitmap;
            double tbv = (double)trackBar1.Value/50;
            double zooom = tbv*tbv*tbv ;
            Size newSize = new Size((int)(originalBitmap.Width * zooom), (int)(originalBitmap.Height * zooom));
            label1.Text = newSize.Width.ToString() + ", ";
            label2.Text = newSize.Height.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            originalBitmap = (Bitmap)pictureBox1.Image;
        }
        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            Bitmap bmp = originalBitmap;
            double tbv = (double)trackBar1.Value / 50;
            double zooom = tbv * tbv * tbv;
            Size newSize = new Size((int)(originalBitmap.Width * zooom), (int)(originalBitmap.Height * zooom));
            try { bmp = new Bitmap(originalBitmap, newSize); }
            catch (Exception)
            {
                trackBar1.Value = 50;
            }
            pictureBox1.Image = bmp;
            label1.Text = bmp.Width.ToString() + ", ";
            label2.Text = bmp.Height.ToString();
            pictureBox1.Invalidate();
        }

        private void halfToneDiffuseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (matrix == null)
            {
                AutoClosingMessageBox.Show("Matrix is null, choose one!", "Caption", 2000);
                using (FormMatrix fm = new FormMatrix())
                {
                    fm.ShowDialog(this);

                    matrix = fm.matrix;
                    matrixName = fm.matrixName;
                }

            }
            else
                ie.Dithering((Bitmap)pictureBox1.Image, matrix, checkBoxRand.Checked);
        }

        private void matrixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormMatrix fm = new FormMatrix())
            {
                fm.ShowDialog(this);

                matrix = fm.matrix;
                matrixName = fm.matrixName;
            }
        }

        private void halfToneCirclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ie.HalfTone((Bitmap)pictureBox1.Image,((int)nPPD.Value));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = r.Next(0, 10000).ToString()+"_"+pictureBox1.Image.Width+"x"+pictureBox1.Image.Height+"in_"+matrixName+".bmp";
            pictureBox1.Image.Save(sfd.FileName,ImageFormat.Bmp);

            
        }

        private void halfToneMultiTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ie.BlackHalfToneMultiTask((Bitmap)pictureBox1.Image, ((int)nPPD.Value));
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "img";
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
        }

        private void bWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ie.BandW((Bitmap)pictureBox1.Image);
        }

        private void reverseIntensityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ie.BlacktoWhite_whiteToGray((Bitmap)pictureBox1.Image);
        }
    }
    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
                MessageBox.Show(text, caption);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }

}
