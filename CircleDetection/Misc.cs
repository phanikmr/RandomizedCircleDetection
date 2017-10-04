namespace CircleDetection
{
    class Misc
    {

        /*
        Image<Gray,Byte> skel = new Image<Gray, byte>(this.binaryEdgeImage.Size);
        Image<Gray,Byte> temp = new Image<Gray, byte>(this.binaryEdgeImage.Size);
        Image<Gray,Byte> erored = new Image<Gray, byte>(this.binaryEdgeImage.Size);
        Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

        //Image<Gray, byte> element = new Image<Gray, byte>(3,3);
        //element[0, 0] = new Gray(8);
        //element[0, 1] = new Gray(4);
        //element[0, 2] = new Gray(2);
        //element[1, 0] = new Gray(16);
        //element[1, 1] = new Gray(0);
        //element[1, 2] = new Gray(1);
        //element[2, 0] = new Gray(32);
        //element[2, 1] = new Gray(64);
        //element[2, 2] = new Gray(128);




        /*
      ///Skeltinization
        bool done = false;
        do
        {
            CvInvoke.MorphologyEx(this.binaryEdgeImage, temp, MorphOp.Open, element, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());
            CvInvoke.BitwiseNot(temp, temp);
            CvInvoke.BitwiseAnd(this.binaryEdgeImage, temp, temp);
            CvInvoke.BitwiseOr(skel, temp, skel);
            CvInvoke.Erode(this.binaryEdgeImage, this.binaryEdgeImage, element, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());
            int max;
            max = CvInvoke.CountNonZero(this.binaryEdgeImage);
            if (max == 0)
                done = true;
        } while (!done);
        this.binaryEdgeImage = skel;

*/



        //  CvInvoke.MorphologyEx(this.getEdgeImage(), this.binaryEdgeImage, MorphOp.Gradient, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
        //ImageView.imShow(this.binaryEdgeImage);
        //this.binaryEdgeImage = Thinning.ZhangSuenThinning(this.edgeImage);
        //  ImageView.imShow(this.binaryEdgeImage);   






        /*
        Image<Gray, Byte> img = new Image<Gray, byte>(edge.Cols, edge.Rows);
        Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
        CvInvoke.MorphologyEx(edge.getEdgeImage(), img, MorphOp.Gradient, element, new Point(-1, -1),1, BorderType.Default, new MCvScalar());
        ImageView.imShow(img);
        //bool[][] mat = new bool[][] {
        //    new bool[] {false,false,false,false,false},
        //     new bool[] {false,true,true,true,false},
        //     new bool[] {false,true,true,true,false},
        //     new bool[] {false,false,false,false,false},
        //     new bool[] {false,false,false,false,false}
        //};


        /*
        Image<Gray, Byte> mat = new Image<Gray, byte>(5, 5);
        Gray black = new Gray(0);
        Gray white = new Gray(255);
        mat.SetZero();
        mat[1, 1] = white;
        mat[1, 2] = white;
        mat[1, 3] = white;
        mat[2, 1] = white;
        mat[2, 2] = white;
        mat[2, 3] = white;
        mat = Thinning.ZhangSuenThinning(mat);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Console.Write(mat[i,j].ToString() + " ");
            }
            Console.WriteLine();
        }
        */


        /*
        Image<Gray, Byte> img = edge.getEdgeImage();
        img = Thinning.ZhangSuenThinning(img);
        ImageView.imShow(img);*/

    }
}
