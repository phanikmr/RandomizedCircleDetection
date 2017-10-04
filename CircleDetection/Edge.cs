using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace CircleDetection
{
    public class Edge : IDisposable
    {
        private bool _disposed = false;
        private readonly Image<Bgr, byte> _originalImage;
        private double _scale;
        private readonly Image<Gray, byte> _grayImage;
        private Image<Gray, byte> _blurImage;
        private Image<Gray, byte> _edgeImage;
        private Image<Gray, byte> _binaryEdgeImage;
        private readonly bool _removeGrabageInside;
        private Size _size;
        private readonly int _cenx;
        private readonly int _ceny;
        private readonly int? _probableRadius;
        public int Rows { get; }

        public int Cols { get; }

        public Edge(Mat image, bool removePixelsInside = false,int? probableRadius=null, double scaleFactor = 1.0) : this(image.ToImage<Bgr, byte>(), removePixelsInside,probableRadius, scaleFactor)
        {
        }

        public Edge(Image<Bgr, byte> image, bool removePixelsInside = false, int? probableRadius = null, double scaleFactor = 1.0)
        {
            this._originalImage = image;
            this._disposed = false;
            this._scale = scaleFactor;
            this._grayImage = new Image<Gray, Byte>(this._originalImage.Rows, this._originalImage.Cols);
            CvInvoke.CvtColor(this._originalImage, this._grayImage, ColorConversion.Bgr2Gray);
            this._size = this._grayImage.Size;
            this.Rows = this._grayImage.Rows;
            this.Cols = this._grayImage.Cols;
            this._cenx = this._grayImage.Rows / 2;
            this._ceny = this._grayImage.Cols / 2;
            this._removeGrabageInside = removePixelsInside;
            this._probableRadius = probableRadius;
        }

        public Edge(Image<Gray, byte> image, bool removePixelsInside = false, int? probableRadius = null,
            double scaleFactor = 1.0)
        {
            this._originalImage = new Image<Bgr, byte>(new Image<Gray, byte>[3] {image,image,image});
            this._grayImage = image;         
            this._size = this._grayImage.Size;
            this.Rows = this._grayImage.Rows;
            this.Cols = this._grayImage.Cols;
            this._cenx = this._grayImage.Rows / 2;
            this._ceny = this._grayImage.Cols / 2;
            this._removeGrabageInside = removePixelsInside;
            this._probableRadius = probableRadius;

        }
            


        public Edge(string imagePath, bool removePixelsInside = false, int? probableRadius = null, double scaleFactor = 1.0)
            : this(CvInvoke.Imread(imagePath, LoadImageType.AnyColor), removePixelsInside, probableRadius, scaleFactor)
        {
        }


        public void RemoveNoise()
        {
            this._blurImage = new Image<Gray, byte>(this._grayImage.Rows, this._grayImage.Cols);
            CvInvoke.MedianBlur(this._grayImage, this._blurImage, 3);
        }

        public Image<Gray, byte> DetectThinnedEdges()
        {
            this._edgeImage = new Image<Gray, byte>(this._grayImage.Rows, this._grayImage.Cols);
            CvInvoke.Threshold(this._grayImage, this._edgeImage, 127, 255, ThresholdType.BinaryInv);
            //ImageView.imShow(this.grayImage);
            if (this._edgeImage.CountNonzero()[0] > 15000)
            {
                this._binaryEdgeImage = new Image<Gray, byte>(this._edgeImage.Rows, this._edgeImage.Cols, new Gray(0));
                return this._binaryEdgeImage.Copy();
            }
            //    this.edgeImage = this.removeGarbageInsideCircle(this.edgeImage, 0.65);
            this._binaryEdgeImage = this._edgeImage.Copy();
            if (this._removeGrabageInside)
                this._binaryEdgeImage = this.RemoveGarbageInsideCircle(this._binaryEdgeImage, 0.65);
            Thinning thinning = new Thinning(this._binaryEdgeImage);
            this._binaryEdgeImage = thinning.BwThinning();
            //this.binaryEdgeImage = thinning.zhangSuenThinning();            
            return this._binaryEdgeImage.Copy();
        }

        public Image<Gray, byte> DetectZhangSuenThinnedEdges()
        {
            this._edgeImage = new Image<Gray, byte>(this._grayImage.Rows, this._grayImage.Cols);
            CvInvoke.Threshold(this._grayImage, this._edgeImage, 127, 255, ThresholdType.BinaryInv);
            //ImageView.imShow(this.grayImage);
            if (this._edgeImage.CountNonzero()[0] > 15000)
            {
                this._binaryEdgeImage = new Image<Gray, byte>(this._edgeImage.Rows, this._edgeImage.Cols, new Gray(0));
                return this._binaryEdgeImage.Copy();
            }
            //  this.edgeImage = this.removeGarbageInsideCircle(this.edgeImage, 0.65);
            this._binaryEdgeImage = this._edgeImage.Copy();
            if (this._removeGrabageInside)
                this._binaryEdgeImage = this.RemoveGarbageInsideCircle(this._binaryEdgeImage, 0.65);
            Thinning thinning = new Thinning(this._binaryEdgeImage);
            // this.binaryEdgeImage = thinning.bwThinning();
            this._binaryEdgeImage = thinning.ZhangSuenThinning();
            return this._binaryEdgeImage.Copy();
        }

        public Image<Gray, byte> DetectCannyEdges()
        {
            this._edgeImage = new Image<Gray, byte>(this._grayImage.Rows, this._grayImage.Cols);
            CvInvoke.Canny(this._grayImage, this._edgeImage, 100, 200);
            if (this._edgeImage.CountNonzero()[0] > 15000)
            {
                this._binaryEdgeImage = new Image<Gray, byte>(this._edgeImage.Rows, this._edgeImage.Cols, new Gray(0));
                return this._binaryEdgeImage.Copy();
            }
            this._binaryEdgeImage = new Image<Gray, byte>(this.Rows, this.Cols);
            CvInvoke.Threshold(this._edgeImage, this._binaryEdgeImage, 127, 255, ThresholdType.Binary);
            if (this._removeGrabageInside)
                this._binaryEdgeImage = this.RemoveGarbageInsideCircle(this._binaryEdgeImage, 0.65);
            return this._binaryEdgeImage.Copy();
        }

        private Image<Gray, byte> RemoveGarbageInsideCircle(Image<Gray, byte> image, double percentage)
        {
            Image<Gray, byte> outputImage = image.Copy();
            Image<Gray, byte> frame = new Image<Gray, byte>(image.Width, image.Height, new Gray(255));
            
            int rows = this._probableRadius??Convert.ToInt32(image.Rows / 2);          
            rows = Convert.ToInt32(percentage*rows);
         //   int cols = Convert.ToInt32(percentage * image.Cols / 2);
            for (int i = this._cenx - rows; i < this._cenx + rows; i++)
            {
                for (int j = this._ceny - rows; j < this._ceny + rows; j++)
                {
                    frame[i, j] = new Gray(0);
                }
            }

            CvInvoke.BitwiseAnd(outputImage, frame, outputImage);
            this._binaryEdgeImage = outputImage;          
            return outputImage;
        }

        public int GetEdgePointsCount()
        {
            return this._binaryEdgeImage.CountNonzero()[0];
        }

        public List<Point> Get1QEdgePointsList()
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < this._cenx; i++)
            { 
                for (int j = 0; j < this._ceny; j++)
                {
                    if (this._binaryEdgeImage[i, j].Intensity == (double) 255)
                    {
                        points.Add(new Point(i, j));
                    }
                }
            }
            return points;
        }

        public List<Point> Get2QEdgePointsList()
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < this._cenx; i++)
            {
                for (int j = this._ceny; j < this.Cols; j++)
                {
                    if (this._binaryEdgeImage[i, j].Intensity == 255)
                    {
                        points.Add(new Point(i, j));
                    }
                }
            }
            return points;

        }

        public List<Point> Get3QEdgePointsList()
        {
            List<Point> points = new List<Point>();
            for (int i = this._cenx; i < this.Rows; i++)
            {
                for (int j = 0; j < this._ceny; j++)
                {
                    if (this._binaryEdgeImage[i, j].Intensity == 255)
                    {
                        points.Add(new Point(i, j));
                    }
                }
            }
            return points;
        }

        public List<Point> Get4QEdgePointsList()
        {
            List<Point> points = new List<Point>();
            for (int i = this._cenx; i < this.Rows; i++)
            {
                for (int j = this._ceny; j < this.Cols; j++)
                {
                    if (this._binaryEdgeImage[i, j].Intensity == 255)
                    {
                        points.Add(new Point(i, j));
                    }
                }
            }
            return points;
        }

        public List<Point> GetEdgePointsList()
        {
            List<Point> points = new List<Point>();
            points.AddRange(this.Get1QEdgePointsList());
            points.AddRange(this.Get2QEdgePointsList());
            points.AddRange(this.Get3QEdgePointsList());
            points.AddRange(this.Get4QEdgePointsList());
            return points;
        }

        private void ApplyMorphologicalGradient()
        {
            Mat element = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(3, 3), new Point(-1, -1));
            CvInvoke.MorphologyEx(this._edgeImage, this._binaryEdgeImage, MorphOp.Gradient, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

        }

        public Image<Bgr, byte> GetOriginalImage()
        {
            return this._originalImage.Copy();
        }
        public Image<Gray, byte> GetGrayImage()
        {
            return this._grayImage.Copy();
        }
        public Image<Gray, byte> GetBlurImage()
        {
            return this._blurImage.Copy();
        }
        public Image<Gray, byte> GetEdgeImage()
        {
            return this._edgeImage.Copy();
        }
        public Image<Gray, byte> GetBinaryEdgeImage()
        {
            return this._binaryEdgeImage.Copy();
        }

        
        public void Dispose()
        {
            this.Dispose(this._disposed);
            GC.SuppressFinalize(this);

        }

        protected virtual void Dispose(bool disposed)
        {
            if(disposed)
                return;
            this._binaryEdgeImage?.Dispose();
            this._blurImage?.Dispose();
            this._edgeImage?.Dispose();
            this._grayImage?.Dispose();
            this._originalImage?.Dispose();
            this._disposed = true;
        }

        ~Edge()
        {
            this.Dispose(false);
        }
    }
}
