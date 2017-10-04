using System;
using System.Configuration;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CircleDetection
{
  public static class CircleValidation
    {
        private static readonly  int RadiusPadding = ConfigurationManager.AppSettings["CirclePadding"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["CirclePadding"]) : 0;
        public static bool IsValid(Image<Gray, byte> image)
        {            
            bool result = false;
            try
            {
                Image<Gray, byte> img = image.Clone();
                int radius = img.Width/2 - RadiusPadding;            
                using (Edge edge = new Edge(img, true, radius))
                {
                    RandomizedCircleDetection randCircleDetect = new RandomizedCircleDetection(edge, 30, 250, 30, 1,
                        0.01, 0.08, 0.55, EdgeType.CANNY_EDGE, false);                   
                    result = randCircleDetect.ContainsCircle();
                }

            }
            catch (Exception errException)
            {
                Console.WriteLine(errException.Message);
            }

            return result;
        }
    }

}
