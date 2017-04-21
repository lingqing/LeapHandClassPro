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
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            //this.notifyCount.Text = "20";
            // 应用下拉日历样式
            TextBox tb = new TextBox();
            //if (sCalendar != null)
            //    this.datePick.Style = sCalendar;
            this.pageContainer.Source = new Uri("pages/Home_page.xaml", UriKind.RelativeOrAbsolute);
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }



        private void Logout_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

            this.Close();
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

            this.pageContainer.Source = new Uri("pages/Page_Chart.xaml", UriKind.RelativeOrAbsolute);

        }
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

        private void Label_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            this.pageContainer.Source = new Uri("pages/Page_Chart2.xaml", UriKind.RelativeOrAbsolute);
        }

        private void TabItem_MouseMove_1(object sender, MouseEventArgs e)
        {
            //var part_text= this.LeftTabControl.Template.FindName("PART_Text", this.LeftTabControl);
            //null
        }
        // 数据采集界面   
        private void Slider_Collect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.pageContainer.Source = new Uri("pages/DataCollect.xaml", UriKind.RelativeOrAbsolute);
            _debug.Content = "prewleftclick";
        }
        // 数据训练界面  
        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.pageContainer.Source = new Uri("pages/DataTrain.xaml", UriKind.RelativeOrAbsolute);
            _debug.Content = "train leftclick";
        }
        // Home
        private void HomeTab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.pageContainer.Source = new Uri("pages/Home_page.xaml", UriKind.RelativeOrAbsolute);
        }

        // Prefect
        private void PrefectTab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.pageContainer.Source = new Uri("pages/Prefect_page.xaml", UriKind.RelativeOrAbsolute);
        }
    }
}
