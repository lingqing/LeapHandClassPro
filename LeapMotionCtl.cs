using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Collections.Generic;
using System.Windows;
//using System.Drawing;

namespace LeapMotionPro
{
    public class LeapMotionInnerCtl
    {
        private const int paraNum = 21;     // 参数个数

        private Controller controller = new Controller();
        private Frame frame;
        private bool isNewFrame = false;
        private bool isNewImage = true;
        // 图像
        private WriteableBitmap bitmap;
        private byte[] imagedata = new byte[1];
        private byte[] data = new byte[1];

        // 初始化
        public LeapMotionInnerCtl()
        {
            List<Color> grayscale = new List<Color>();
            for (byte i = 0; i < 0xff; i++)
            {
                grayscale.Add(System.Windows.Media.Color.FromArgb(0xff, i, i, i));
            }
            BitmapPalette palette = new BitmapPalette(grayscale);
            bitmap = new WriteableBitmap(640, 480, 72, 72, PixelFormats.Gray8, palette);
            controller.FrameReady += newFrameHandler;
            controller.ImageReady += onImageReady;
            controller.ImageRequestFailed += onImageRequestFailed;
        }       
        // 新帧到位
        private void newFrameHandler(object sender, FrameEventArgs eventArgs)
        {
            frame = eventArgs.frame;            
            controller.RequestImages(frame.Id, Leap.Image.ImageType.DEFAULT, data);
            // 自定义图片裁剪拉伸
            if (data.Length == 307200)
            {
                for (int i = 0; i < 479; i++)
                {
                    for (int j = 0; j < 640; j++)
                    {
                        if (i % 2 == 0)
                        {
                            imagedata[i * 640 + j] = data[(239 + i / 2) * 640 + j]; // 偶数行不变
                        }
                        else
                        {
                            imagedata[i * 640 + j] = (byte)((data[(239 + i / 2) * 640 + j] 
                                + data[(239 + (i+1) / 2) * 640 + j]) / 2); // 偶数行不变
                        }
                    }
                }
            }
            isNewFrame = true;
        }
        // 
        void onImageRequestFailed(object sender, ImageRequestFailedEventArgs e)
        {
            if (e.reason == Leap.Image.RequestFailureReason.Insufficient_Buffer)
            {
                data = new byte[e.requiredBufferSize];
                imagedata = new byte[e.requiredBufferSize];
            }           
        }

        // 图片到位
        private void onImageReady(object sender, ImageEventArgs e)
        {
            bitmap.WritePixels(new Int32Rect(0, 0, 640, 480), imagedata, 640, 0);
            isNewImage = true;
        }
        // 获取图像
        public WriteableBitmap GetImage()
        {
            if (!isNewImage) return null;
            isNewImage = false;
            return bitmap;
        }
        // 采集多组数据
        public List<float[]> LeapGrabData(int num)
        {
            List<float[]> grabDataList = new List<float[]>();

            for (int i = 0; i < num; i++)
            {
                float[] grabData = LeapGrabSingleData();
                if (grabData != null)
                {
                    grabDataList.Add(grabData);
                }
            }
            return grabDataList;
            //判断离群向量 → 放在上层代码中实现 已完成        
        }
        public float[] LeapGrabSingleData()
        {
            if (!isNewFrame) return null;
            isNewFrame = true;
            // 本程序只处理单手
            foreach (Hand hand in frame.Hands)
            {
                //if (hand.IsLeft != isLeft) break;

                float[] anglePara = new float[paraNum];
                float grabAngle = hand.GrabAngle;

                // Get fingers: 获得评价参数——20个
                Leap.Vector vPalmN = hand.PalmNormal;   // 手掌法向量 
                Leap.Vector vPalmD = hand.Direction;    // 手掌方向向量
                foreach (Finger finger in hand.Fingers)
                {
                    Leap.Vector vMeta, vProx, vInte, vDist; // 四个指节
                    vMeta = finger.Bone(Bone.BoneType.TYPE_METACARPAL).Direction;
                    vProx = finger.Bone(Bone.BoneType.TYPE_PROXIMAL).Direction;
                    vInte = finger.Bone(Bone.BoneType.TYPE_INTERMEDIATE).Direction;
                    vDist = finger.Bone(Bone.BoneType.TYPE_DISTAL).Direction;

                    //string disStr = "id:" + finger.Id + ", length=" + finger.Length;
                    switch (finger.Type)
                    {
                        case Finger.FingerType.TYPE_THUMB:
                            anglePara[0] = vProx.AngleTo(vPalmN);
                            anglePara[1] = vProx.AngleTo(vPalmD);
                            anglePara[2] = vInte.AngleTo(vPalmN);
                            anglePara[3] = vInte.AngleTo(vPalmD);                            
                            anglePara[4] = vDist.AngleTo(vInte);
                            Leap.Vector tip = finger.TipPosition;
                            break;
                        case Finger.FingerType.TYPE_INDEX:
                            anglePara[5] = vProx.AngleTo(vPalmD);
                            anglePara[6] = vProx.AngleTo(vMeta);
                            anglePara[7] = vInte.AngleTo(vProx);
                            anglePara[8] = vDist.AngleTo(vInte);
                            break;
                        case Finger.FingerType.TYPE_MIDDLE:
                            anglePara[9] = vProx.AngleTo(vPalmD);
                            anglePara[10] = vProx.AngleTo(vMeta);
                            anglePara[11] = vInte.AngleTo(vProx);
                            anglePara[12] = vDist.AngleTo(vInte);
                            break;
                        case Finger.FingerType.TYPE_RING:
                            anglePara[13] = vProx.AngleTo(vPalmD);
                            anglePara[14] = vProx.AngleTo(vMeta);
                            anglePara[15] = vInte.AngleTo(vProx);
                            anglePara[16] = vDist.AngleTo(vInte);
                            break;
                        case Finger.FingerType.TYPE_PINKY:
                            anglePara[17] = vProx.AngleTo(vPalmD);
                            anglePara[18] = vProx.AngleTo(vMeta);
                            anglePara[19] = vInte.AngleTo(vProx);
                            anglePara[20] = vDist.AngleTo(vInte);
                            break;
                    }
                } // end of finger foreach                  
                return anglePara;
            }
            return null;
        } // end of function grabsingedate
    }

    // 静态控制类
    public static class LeapMotionCtl
    {
        // 常量
        private static LeapMotionInnerCtl leapCtr = new LeapMotionInnerCtl();

        public static float[] LeapGrabSingleData()
        {
            return leapCtr.LeapGrabSingleData();
        }

        public static WriteableBitmap GetImage()
        {
            return leapCtr.GetImage();
        }


    }

    

}
