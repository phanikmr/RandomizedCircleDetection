using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace CircleDetection
{
    public class ImageView
    {
        public static void imShow(params IInputArray[] images)
        {
            int i = 0;
            foreach (IInputArray image in images)
            {
                Image<Bgr, Byte> temp = new Image<Bgr, byte>(new Size(500, 500));
                CvInvoke.Resize(image, temp, new Size(500, 500));
                CvInvoke.Imshow(i.ToString(), temp);
                i++;
            }
            CvInvoke.WaitKey(0);
        }

        public static void imshow(IInputArray image)
        {
            CvInvoke.Imshow("", image);
            CvInvoke.WaitKey(0);
        }

        public static void printList<T>(List<T> list)
        {
            foreach (T t in list)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine("\n");
        }

        public static void printImage(Image<Gray, Byte> image)
        {
            for (int i = 0; i < image.Rows; i++)
            {
                for (int j = 0; j < image.Cols; j++)
                {
                    Console.Write(image[i, j].Intensity);
                }
                Console.WriteLine();
            }
        }
    }
}
