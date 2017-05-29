using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using System.IO;
namespace WhetherAndCamera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           
        }
        FilterInfoCollection videoDevices; //摄像头设备  
        VideoCaptureDevice videoSource;
        VideoFileWriter videoWriter = null;
        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = GetDevices();
            foreach (FilterInfo i in videoDevices)
            {
                toolStripComboBox1.Items.Add(i.Name);
            }
            if (videoDevices == null)
                return;
            FilterInfo info = videoDevices[0];//选取第一个,此处可作灵活改动
            videoSource = new VideoCaptureDevice(info.MonikerString);//视频的来源选择 
            videoSourcePlayer1.VideoSource = videoSource;
            toolStripComboBox1.Text = videoDevices[0].Name;
           


        }
        public FilterInfoCollection GetDevices()
        {
            try
            {
                //枚举所有视频输入设备  
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count != 0)
                {
                    return videoDevices;
                }
                else
                {
                    MessageBox.Show("没有找到设备！");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("没有找到设备！");
                return null;
            }
        }   
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoSourcePlayer1.Stop();
        }

        private void 结束ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoSourcePlayer1.SignalToStop();
            videoSourcePlayer1.WaitForStop();
        }

        private void 开始ToolStripMenuItem_Click(object sender, EventArgs e)
        {          
            videoSourcePlayer1.Start();
        }

        private void 拍照ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (videoSourcePlayer1.IsRunning)
            {
                Bitmap bm = videoSourcePlayer1.GetCurrentVideoFrame();
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.FileName = "0.bmp";
                saveDialog.DefaultExt = "bmp";
                saveDialog.Filter = "图片文件(*.bmp)|*.bmp";
                DialogResult dr=saveDialog.ShowDialog();
                string FileName = saveDialog.FileName;
                if (dr == DialogResult.OK)
                    bm.Save(FileName);         
            }
            else
            {
                MessageBox.Show("摄像头未打开");
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (videoSourcePlayer1.IsRunning)
                videoSourcePlayer1.Stop();
            FilterInfo info = videoDevices[toolStripComboBox1.SelectedIndex];//选取第一个,此处可作灵活改动
            VideoCaptureDevice videoSource = new VideoCaptureDevice(info.MonikerString);
            videoSourcePlayer1.VideoSource = videoSource;
        }
        bool isShooting = false;
        const int frameRate = 10;
        string VideoFileName;
        string VideoFile;
        private void 录像ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!videoSourcePlayer1.IsRunning)
            {
                MessageBox.Show("摄像头未启动!","视频录制", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            isShooting = !isShooting;
            if (isShooting)
            {
               
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "选择视频保存路径";
                saveDialog.FileName = "newVideo";
                saveDialog.DefaultExt = "avi";
                saveDialog.Filter = "视频文件(*.avi)|*.avi";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    //  NewVideo(saveDialog.FileName);
                    string[] strs = saveDialog.FileName.Split('\\');
                    VideoFile = "";
                    for (int i = 0; i < strs.Length - 1; i++)
                        VideoFile += strs[i] + "\\";
                    VideoFileName = saveDialog.FileName;
                    videoSourcePlayer1.NewFrame += new AForge.Controls.VideoSourcePlayer.NewFrameHandler(sourcePlayer_NewFrame);                  
                    录像ToolStripMenuItem.Text = "结束录像";
                }
                
            }
            else
            {
                videoSourcePlayer1.NewFrame -= new AForge.Controls.VideoSourcePlayer.NewFrameHandler(sourcePlayer_NewFrame);
                录像ToolStripMenuItem.Text = "开始录像";
                isBeginWriteVideo = false;
                if (videoWriter != null)
                {
                    videoWriter.Close();
                    videoWriter.Dispose();
                    if (MessageBox.Show("录制成功,是否查看视频？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        System.Diagnostics.Process.Start(VideoFileName);
                }

            }
        }
        bool isBeginWriteVideo=false;
        private void NewVideo(string FileName)
        {
            if (videoWriter != null)
            {
                videoWriter.Close();
                videoWriter.Dispose();
            }
            try
            {
                Bitmap image = new Bitmap(640, 480);
                videoWriter = new VideoFileWriter();
                //这里必须是全路径，否则会默认保存到程序运行根据录下了
                videoWriter.Open(FileName, image.Width, image.Height, frameRate, VideoCodec.MPEG4);
                videoWriter.WriteVideoFrame(image);
                isBeginWriteVideo = true;
                
            }
            catch
            {
                MessageBox.Show("录制视频错误");
            }
        }
        private void show_video(object sender, NewFrameEventArgs eventArgs)
        {                     
            if (isShooting)
            {
                videoWriter.WriteVideoFrame(eventArgs.Frame);
            }
        }
        private void sourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            if (!isBeginWriteVideo)
                NewVideo(VideoFileName);
            else
            {
                videoWriter.WriteVideoFrame(image);
            }
        }

    }
}
