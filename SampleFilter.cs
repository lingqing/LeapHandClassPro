using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapMotionPro
{
    public static class SampleFilter
    {
        public static List<float[]> CleraFaultVector(List<float[]> vecIn)
        {
            if(vecIn.Count <= 1) return null;

            int vecLength = vecIn[0].Length;  // 向量长度

            float[,] residual = new float[vecIn.Count,vecLength]; // 残差

            float[] avg = new float[vecLength];
            double[] sum = new double[vecLength];
            double[] variance = new double[vecLength];
            double[] standev = new double[vecLength]; // 标准差
            double[] variancesum = new double[vecLength]; // 标准差

            bool[] isGood = new bool[vecIn.Count];
            // 初始化为0
            for (int j = 0; j < vecLength; j++)
            {
                avg[j] = 0;
                sum[j] = 0;
                variance[j] = 0;
                standev[j] = 0;
                variancesum[j] = 0;                
            }
            // 求均值
            foreach (var vec in vecIn)
            {
                for (int j = 0; j < vecLength; j++)
                {
                    sum[j] += vec[j]; 
                }
            }
            for (int j = 0; j < vecLength; j++)
            {
                avg[j] = (float)(sum[j] / vecIn.Count);
            }
            // 求残差
            for (int i = 0; i < vecIn.Count; i++)
            {
                for (int j = 0; j < vecLength; j++)
                {
                    residual[i, j] = Math.Abs(vecIn[i][j] - avg[j]);
                }
            }
            // 求标准差
            for (int j = 0; j < vecLength; j++)
            {
                variancesum[j] = 0;
                for (int i = 0; i < vecIn.Count; i++)
                {
                    variancesum[j] += residual[i, j] * residual[i, j];
                }
                variance[j] = variancesum[j] / (vecIn.Count-1);
                standev[j] = Math.Sqrt(variance[j]);
            }
            // 剔除坏点
            for (int i = 0; i < vecIn.Count; i++)
            {
                isGood[i] = true;
                for (int j = 0; j < vecLength; j++)
                {
                    if (residual[i,j] > 2 * standev[j])
                    {
                        isGood[i] = false; break;
                    }
                }
            }

            List<float[]> ret = new List<float[]>();
            for (int i = 0; i < vecIn.Count; i++)
            {
                if (isGood[i]) ret.Add(vecIn[i]);
            }

            return ret;
        }
    }
}
