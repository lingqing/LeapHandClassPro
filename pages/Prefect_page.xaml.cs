using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpOSC;

namespace LeapMotionPro.pages
{
    /// <summary>
    /// Prefect_page.xaml 的交互逻辑
    /// </summary>
    public partial class Prefect_page : Page
    {
        private BpNet prefectBp = new BpNet();
        private string recordeFileName;
        private bool isPrefect = false;
        private bool isRecord = false;
        // GTK
        private bool sendToGTK = false; // 直接发送到GTK标志
        OscMessage message = new OscMessage("/Data", 1);
        UDPSender oscSender = new UDPSender("127.0.0.1", 9000);

        private long recordedNum = 0; // 记录样本数
        private int nullTimes = 0;

        private List<HandType> handTypes;

        DispatcherTimer frameTimer = new DispatcherTimer();
        private WriteableBitmap bitmap;
        // 存储采集缓存数据，通过滤波提高准确率
        List<float[]> dataBuf = new List<float[]>();

        private List<int> resultList = new List<int>();  // 预测结果缓存
        public Prefect_page()
        {
            InitializeComponent();

            handTypes = HandTypeManager.GetHandType();
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
            frameTimer.Tick += new System.EventHandler(updataFrame);
            frameTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            //frameTimer.Start();

            handTypes = HandTypeManager.GetHandType();
        }

        // page 暂停与继续
        public void pageActive()
        {
            frameTimer.Start();
        }
        public void pagePause()
        {
            frameTimer.Stop();
        }
        // 定时器触发函数
        private void updataFrame(object sender, EventArgs e)
        {
            // update image
            WriteableBitmap bm = LeapMotionCtl.GetImage();
            bitmap = bm;
            displayImage.Source = bitmap;
            // prefect 改为 record
            if (isRecord)
            {
                // 采集3帧
                for (int i = 0; i < 3; i++)
                {
                    float[] item = LeapMotionCtl.LeapGrabSingleData();
                    if (item == null)
                    {
                        if (nullTimes ++ > 5)
                        {
                            nullTimes = 0; dataBuf.Clear();
                            TxtPrefectResult.Text = "";
                        }
                        return;
                    }
                    dataBuf.Add(item);
                }
                if (dataBuf.Count >= 5)
                {
                    bool[] isGood = SampleFilter.FindFaultVector(dataBuf);
                    if (!isGood[isGood.Length - 1]) return;  // 最后一个被剔除
                    // 坏点比例
                    int goodNum = 0;
                    for(int i = 0; i< isGood.Length; i++)
                    {
                        if (isGood[i]) goodNum++;
                    }
                    if ((float)goodNum / isGood.Length <= 0.7f)
                    {
                        dataBuf.Clear();
                        return; // 可能处于过度阶段
                    }

                    SampleFilter.UpdateFaultVector(dataBuf, isGood);   // 清异常点   
                    // 删除
                    // Todo
                    for (int i = 0; i < isGood.Length - 5; i++)  // 最多保留5个
                    {
                        dataBuf.RemoveAt(0);
                    }
                }
                // 记录
                double[] prefDataIn = Array.ConvertAll<float, double>(dataBuf[dataBuf.Count - 1],s => (double)s);
                // prefDataIn 表示选中的预测输入向量

                /*************** V2.0 不进行预测 **************************/
                //double[] prefctResult = prefectBp.sim(prefDataIn); 
                //int intResult = (int)Math.Round(prefctResult[0], 0);
                //if (Math.Abs(intResult - prefctResult[0]) > 0.2) return; // 类型无法分类

                //resultList.Add(intResult);
                //if (resultList.Count < 5) return; //  采样太少
                //resultList.RemoveAt(0);
                //// @Todo 分析结果队列
                //int[] typeCount = new int[handTypes.Count];  // 累加表
                //for (int i = 0; i < handTypes.Count; i++)       // 归零
                //{
                //    typeCount[i] = 0;
                //}
                //foreach (var res in resultList)
                //{
                //    typeCount[res]++;
                //}
                //int maxType = 0;
                //for (int i = 1; i < typeCount.Length; i++)
                //{
                //    if (typeCount[maxType] <= typeCount[i]) maxType = i;
                //}
                //if ((float)typeCount[maxType] / (resultList.Count + 1) < 0.7)
                //{
                //    dataBuf.Clear();
                //    return; // 可能处于过度阶段
                //}

                //TxtPrefectResult.Text = handTypes[maxType].index + ": " + handTypes[maxType].name;

                using (StreamWriter sw = new StreamWriter(recordeFileName, true))
                {
                    //sw.WriteLine(maxType.ToString());
                    sw.Write(prefDataIn[0].ToString());
                    for (int i = 1; i < prefDataIn.Length; i++)
                    {
                        sw.Write(",");
                        sw.Write(prefDataIn[i].ToString());
                    }
                    sw.WriteLine();
                    sw.Close();
                    recordedNum++;

                    TxtPrefectResult.Text = "记录数目：" + recordedNum.ToString();
                } 
                if(sendToGTK)
                {
                    message = new SharpOSC.OscMessage("/Data", prefDataIn[0], prefDataIn[1], prefDataIn[2], prefDataIn[3], prefDataIn[4],
                    prefDataIn[5], prefDataIn[6], prefDataIn[7], prefDataIn[8], prefDataIn[9], prefDataIn[10], prefDataIn[11], prefDataIn[12], prefDataIn[13], prefDataIn[14],
                    prefDataIn[15], prefDataIn[16], prefDataIn[17], prefDataIn[18], prefDataIn[19], prefDataIn[20]);
                    oscSender.Send(message);                    
                }
            }           
            else dataBuf.Clear(); // 清除
        }

        // 加载训练好的分类器
        //private void LoadBpNet_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "*.train|*.train";
        //    openFileDialog.DefaultExt = "train";//缺省默认后缀名
        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        string fileName = openFileDialog.FileName;
        //        if (!File.Exists(fileName)) return;
        //        if(prefectBp.ReadParaFromFile(fileName))
        //        {
        //            TxtBpNetPara.Text = "输入层数目：" + prefectBp.inNum + "\r\n"
        //                + "隐层节点数：" + prefectBp.hideNum;
        //            // 显示新建记录文件按钮                    
        //            ButtonPrefect.Visibility = Visibility.Visible;
        //            Monitor.ReportMsg(1, "加载分类器：" + fileName);
        //        }
        //        else
        //        {
        //            TxtBpNetPara.Text = "加载失败";
        //            Monitor.ReportMsg(1,"分类器加载失败!");
        //        }
        //    }
        //}

        // 预测
        //private void ButtonPrefect_Click(object sender, RoutedEventArgs e)
        //{
        //    // Todo
        //    isPrefect = !isPrefect;
        //    if (isPrefect)
        //    {
        //        ButtonNewRecordFile.Visibility = Visibility.Visible;
        //        ButtonPrefect.Content = "停止预测";
        //        Monitor.ReportMsg(1,"预测中");
        //    }
        //    else
        //    {
        //        ButtonNewRecordFile.Visibility = Visibility.Hidden;
        //        ButtonPrefect.Content = "点击预测";
        //        Monitor.ReportMsg(1,"停止预测");
        //    }
        //}

        // 开始记录至文件
        private void ButtonRecord_Click(object sender, RoutedEventArgs e)
        {
            isRecord = true;
            ButtonRecord.IsEnabled = false;
            ButtonStopRecord.Visibility = Visibility.Visible;
            Monitor.ReportMsg(1, "记录预测结果中");
        }
        // 新建记录文件
        private void ButtonNewRecordFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.OverwritePrompt = true;//询问是否覆盖
            fileDialog.Filter = "*.csv|*.csv";
            fileDialog.DefaultExt = "csv";//缺省默认后缀名
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                recordeFileName = fileDialog.FileName;
                //如果文件存在，删除重建
                if (File.Exists(recordeFileName))
                {
                    File.Delete(recordeFileName);
                }
                //System.IO.Stream fileStream = fileDialog.OpenFile();//保存
                //fileStream.Close();
                ButtonRecord.Visibility = Visibility.Visible;
                Monitor.ReportMsg(1, "新建记录文件" + recordeFileName);
                recordedNum = 0;
                TxtPrefectResult.Text = "记录数目：0";
            }
            else
            { return; }
        }

        private void ButtonStopRecord_Click(object sender, RoutedEventArgs e)
        {
            isRecord = false;
            ButtonRecord.IsEnabled = true;
            ButtonStopRecord.Visibility = Visibility.Hidden;
            Monitor.ReportMsg(1, "停止记录,记录文件" + recordeFileName);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int updateFreq = 20;
            switch (ComboFreq.SelectedIndex)
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
            frameTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / updateFreq);
            Monitor.ReportMsg(0, "帧频：" + updateFreq.ToString());
        }

        // 发送至GTK
        private void BtnSendToGTK_Click(object sender, RoutedEventArgs e)
        {
            sendToGTK = (bool)BtnSendToGTK.IsChecked;
        }
    }
}
