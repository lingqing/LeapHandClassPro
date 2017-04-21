using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using Visifire.Charts;
using Visifire.Commons;
using System.IO;
using System.Windows.Forms;

namespace LeapMotionPro.pages
{
    /// <summary>
    /// DataTrain.xaml 的交互逻辑
    /// </summary>
    public partial class DataTrain : Page
    {
        // 后台进程
        private BackgroundWorker worker = new BackgroundWorker();
        private const int paraNum = 20;
        private string sampleFileName; // 样本文件名
        private long sampleCnt = 0;
        //
        private BpNet bp = new BpNet();
        // 绘图       
        private double bpE = 0;
        // 列表
        List<double[]> sampleList = new List<double[]>();
        List<double[]> sampleClassList = new List<double[]>();        

        public DataTrain()
        {
            InitializeComponent();
        }

        private void NewTrain_Checked(object sender, RoutedEventArgs e)
        {

        }
        // 开始训练
        private void ButtonDataTrain_Click(object sender, RoutedEventArgs e)
        {
            worker.DoWork += new DoWorkEventHandler(backTrain);
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(updateTrainProcess);
            string[] strTxt = new string[2];
            strTxt[0] = TxtMaxCnt.Text;
            strTxt[1] = TxtMinErr.Text;
            worker.RunWorkerAsync(strTxt);            
        }
        // 训练函数
        private void backTrain(object sender, DoWorkEventArgs e)
        {
            // read samples 
            double[,] ppArray = new double[sampleCnt, paraNum];
            double[,] ttArray = new double[sampleCnt, 1];

            // 变List 为Array
            for (int i = 0; i < sampleCnt; i++)
            {
                for (int j = 0; j  < paraNum; j++)
			    {
			        ppArray[i,j] = sampleList[i][j];
			    }
                ttArray[i, 0] = sampleClassList[i][0];
            }

            bp = new BpNet(ppArray, ttArray);
            string[] para = ((string[])e.Argument);
            long maxCount = Convert.ToInt64(para[0]);
            double minError = Convert.ToDouble(para[1]);
            long study = 0;
            do
            {
                study++;
                bp.train(ppArray, ttArray);
                if (study % 10 == 0) worker.ReportProgress((int)(study * 1000 / maxCount));
                bpE = bp.e;
                //       bp.rate=0.95-(0.95-0.3)*study/50000;  
                //        Console.Write("第 "+ study+"次学习： ");  
                //        Console.WriteLine(" 均方差为 "+bp.e);                 
            } while (bp.e > minError && study < maxCount);
            worker.ReportProgress(1000);
            //TrainState.Content = "共 " + study + "次学习： "
            //    + " 均方差为 " + bp.e;

            //bp.saveMatrix(bp.w, "conf/w.txt");
            //bp.saveMatrix(bp.v, "conf/v.txt");
            //bp.saveMatrix(bp.b1, "conf/b1.txt");
            //bp.saveMatrix(bp.b2, "conf/b2.txt");
            //bp.saveParas("conf/para.txt");
        }
        // 训练进度
        private void updateTrainProcess(object sender, ProgressChangedEventArgs e)
        {
            int i = e.ProgressPercentage;
            TrainProcess.Value = i;
            TrainState.Content = i / 10 + "." + i % 10 + "%";
            //TrainTimes.Content = i*

            LightDataPoint ld = new LightDataPoint();
            ld.XValue = i;
            ld.YValue = bpE;
            _chart.Series[0].DataPoints.Add(ld);
        }

        // 打开训练数据
        private void OpenDataFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();            
            fileDialog.Filter = "*.txt|*.txt";
            fileDialog.DefaultExt = "txt";//缺省默认后缀名
           
            double[] pp1 = new double[paraNum];
            double[] tt1 = new double[1];

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                sampleList.Clear();
                sampleFileName = fileDialog.FileName;
                if (!File.Exists(sampleFileName)) return;
                StreamReader sr = new StreamReader(sampleFileName);
                String line;
                for (int j = 0; (line = sr.ReadLine()) != null; j++)
                {
                    string[] strArr = line.Split('\t');
                    for (int i = 0; i < paraNum; i++)
                    {
                         pp1[i] = Convert.ToDouble(strArr[i]);
                    }
                    sampleList.Add(pp1);
                    sampleClassList.Add(tt1);
                    sampleCnt = sampleList.Count;
                }
                sr.Close();
                TxtSampleCnt.Text = sampleCnt.ToString();
                //如果文件存在，删除重建
                //if (File.Exists(sampleFileName))
                //{
                //    File.Delete(sampleFileName);
                //}
                //System.IO.Stream fileStream = fileDialog.OpenFile();//保存
                //fileStream.Close();
                ButtonDataTrain.Visibility = Visibility.Visible;
            }
            else
            { return; }
        }
        // 导入训练结果
        private void ButtonReadPara_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "*.train|*.train";
            openFileDialog.DefaultExt = "train";//缺省默认后缀名
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                if (!File.Exists(fileName)) return;
                bp.ReadParaFromFile(fileName);
            }
        }
        // 保存训练结果
        private void SaveTrainFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.OverwritePrompt = true;//询问是否覆盖
            saveFileDialog.Filter = "*.train|*.train";
            saveFileDialog.DefaultExt = "train";//缺省默认后缀名
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                bp.SaveParaToFile(fileName);
            }
        }
    }
}
