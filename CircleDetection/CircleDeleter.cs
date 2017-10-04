using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CircleDetection
{
    public class CircleDeleter
    {
        Point center;        
        int radius;
        public readonly Image<Gray, byte> grayImage;
        public readonly int Rows, Cols;

        public CircleDeleter(Image<Gray, byte> image, int centerX, int centerY, int rad)
        {
            this.center = new Point(centerX, centerY);
            this.radius = rad;
            this.grayImage = image;
            LineDeleter.DeleteHorizonatalLines(this.grayImage);
            this.Rows = this.grayImage.Rows;
            this.Cols = this.grayImage.Cols;
        }

        public Image<Gray, Byte> deleteCircle()
        {
            Image<Gray, Byte> outputImage = new Image<Gray, byte>(this.grayImage.Rows, this.grayImage.Cols);
            Image<Gray, Byte> tmp = this.grayImage.Copy();
            this.correctErrors();
            CvInvoke.Circle(outputImage, this.center, this.radius, new MCvScalar(255), -1);
            // CvInvoke.BitwiseNot(tmp, tmp);
            CvInvoke.BitwiseAnd(outputImage, tmp, outputImage);
            return outputImage;
        }

        private void correctErrors()
        {
            Image<Gray, Byte> temp = this.grayImage.Copy();
            Image<Gray, Byte> temp1 = this.grayImage.Copy();
            Image<Gray, Byte> temp2 = this.grayImage.Copy();
            int tempRadius = this.radius - 10;
            //int[] radii = new int[8];
            //radii[0] = this.serachEdgePixel(this.center.X + tempRadius, this.center.Y, 1, 0);
            //radii[1] = this.serachEdgePixel(this.center.X + tempRadius, this.center.Y + tempRadius, 1, 1);
            //radii[2] = this.serachEdgePixel(this.center.X, this.center.Y + tempRadius, 0, 1);
            //radii[3] = this.serachEdgePixel(this.center.X - tempRadius, this.center.Y + tempRadius, -1, 1);
            //radii[4] = this.serachEdgePixel(this.center.X - tempRadius, this.center.Y, -1, 0);
            //radii[5] = this.serachEdgePixel(this.center.X - tempRadius, this.center.Y - tempRadius, -1, -1);
            //radii[6] = this.serachEdgePixel(this.center.X, this.center.Y - tempRadius, 0, -1);
            //radii[7] = this.serachEdgePixel(this.center.X + tempRadius, this.center.Y - tempRadius, 1, -1);
            //Array.Sort(radii);
            //this.radius = radii[0];     
            int count;
            while (tempRadius < this.radius)
            {
                count = drawCircle(temp, this.center, tempRadius);
                double ratio = count / (2 * Math.PI * tempRadius);
                if (ratio >= 0.1)
                {
                    //    CvInvoke.Circle(temp1, this.center, this.radius, new MCvScalar(255), 1);
                    //  CvInvoke.Circle(temp2, this.center, tempRadius, new MCvScalar(255), 1);
                    this.radius = tempRadius;
                    //ImageView.imShow(temp1, temp2);
                }
                //  Console.WriteLine(ratio.ToString() + " Found Ratio\n\n");
                tempRadius++;
            }
        }

        private int serachEdgePixel(int startX, int startY, int directionX, int directionY)
        {
            int tempRadius = this.radius - 5;
            int X = startX;
            int Y = startY;
            while (tempRadius < this.radius)
            {
                if ((X >= 0 && X < this.Rows) && (Y >= 0 && Y < this.Cols))
                {
                    if (this.grayImage[X, Y].Intensity == 255)
                        return tempRadius;
                    X += directionX;
                    Y += directionY;
                }
                tempRadius++;
            }
            return this.radius;
        }

        public static int drawCircle(Image<Gray, Byte> image, Point cen, int rad)
        {
            int X = cen.X;
            int Y = cen.Y;
            int x = rad;
            int y = 0;
            int err = 0;
            int count = 0;
            while (x >= y)
            {
                count += putCircle(image, X, Y, x, y);

                y++;
                err += 1 + 2 * y;
                if (2*(err - x) + 1 <= 0) continue;
                x -= 1;
                err += 1 - 2 * x;
            }
            //Console.WriteLine(count.ToString() + " points found for radius " + rad.ToString() + " but actual Count should be "
            //    + ((int)(2 * Math.PI * rad)).ToString());
            return count;
        }

        private static int putCircle(Image<Gray, Byte> image, int X, int Y, int x, int y)
        {
            int indX, indY;
            int count = 0;
            indX = X + x;
            indY = Y + y;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            indX = X + y;
            indY = Y + x;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            indX = X - y;
            indY = Y + x;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            indX = X - x;
            indY = Y + y;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            indX = X - x;
            indY = Y - y;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            indX = X - y;
            indY = Y - x;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            indX = X + y;
            indY = Y - x;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            indX = X + x;
            indY = Y - y;
            if (0 <= indX && 0 <= indY && indX < image.Rows && indY < image.Cols)
                if (image[indX, indY].Intensity == 255)
                    count++;
            return count;
        }
    }
}
