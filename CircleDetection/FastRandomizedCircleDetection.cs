using System;
using System.Collections.Generic;
using System.Drawing;

namespace CircleDetection
{
    public class FastRandomizedCircleDetection : RandomizedCircleDetection
    {
        private double _t1, _t2, _t3, _t4, _t5, _t6, _t7, _t8;

        public FastRandomizedCircleDetection(Edge edge, int minEdgePointsThreshold, int failThreshold,
            int minDistBetweenPoints, double minRadiusErrorThreshold, double minimumEdgesDensity, double maximumEdgesDensity, double circlecircumfrenceRange, EdgeType edgeType, bool hasMultipleCircles = true)
            : base(edge, minEdgePointsThreshold, failThreshold, minDistBetweenPoints, minRadiusErrorThreshold,
                  minimumEdgesDensity, maximumEdgesDensity, circlecircumfrenceRange, edgeType, hasMultipleCircles)
        {
        }

        private void IntiliseThresholds(Point center, int radius)
        {
            this._t1 = center.X - radius - this.Td;
            this._t2 = center.X + radius + this.Td;
            this._t3 = center.Y - radius - this.Td;
            this._t4 = center.Y + radius + this.Td;
            this._t5 = center.X + 1.4142 * (radius - this.Td) / 2;
            this._t6 = center.X - 1.4142 * (radius - this.Td) / 2;
            this._t7 = center.Y + 1.4142 * (radius - this.Td) / 2;
            this._t8 = center.Y - 1.4142 * (radius - this.Td) / 2;
        }

        private bool IsPossiblePointOnCircle(Point p4)
        {
            if (!(this._t1 < p4.X && p4.X < this._t2))
                return false;
            else if (!(this._t3 < p4.Y && p4.Y < this._t4))
                return false;
            else if (!(p4.X > this._t5 || p4.X < this._t6 || p4.Y > this._t7 || p4.Y < this._t8))
                return false;
            return true;
        }

        private Point? SelectPossibleTruePointOnCircle(int failCount = 10)
        {
            int count = 0;
            List<Point> points1Q = new List<Point>();
            List<Point> points2Q = new List<Point>();
            List<Point> points3Q = new List<Point>();
            List<Point> points4Q = new List<Point>();
            Random randint = new Random();
            while (count < failCount)
            {
                var quad = count % 4 + 1;
                int id;
                Point p4;
                switch (quad)
                {
                    case 1:
                        id = randint.Next(0, this.Edges1Q.Count);
                        p4 = this.Edges1Q[id];
                        this.Edges1Q.RemoveAt(id);
                        points1Q.Add(new Point(p4.X, p4.Y));
                        break;
                    case 2:
                        id = randint.Next(0, this.Edges2Q.Count);
                        p4 = this.Edges2Q[id];
                        this.Edges2Q.RemoveAt(id);
                        points2Q.Add(new Point(p4.X, p4.Y));
                        break;
                    case 3:
                        id = randint.Next(0, this.Edges3Q.Count);
                        p4 = this.Edges3Q[id];
                        this.Edges3Q.RemoveAt(id);
                        points3Q.Add(new Point(p4.X, p4.Y));
                        break;
                    case 4:
                        id = randint.Next(0, this.Edges4Q.Count);
                        p4 = this.Edges4Q[id];
                        this.Edges4Q.RemoveAt(id);
                        points4Q.Add(new Point(p4.X, p4.Y));
                        break;
                    default:
                        p4 = new Point();
                        break;
                }
                if (this.IsPossiblePointOnCircle(p4))
                {
                    this.Edges1Q.AddRange(points1Q);
                    this.Edges2Q.AddRange(points2Q);
                    this.Edges3Q.AddRange(points3Q);
                    this.Edges4Q.AddRange(points4Q);
                    return p4;
                }
                count++;
            }
            this.Edges1Q.AddRange(points1Q);
            this.Edges2Q.AddRange(points2Q);
            this.Edges3Q.AddRange(points3Q);
            this.Edges4Q.AddRange(points4Q);
            return null;
        }

        private bool IsDeterminedPointsGivesCandidateCircle(IReadOnlyList<Point> points, Point center, int radius)
        {
            int count = 3;
            double td = this.Td + 1;
            while (count >= 0)
            {
                var dist = this.DistanceBetweenTwoPoints(points[count], center);
                dist = Math.Abs(dist - radius);
                if (dist > td)
                {
                    return false;
                }
                else if (this.Td <= dist || dist >= td)
                {
                    count--;
                }
                else if (dist <= this.Td)
                {
                    return true;
                }
            }
            return false;
        }

        public override List<int[]> DetectCircles()
        {
            while (true)
            {
                this.CurrentTestNo++;
                List<int[]> circles = new List<int[]>();
                int rows = this.EdgeObject.Rows;
                int cols = this.EdgeObject.Cols;
                int failCount = 0;
                if (this.imageDensity > this.MinEdgeDensity && this.imageDensity < this.MaxEdgeDensity)
                {
                    while (failCount < this.Tf && this.EdgePointsList.Count > this.Tmin)
                    {
                        var failed = false;
                        List<Point> points = this.ChooseFourSemiRandomPoints(true);
                        if (points.Count == 3)
                        {
                            if (!this.IsCollinear(points[0], points[1], points[2]) &&
                                this.IsDistanceBetweenPointsGreaterThanTa(points[0], points[1], points[2], this.Ta))
                            {
                                Point center = this.PossibleCircleCenter(points[0], points[1], points[2]);
                                int radius = (int) this.DistanceBetweenTwoPoints(center, points[0]);
                                this.IntiliseThresholds(center, radius);
                                Point? p4 = this.SelectPossibleTruePointOnCircle();
                                if (p4 == null)
                                {
                                    failed = true;
                                }
                                else
                                {
                                    points.Add((Point) p4);
                                    if (this.IsDeterminedPointsGivesCandidateCircle(points, center, radius))
                                    {
                                        var count = 0;
                                        int np = Convert.ToInt32(this.LambdaThreshold*2*3.1416*radius);
                                        count = this.PixelVoter(this.Edges1Q, center, radius, np, count);
                                        count = this.PixelVoter(this.Edges2Q, center, radius, np, count);
                                        count = this.PixelVoter(this.Edges3Q, center, radius, np, count);
                                        count = this.PixelVoter(this.Edges4Q, center, radius, np, count);
                                        if (count >= np && !this.DetectMultiCircles)
                                        {
                                            int[] circle = new int[3];
                                            circle[0] = center.X;
                                            circle[1] = center.Y;
                                            circle[2] = radius;
                                            circles.Add(circle);
                                            return circles;
                                        }
                                        else if (count >= np)
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
                            }
                        }
                        else
                        {
                            failed = true;
                        }
                        if (!failed) continue;
                        failCount++;
                        try
                        {
                            this.Edges1Q.Add(points[0]);
                            this.Edges2Q.Add(points[1]);
                            this.Edges3Q.Add(points[2]);
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch (Exception)
                        {
                        }
                      
                    }
                }
                if (this.CurrentTestNo <= MaxTests && circles.Count == 0)
                {
                    return this.DetectCircles();
                }
                return circles;
            }
        }

        public override bool ContainsCircle()
        {
            List<int[]> circles = this.DetectCircles();
            return circles.Count > 0;
        }

    }
}
