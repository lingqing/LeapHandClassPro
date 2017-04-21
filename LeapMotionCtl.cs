using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using System.IO;

namespace LeapMotionPro
{
    public static class LeapMotionCtl
    {
        // 常量
        private const int paraNum = 20;     // 参数个数

        private static Controller controller = new Controller();
        private static Frame frame;

        // 采集数据
        public static List<float[]> LeapGrabData()
        {
            List<float[]> grabDataList = new List<float[]>();
            
            for (int i = 0; i < /*times*/5; i++)
            {
                float[] grabData = LeapGrabSingleData();
                if (grabData !=null)
                {
                    grabDataList.Add(grabData);
                }
            }
            return grabDataList;
            // Todo 判断离群向量            
        }
        public static float[] LeapGrabSingleData()
        {
            frame = controller.Frame();
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
                            anglePara[2] = vInte.AngleTo(vProx);
                            anglePara[3] = vDist.AngleTo(vInte);
                            break;
                        case Finger.FingerType.TYPE_INDEX:                            
                            anglePara[4] = vProx.AngleTo(vPalmD);
                            anglePara[5] = vProx.AngleTo(vMeta);
                            anglePara[6] = vInte.AngleTo(vProx);
                            anglePara[7] = vDist.AngleTo(vInte);                            
                            break;
                        case Finger.FingerType.TYPE_MIDDLE:                            
                            anglePara[8] = vProx.AngleTo(vPalmD);
                            anglePara[9] = vProx.AngleTo(vMeta);
                            anglePara[10] = vInte.AngleTo(vProx);
                            anglePara[11] = vDist.AngleTo(vInte);
                            break;
                        case Finger.FingerType.TYPE_RING:                            
                            anglePara[12] = vProx.AngleTo(vPalmD);
                            anglePara[13] = vProx.AngleTo(vMeta);
                            anglePara[14] = vInte.AngleTo(vProx);
                            anglePara[15] = vDist.AngleTo(vInte);
                            break;
                        case Finger.FingerType.TYPE_PINKY:                            
                            anglePara[16] = vProx.AngleTo(vPalmD);
                            anglePara[17] = vProx.AngleTo(vMeta);
                            anglePara[18] = vInte.AngleTo(vProx);
                            anglePara[19] = vDist.AngleTo(vInte);
                            break;
                    }                    
                } // end of finger foreach                  
                return anglePara;
            }
            return null;
        } // end of function grabsingedate

    }

    

}
