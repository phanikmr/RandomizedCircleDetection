using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace CircleDetection
{
    public class Thinning
    {
        private static readonly int[] G123Lut = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1,
       0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0,
       1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0,
       0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1,
       0, 0, 0 };

        private static readonly int[] G123PLut = {
            0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0,
       1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0,
       0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0,
       1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 1,
       0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0 };
        private Image<Gray, byte> _element;
        private readonly List<Point> _changedSequenceIn1;
        private readonly List<Point> _changedSequenceIn2;

        private Image<Gray, byte> ThinnedImage { get; set; }

        private readonly Image<Gray, byte> _originalImage;
        private int _p1, _p2, _p3, _p4, _p5, _p6, _p7, _p8, _p9;

        public Thinning(Image<Gray, byte> image)
        {
            this._originalImage = image.Copy();
            this._changedSequenceIn1 = new List<Point>();
            this._changedSequenceIn2 = new List<Point>();
        }

        public Image<Gray, byte> ZhangSuenThinning()
        {
            this.ThinnedImage = this._originalImage.Copy();
            for (int i = 0; i < this.ThinnedImage.Rows; i++)
            {
                this.ThinnedImage[i, 0] = new Gray(0);
                this.ThinnedImage[0, i] = new Gray(0);
                this.ThinnedImage[this.ThinnedImage.Rows - 1, i] = new Gray(0);
                this.ThinnedImage[i, this.ThinnedImage.Cols - 1] = new Gray(0);
            }
            do
            {
                this._changedSequenceIn1.Clear();
                this._changedSequenceIn2.Clear();
                this.ZsSequencer(1);
                this.PixelsBlacker(this._changedSequenceIn1);
                this.ZsSequencer(2);
                this.PixelsBlacker(this._changedSequenceIn2);
            } while (this._changedSequenceIn1.Count != 0 && this._changedSequenceIn2.Count != 0);
            return this.ThinnedImage;
        }

        private void InitialisePMat(int x, int y)
        {
            this._p1 = (int)this.ThinnedImage[x, y].Intensity;
            this._p2 = (int)this.ThinnedImage[x - 1, y].Intensity;
            this._p3 = (int)this.ThinnedImage[x - 1, y + 1].Intensity;
            this._p4 = (int)this.ThinnedImage[x, y + 1].Intensity;
            this._p5 = (int)this.ThinnedImage[x + 1, y + 1].Intensity;
            this._p6 = (int)this.ThinnedImage[x + 1, y].Intensity;
            this._p7 = (int)this.ThinnedImage[x + 1, y - 1].Intensity;
            this._p8 = (int)this.ThinnedImage[x, y - 1].Intensity;
            this._p9 = (int)this.ThinnedImage[x - 1, y - 1].Intensity;
        }

        private void ZsSequencer(int sequenceNumber)
        {
            for (int i = 1; i < this.ThinnedImage.Rows - 1; i++)
            {
                for (int j = 1; j < this.ThinnedImage.Cols - 1; j++)
                {
                    this.InitialisePMat(i, j);
                    if (this._p1 != 255) continue;
                    var transitionCount = this.TransitionCountForCurrentSequence();
                    var whitePixelCount = this.WhitePixelCountForCurrentSequence();
                    if (transitionCount != 1)
                        continue;
                    if (2 > whitePixelCount && whitePixelCount < 6)
                        continue;
                    switch (sequenceNumber)
                    {
                        case 1:
                            if (!(this._p2 == 0 || this._p4 == 0 || this._p6 == 0))
                                continue;
                            if (!((this._p4 == 0 || this._p6 == 0 || this._p8 == 0)))
                                continue;
                            this._changedSequenceIn1.Add(new Point(i, j));
                            break;
                        case 2:
                            if (!(this._p2 == 0 || this._p4 == 0 || this._p8 == 0))
                                continue;
                            if (!((this._p2 == 0 || this._p6 == 0 || this._p8 == 0)))
                                continue;
                            this._changedSequenceIn2.Add(new Point(i, j));
                            break;
                    }
                }
            }
        }

        private int TransitionCountForCurrentSequence()
        {
            int count = 0;

            if (this._p2 == 0 && this._p3 == 255)
                count++;
            if (this._p3 == 0 && this._p4 == 255)
                count++;
            if (this._p4 == 0 && this._p5 == 255)
                count++;
            if (this._p5 == 0 && this._p6 == 255)
                count++;
            if (this._p6 == 0 && this._p7 == 255)
                count++;
            if (this._p7 == 0 && this._p8 == 255)
                count++;
            if (this._p8 == 0 && this._p9 == 255)
                count++;
            if (this._p9 == 0 && this._p2 == 255)
                count++;

            return count;
        }

        private int WhitePixelCountForCurrentSequence()
        {
            int count = 0;

            if (this._p2 == 255)
                count++;
            if (this._p3 == 255)
                count++;
            if (this._p4 == 255)
                count++;
            if (this._p5 == 255)
                count++;
            if (this._p6 == 255)
                count++;
            if (this._p7 == 255)
                count++;
            if (this._p8 == 255)
                count++;
            if (this._p9 == 255)
                count++;

            return count;
        }

        private void PixelsBlacker(List<Point> points)
        {
            foreach (Point point in points)
            {
                this.ThinnedImage[point.X, point.Y] = new Gray(0);
            }
        }

        private int[,] TakeFromLut(IReadOnlyList<int> lut, Image<Gray, byte> image)
        {
            int[,] takenvalues = new int[image.Rows, image.Cols];

            for (int i = 0; i < image.Rows; i++)
            {
                for (int j = 0; j < image.Cols; j++)
                {
                    takenvalues[i, j] = lut[(int)image[i, j].Intensity];
                }
            }

            return takenvalues;
        }

        private void PutValuesInImage(Image<Gray, Byte> image, int[,] indices, int value)
        {
            Gray pixel = new Gray(value);
            for (int i = 0; i < indices.GetLength(0); i++)
            {
                for (int j = 0; j < indices.GetLength(1); j++)
                {
                    if (indices[i, j] == 1)
                    {
                        image[i, j] = pixel;
                    }
                }
            }
        }

        public Image<Gray, byte> BwThinning(int iterations = -1)
        {
            this.ThinnedImage = this._originalImage.Copy() / 255;
            Image<Gray, byte> correlatedImage = new Image<Gray, Byte>(this.ThinnedImage.Rows, this.ThinnedImage.Cols);
            this._element = new Image<Gray, byte>(3, 3)
            {
                [0, 0] = new Gray(8),
                [0, 1] = new Gray(4),
                [0, 2] = new Gray(2),
                [1, 0] = new Gray(16),
                [1, 1] = new Gray(0),
                [1, 2] = new Gray(1),
                [2, 0] = new Gray(32),
                [2, 1] = new Gray(64),
                [2, 2] = new Gray(128)
            };

            while (iterations != 0)
            {
                var beforeCount = this.ThinnedImage.CountNonzero()[0];
                foreach (int[] lut in new int[][] { G123Lut, G123PLut })
                {
                    CvInvoke.Filter2D(this.ThinnedImage, correlatedImage, this._element, new Point(-1, -1), 0, BorderType.Constant);
                    var removedValues = this.TakeFromLut(lut, correlatedImage);
                    this.PutValuesInImage(this.ThinnedImage, removedValues, 0);
                }
                var afterCount = this.ThinnedImage.CountNonzero()[0];
                if (beforeCount == afterCount)
                    break;
                iterations--;
            }
            this.ThinnedImage = this.ThinnedImage * 255;
            return this.ThinnedImage.Copy();
        }
    }
}
