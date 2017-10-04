using System;
using System.Collections.Generic;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Structure;

namespace CircleDetection
{
    /// <summary>
    /// This class determines Circle attributes of given Edge Object using Randomized Algorithm
    /// </summary>
    public class RandomizedCircleDetection
    {
        protected int CurrentTestNo;
        protected const int MaxTests = 3;
        protected Edge EdgeObject;
        protected bool DetectMultiCircles;
        protected List<Point> Edges1Q, Edges2Q, Edges3Q, Edges4Q;
        protected int Tmin, Tf, Ta;
        protected double Td;
        protected double LambdaThreshold;
        protected List<Point> EdgePointsList;
        protected double MinEdgeDensity, MaxEdgeDensity;
        protected Image<Gray, byte> BinaryEdgeImage;
        protected readonly double imageDensity;
        public int MinEdgePtsThres => this.Tmin;
        protected int failCount = 0 ;
        public int FailCount => this.failCount;
        public double ImageDensity => this.imageDensity;
        public int FailThres => this.Tf;

        public int MinDistBwnPts => this.Ta;

        public double MinRadiusErrorThres => this.Td;

        public RandomizedCircleDetection(Edge edge, int minEdgePointsThreshold, int failThreshold,
            int minDistBetweenPoints, double minRadiusErrorThreshold, double minimumEdgesDensity, double maximumEdgesDensity, double circlecircumfrenceRange, EdgeType edgeType, bool hasMultipleCircles = true)
        {
            this.EdgeObject = edge;
            if (edgeType == EdgeType.CANNY_EDGE)
                this.BinaryEdgeImage = edge.DetectCannyEdges();
            else if (edgeType == EdgeType.MORPHOLOGICAL_THINNING)
                this.BinaryEdgeImage = edge.DetectThinnedEdges();
            else if (edgeType == EdgeType.ZHANG_SUEN_THINNING)
                this.BinaryEdgeImage = edge.DetectZhangSuenThinnedEdges();
            this.Edges1Q = edge.Get1QEdgePointsList();
            this.Edges2Q = edge.Get2QEdgePointsList();
            this.Edges3Q = edge.Get3QEdgePointsList();
            this.Edges4Q = edge.Get4QEdgePointsList();
            this.DetectMultiCircles = hasMultipleCircles;
            this.EdgePointsList = new List<Point>();
            this.EdgePointsList.AddRange(this.Edges1Q);
            this.EdgePointsList.AddRange(this.Edges2Q);
            this.EdgePointsList.AddRange(this.Edges3Q);
            this.EdgePointsList.AddRange(this.Edges4Q);
            this.Tmin = minEdgePointsThreshold;
            this.Ta = minDistBetweenPoints;
            this.Tf = failThreshold;
            this.Td = minRadiusErrorThreshold;
            this.LambdaThreshold = circlecircumfrenceRange;
            this.MinEdgeDensity = minimumEdgesDensity;
            this.MaxEdgeDensity = maximumEdgesDensity;
            this.CurrentTestNo = 0;
            this.imageDensity = (double)this.EdgeObject.GetEdgePointsCount() / (double)(this.EdgeObject.Rows * this.EdgeObject.Cols);
        }

        protected Point PossibleCircleCenter(Point p1, Point p2, Point p3)
        {
            int x1Sq = p1.X * p1.X;
            int x2Sq = p2.X * p2.X;
            int x3Sq = p3.X * p3.X;
            int y1Sq = p1.Y * p1.Y;
            int y2Sq = p2.Y * p2.Y;
            int y3Sq = p3.Y * p3.Y;

            int a00 = x2Sq + y2Sq - (x1Sq + y1Sq);
            int a01 = 2 * (p2.Y - p1.Y);
            int a10 = x3Sq + y3Sq - (x1Sq + y1Sq);
            int a11 = 2 * (p3.Y - p1.Y);

            int b00 = 2 * (p2.X - p1.X);
            int b01 = x2Sq + y2Sq - (x1Sq + y1Sq);
            int b10 = 2 * (p3.X - p1.X);
            int b11 = x3Sq + y3Sq - (x1Sq + y1Sq);

            int denom = 4 * ((p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y));
            if (denom == 0)
                denom = 1;
            int x = ((a00 * a11) - (a01 * a10)) / denom;
            int y = ((b00 * b11) - (b01 * b10)) / denom;

            return new Point(x, y);
        }

        protected double DistanceBetweenTwoPoints(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        protected bool IsDistanceBetweenPointsGreaterThanTa(Point p1, Point p2, Point p3, int Ta)
        {
            if (this.DistanceBetweenTwoPoints(p1, p2) < Ta)
                return false;
            else if (this.DistanceBetweenTwoPoints(p2, p3) < Ta)
                return false;
            else if (this.DistanceBetweenTwoPoints(p3, p1) < Ta)
                return false;
            else
                return true;
        }

        protected bool IsCollinear(Point p1, Point p2, Point p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.Y) * (p2.Y - p1.Y) == 0;
        }

        protected List<Point> ChooseFourSemiRandomPoints(bool onlyThree = false)
        {
            int index;
            List<Point> points = new List<Point>();
            int minEdgePtsCount = (int)(this.MinEdgeDensity * this.BinaryEdgeImage.Rows * this.BinaryEdgeImage.Cols);
            Random randint = new Random();
            if (this.Edges1Q.Count >= minEdgePtsCount / 4)
            {
                index = randint.Next(0, this.Edges1Q.Count-1);
                points.Add(this.Edges1Q[index]);             
                this.Edges1Q.RemoveAt(index);
            }

            if (this.Edges2Q.Count >= minEdgePtsCount / 4)
            {
                index = randint.Next(0, this.Edges2Q.Count-1);
                points.Add(this.Edges2Q[index]);
                this.Edges2Q.RemoveAt(index);
            }

            if (this.Edges3Q.Count >= minEdgePtsCount / 4)
            {
                index = randint.Next(0, this.Edges3Q.Count-1);
                points.Add(this.Edges3Q[index]);
                this.Edges3Q.RemoveAt(index);
            }
            if (onlyThree)
                return points;

            if (this.Edges4Q.Count < minEdgePtsCount / 4) return points;
            index = randint.Next(0, this.Edges4Q.Count-1);
            points.Add(this.Edges4Q[index]);
            this.Edges4Q.RemoveAt(index);            

            return points;
        }


        protected List<Point> ChooseFourRandomPoints(bool chooseOnlyThree = false)
        {
       
            List<Point> points = new List<Point>();
            Random random = new Random();        
            var index = random.Next(0, this.EdgePointsList.Count - 1);
            points.Add(this.EdgePointsList[index]);
            this.EdgePointsList.RemoveAt(index);


            index = random.Next(0, this.EdgePointsList.Count - 1);
            points.Add(this.EdgePointsList[index]);
            this.EdgePointsList.RemoveAt(index);

            index = random.Next(0, this.EdgePointsList.Count - 1);
            points.Add(this.EdgePointsList[index]);
            this.EdgePointsList.RemoveAt(index);

            if (chooseOnlyThree)
                return points;            
            index = random.Next(0, this.EdgePointsList.Count - 1);
            points.Add(this.EdgePointsList[index]);
            this.EdgePointsList.RemoveAt(index);

          
            return points;
        }

        public virtual List<int[]> DetectCircles()
        {
            this.CurrentTestNo++;
            List<int[]> circles = new List<int[]>();
            int rows = this.EdgeObject.Rows;
            int cols = this.EdgeObject.Cols;
            this.failCount = 0;
            if (this.imageDensity > this.MinEdgeDensity && this.imageDensity < this.MaxEdgeDensity)
            {
            
                while (failCount < this.Tf && this.EdgePointsList.Count > this.Tmin)
                {               
                    var failed = false;
                    List<Point> points = this.ChooseFourSemiRandomPoints();
                    //ImageView.printList<Point>(points);
                    if (points.Count < 4)
                        return circles;                    
                    
                    if (!this.IsCollinear(points[0], points[1], points[2]) && this.IsDistanceBetweenPointsGreaterThanTa(points[0], points[1], points[2], this.Ta))
                    {                        
                        Point center = this.PossibleCircleCenter(points[0], points[1], points[2]);
                        int radius = (int)this.DistanceBetweenTwoPoints(center, points[0]);
                        if (Math.Abs(this.DistanceBetweenTwoPoints(center, points[3]) - radius) <= this.Td)
                        {
                            int count = 0;
                            int np = Convert.ToInt32(this.LambdaThreshold * 2 * Math.PI * radius);
                            count = this.PixelVoter(this.Edges1Q, center, radius, np, count);
                            count = this.PixelVoter(this.Edges2Q, center, radius, np, count);
                            count = this.PixelVoter(this.Edges3Q, center, radius, np, count);
                            count = this.PixelVoter(this.Edges4Q, center, radius, np, count);
                            if (!this.DetectMultiCircles)
                            {
                                if (count >= np)
                                {
                                    int[] circle = new int[3];
                                    circle[0] = center.X;
                                    circle[1] = center.Y;
                                    circle[2] = radius;
                                    circles.Add(circle);
                                    return circles;
                                }
                            }

                            if (count >= np)
                            {
                                int[] circle = new int[3];
                                circle[0] = center.X;
                                circle[1] = center.Y;
                                circle[2] = radius;
                                circles.Add(circle);
                            }
                            else
                            {
                                failed = true;
                            }
                        }
                        else
                        {
                            failed = true;
                        }
                    }
                    else
                    {
                        failed = true;
                    }

                    if (!failed) continue;
                    this.failCount++;
                    this.Edges1Q.Add(points[0]);
                    this.Edges2Q.Add(points[1]);
                    this.Edges3Q.Add(points[2]);
                    this.Edges4Q.Add(points[3]);                    
                }
            }
            if (this.CurrentTestNo <= MaxTests && circles.Count == 0)
            {
                return this.DetectCircles();
            }



            return circles;
        }

        public virtual bool ContainsCircle()
        {
            List<int[]> circles = this.DetectCircles();
            return circles.Count > 0;
        }

        public Edge GetEdgeObject()
        {
            return this.EdgeObject;
        }
        protected int PixelVoter(List<Point> points, Point center, int radius, int np, int presentCount)
        {
            
            foreach (var t in points)
            {
                if (Math.Abs(this.DistanceBetweenTwoPoints(t, center) - radius) <= this.Td)
                {                
                    presentCount++;
                }
            }
            return presentCount;
        }
    }
}
