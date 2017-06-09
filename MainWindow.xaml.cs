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

namespace LeapMotionPro
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    ///     

    enum pageType
    {
        Home,
        Prefect,
        Collect,
        Train,
    };
    // 静态监控类，用于修改状态栏
    public static class Monitor
    {
        public static event EventHandler<MessageArgs> PartEvent;//定义在Monitor中的一个事件，参数是string对象

        public static void ReportMsg(int index, string message)
        {            
            if (PartEvent != null)//如果mainwindow构造函数中给PartEvent注册了函数就不为null
            {
                var messageArg = new MessageArgs(index, message);
                PartEvent(null, messageArg);//触发事件，执行所有注册过的函数
            }
        }

        public static bool MonitorCenter(int index, string s)
        {
            ReportMsg(index, s);//在外部类中修改TextBlock的Text
            return true;
        }
    }
    // 传递参数
    public class MessageArgs : EventArgs
    {
        public MessageArgs(string message)
        {
            this.TxtMessage = message;
        }
        public MessageArgs(int index, string message)
        {
            this.MsgIndex = index;
            this.TxtMessage = message;
        }
        public string TxtMessage { get; set; }
        public int MsgIndex { get; set; }
    }

    public partial class MainWindow : Window
    {
        pages.DataCollect pageDataCollect = new pages.DataCollect();
        //pages.DataTrain pageDataTrain = new pages.DataTrain();   // v2.0删除
        pages.Prefect_page pagePrefect = new pages.Prefect_page();
        pages.Home_page pageHome = new pages.Home_page();

        pageType currentPageType;
        public MainWindow()
        {
            InitializeComponent();
            //this.notifyCount.Text = "20";
            // 应用下拉日历样式
            TextBox tb = new TextBox();
            //if (sCalendar != null)
            //    this.datePick.Style = sCalendar;
            // page 页           
            pageContainer.Navigate(pageHome);
            currentPageType = pageType.Home;

            // 状态改变监控
            Monitor.PartEvent += OnNewStateEvent;
        }
        
        // 状态改变操作
        public void OnNewStateEvent(Object sender, MessageArgs message)
        {
            switch (message.MsgIndex)
            {
                case 0:
                    ParaState.Content = message.TxtMessage;
                    break;
                case 1:
                    RunState.Content = message.TxtMessage;
                    break;
                default:
                    break;
            }  
        }


        // 拖拽窗口
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        

        private void Logout_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        // 导航栏
        private bool __isLeftHide = false;
        private void spliter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Left Bar hide and show
            __isLeftHide = !__isLeftHide;
            if (__isLeftHide)
            {
                this.gridForm.ColumnDefinitions[0].Width = new GridLength(1d);
            }
            else
            {
                this.gridForm.ColumnDefinitions[0].Width = new GridLength(100d);
            }

        }

        // page 导航
        // 导航至指定界面
        void PageNavigateTo(pageType type)
        {
            if (type != currentPageType)
            {
                switch (currentPageType)
                {
                    case pageType.Home:
                        pageHome.pagePause();
                    break;
                    case pageType.Collect:
                        pageDataCollect.pagePause();
                    break;
                    //case pageType.Train:
                    //    pageDataTrain.pagePause();
                    //break;
                    case pageType.Prefect:
                        pagePrefect.pagePause();
                    break;
                }
                switch (type)
                {
                    case pageType.Home:
                        pageHome.pageActive();
                        pageContainer.Navigate(pageHome);
                        break;
                    case pageType.Collect:
                        pageDataCollect.pageActive();
                        pageContainer.Navigate(pageDataCollect);
                        break;
                    //case pageType.Train:
                    //    pageDataTrain.pageActive();
                    //    pageContainer.Navigate(pageDataTrain);
                    //    break;
                    case pageType.Prefect:
                        pagePrefect.pageActive();
                        pageContainer.Navigate(pagePrefect);
                        break;
                }

                currentPageType = type;                
            }
        }
        // 数据采集界面   
        private void Slider_Collect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PageNavigateTo(pageType.Collect);     
        }
        //// 数据训练界面  
        //private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    PageNavigateTo(pageType.Train); 
        //}
        // Home
        private void HomeTab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PageNavigateTo(pageType.Home); 
        }

        // Prefect
        private void PrefectTab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PageNavigateTo(pageType.Prefect); 
        }
    }
}
