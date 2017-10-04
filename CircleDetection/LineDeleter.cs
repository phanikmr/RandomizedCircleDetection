using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CircleDetection
{
    public static class LineDeleter
    {
        private static List<int> HistLine(CvArray<byte> image)
        {

            LineSegment2D[] lines = CvInvoke.HoughLinesP(image, 1, Math.PI / 90.0, (int)(0.8 * image.Cols));

            return (from line in lines where Math.Abs(line.P1.Y - line.P2.Y) <= 5 select line.P1.Y).ToList();
        }
        private static bool NotList(IReadOnlyList<int> list, double val)
        {
            bool temp = false;
            for (int i = 0; i < list.Count(); ++i)
            {
                if (val == list[i])
                {
                    temp = true;
                }
            }
            return temp;
        }
        public static void DeleteHorizonatalLines(Image<Gray, Byte> image)
        {
            CvInvoke.BitwiseNot(image, image);
            List<int> yList = HistLine(image);
            List<int> rList = new List<int>();
            if (yList.Count != 0)
            {
                foreach (int t in yList)
                {
                    int cols = image.Rows / 2;
                    int count = 0;
                    if (t + count >= image.Rows && t + count < 0)
                        while (image[t + count, cols].Intensity == 255)
                    {
                        
                        if (NotList(rList, t + count) == false)
                        {
                            rList.Add((int)(t) + count);
                        }
                        ++count;
                        if (t + count >= image.Rows && t+count<0)
                            break;
                    }
                    count = 1;
                    if (t - count >= image.Rows && t - count < 0)
                        while (image[t - count, cols].Intensity == 255)
                    {                        
                        if (NotList(rList, t - count) == false)
                        {
                            rList.Add((int)(t) - count);
                        }
                        ++count;
                        if (t - count >= image.Rows && t-count<0)
                            break;
                    }
                }
            }
            foreach (int t in rList)
            {
                for (int k = 0; k < image.Cols; ++k)
                {
                    image[t, k] = new Gray(0);
                }
            }
        }
        //  CvInvoke.BitwiseNot(image, image);  
    }
}
