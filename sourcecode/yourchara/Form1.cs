using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace yourchara
{
    public partial class Form1 : Form
    {
        private Image img_idle = Image.FromFile("chara0.png");
        private Image img_lowtalk = Image.FromFile("chara1.png");
        private Image img_hightalk = Image.FromFile("chara2.png");
        private Image nowimg = Image.FromFile("chara0.png");
        private int global_delta_r = 100;
        private float global_delta = 50;
        private WaveIn recorder;
        private bool nowrecording = false;
        private Color bg_color = Color.Lime;
        private Graphics img_graphics;

        public Form1()
        {
            InitializeComponent();
            img_graphics = this.CreateGraphics();
            this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                string get = String.Format("Device {0}: {1}, {2} channels", waveInDevice, deviceInfo.ProductName, deviceInfo.Channels);
                comboBox1.Items.Add(get);
            }
            this.ReDraw();
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs args)
        {
            float max = 0;
            // interpret as 16 bit audio
            for (int index = 0; index < args.BytesRecorded; index += 2)
            {
                short sample = (short)((args.Buffer[index + 1] << 8) |
                                        args.Buffer[index + 0]);
                // to floating point
                var sample32 = sample / 32768f;
                // absolute value 
                if (sample32 < 0) sample32 = -sample32;
                // is this the max value?
                if (sample32 > max) max = sample32;
            }
            progressBar1.Value = (int)(max * 100f);
            volumelabel.Text = progressBar1.Value.ToString();
            bool somechange = false;
            if (progressBar1.Value >= track_low.Value && nowimg != img_lowtalk)
            {
                nowimg = img_lowtalk;
                somechange = true;
            }
            else if (progressBar1.Value >= track_high.Value && nowimg != img_hightalk)
            {
                nowimg = img_hightalk;
                somechange = true;
            }
            else if (nowimg != img_idle)
            {
                nowimg = img_idle; 
                somechange = true;
            }
            if (somechange)
            {
                global_delta = 50;
                this.Refresh();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.AllowFullOpen = true;
            colorDlg.AnyColor = true;
            colorDlg.SolidColorOnly = false;
            colorDlg.Color = bg_color;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                bg_color = colorDlg.Color;
                this.BackColor = bg_color;
                button2.ForeColor = bg_color;
            }  
        }

        private int Lerp(int a, int b, float t)
        {
            float tp = 1.0f - t;
            return (int)(tp * a + t * b);
        }

        private void ReDraw()
        {
            this.Refresh();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //global_delta = (float) this.Lerp((int)global_delta, global_delta_r, 0.001f);
            float delta = 100f;
            float size = this.Height;
            e.Graphics.DrawImage(nowimg, this.Width / 2 - size / 2 + delta / 2, this.Height - size + delta, size - delta, size - delta);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (nowrecording == true)
            {
                recorder.StopRecording();
                nowrecording = false;
            }
            recorder = new WaveIn();
            recorder.DeviceNumber = comboBox1.SelectedIndex;
            recorder.DataAvailable += waveIn_DataAvailable;
            int sampleRate = 8000;
            int channels = 1;
            recorder.WaveFormat = new WaveFormat(sampleRate, channels);
            recorder.StartRecording();
            nowrecording = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (groupBox1.Visible)
                groupBox1.Visible = false;
            else
                groupBox1.Visible = true;
        }

        private void Form1_AutoSizeChanged(object sender, EventArgs e)
        {
            this.ReDraw();
        }
    }
}
