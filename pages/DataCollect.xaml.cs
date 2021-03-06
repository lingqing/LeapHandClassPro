﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Windows.Threading;

namespace LeapMotionPro.pages
{
    /// <summary>
    /// DataCollect.xaml 的交互逻辑
    /// </summary>
    public partial class DataCollect : Page
    {        
        private static string sampleFileName = "";
        private List<HandType> handTypes;
        private long sampleNum = 0;
        private bool isGrapSample = false; // 采集中标志位
                
        private int updateFreq = 20;
        private long grapTimes = 10;
        private int grapTimesIndex = 0; // 已采集次数

        private List<float[]> sampleDataList = new List<float[]>();
        
        DispatcherTimer readFrameTimer = new DispatcherTimer();
        private WriteableBitmap bitmap;
             
        public DataCollect()
        {
            InitializeComponent();
            // 获得手型预分类
            handTypes = HandTypeManager.GetHandType();
            foreach (var handtype in handTypes)
            {
                IndexGrabClass.Items.Add(handtype.index + ":" + handtype.name);
            }
            IndexGrabClass.SelectedIndex = 0;

            //set greyscale palette for WriteableBitmap object
            List<Color> grayscale = new List<Color>();
            for (byte i = 0; i < 0xff; i++)
            {
                grayscale.Add(Color.FromArgb(0xff, i, i, i));
            }
            BitmapPalette palette = new BitmapPalette(grayscale);
            bitmap = new WriteableBitmap(640, 480, 72, 72, PixelFormats.Gray8, palette);
            displayImage.Source = bitmap;            

            // timer:定时触发Leap Frame
            readFrameTimer.Tick += new System.EventHandler(updataFrame);
            readFrameTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            //readFrameTimer.Start();
        }
        // page 暂停与继续
        public void pageActive()
        {
            readFrameTimer.Start();
        }
        public void pagePause()
        {
            readFrameTimer.Stop();
        }
        // 更新
        void updataFrame(object sender, EventArgs e)
        {
            //LeapMotionCtl.InitLeapMotion();
            WriteableBitmap bm = LeapMotionCtl.GetImage();
            bitmap = bm;
            displayImage.Source = bitmap;
            if (isGrapSample)
            {
                float[] item = LeapMotionCtl.LeapGrabSingleData();
                if (item == null) return;
                sampleDataList.Add(item);
                if (++grapTimesIndex >=  grapTimes)
                {
                    grapTimesIndex = 0;
                    isGrapSample = false;
                    ButtonDataGrab.IsEnabled = true;
                    
                    // 整理并写入 
                    List<float[]> gdl = SampleFilter.CleraFaultVector(sampleDataList);
                    if (gdl.Count <= 0) return;
                    int grabClass = IndexGrabClass.SelectedIndex + 1;  // 避开0
                    bool isExisted = false;
                    isExisted = File.Exists(sampleFileName);
                    // 文件存在           
                    using (StreamWriter file = new StreamWriter(sampleFileName, true))
                    {
                        foreach (var data in gdl)
                        {
                            file.Write(grabClass.ToString());                            
                            for (int i = 0; i < data.Length; i++)
                            {
                                file.Write(",");
                                file.Write(data[i].ToString());
                            }
                            file.WriteLine();                            
                        }
                        file.Close();
                        sampleNum += gdl.Count;
                        TxtBlkSmpNum.Text = sampleNum.ToString();
                    }
                    sampleDataList.Clear();
                }
            }

        }
        // 新建训练样本文件
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.OverwritePrompt = true;//询问是否覆盖
            fileDialog.Filter = "*.csv|*.csv";
            fileDialog.DefaultExt = "csv";//缺省默认后缀名
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                sampleFileName = fileDialog.FileName;
                //如果文件存在，删除重建
                if (File.Exists(sampleFileName))
                {
                    File.Delete(sampleFileName);
                }                
                //System.IO.Stream fileStream = fileDialog.OpenFile();//保存
                //fileStream.Close();
                ButtonDataGrab.Visibility = Visibility.Visible;
                sampleNum = 0;
                TxtBlkSmpNum.Text = "0";
            }
            else
            { return; }
        }

        /// 改变参数      
        private void ButtonSetPara_Click(object sender, RoutedEventArgs e)
        {
            updatePara();
        }
        
        // 更新参数   
        private void updatePara()
        {   
            if (GrapTimes.Text == null) grapTimes = 10;
            else grapTimes = Convert.ToInt64(GrapTimes.Text);            
            switch (BoxFreq.SelectedIndex)
            {
                case 0:
                    updateFreq = 5;
                    break;
                case 1:
                    updateFreq = 10;
                    break;
                case 2:
                    updateFreq = 20;
                    break;
                case 3:
                    updateFreq = 30;
                    break;
                case 4:
                    updateFreq = 50;
                    break;
                default:
                    break;
            }
            LabelHoldTime.Content = (1000 * grapTimes / updateFreq).ToString();
            readFrameTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000/updateFreq);
        }



        // 写入采集文件
        private void ButtonDataGrab_Click(object sender, RoutedEventArgs e)
        {
            isGrapSample = true;
            ButtonDataGrab.IsEnabled = false;    
        }
    }
}
