using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LeapMotionPro
{
    /**
     * 手型类，包含索引和名称
     * */
    class HandType
    {
        public int index{get; set;}
        public string name{get; set;}
    }
    /**
     * 手型集合
     * */
    
    class HandTypeManager
    {
        public static List<HandType> GetHandType()
        {
            var hands = new List<HandType>();
            using (StreamReader sr = new StreamReader("conf/handdef.type"))
            {
                String line;
                for (int i = 0; (line = sr.ReadLine())!= null; i++)
                {
                    hands.Add(new HandType {index = i,name=line });
                }
                sr.Close();
            }
            return hands;
        }        
    }
}
