using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Text;
using ClosedXML.Excel;


namespace CircleDetection
{
    internal static class MainClass
    {
        [STAThread]
        private static void Main(string[] args)
        {
            CvInvoke.UseOptimized = true;
            CvInvoke.UseOpenCL = true;
            String filePath  = OpenFile(@"/test images");
            using(Edge edge = new Edge(filePath, true))
            {
                RandomizedCircleDetection randmoizedCircleDetect = new RandomizedCircleDetection(edge, 30, 2000, 15, 1,
                         0.01, 0.08, 0.47, EdgeType.CANNY_EDGE, false);
                List<int[]> circles = randmoizedCircleDetect.DetectCircles();
                if (circles.Count != 0)
                {
                    Console.WriteLine("Randomized Circle Detection");
                    Console.WriteLine("CX, CY, R");
                    Console.Write(circles[0][0]);
                    Console.Write(", ");
                    Console.Write(circles[0][1]);
                    Console.Write(", ");
                    Console.Write(circles[0][2]);
                    Console.WriteLine(" ");

                    Image<Bgr, byte> image = edge.GetOriginalImage();
                    CvInvoke.Circle(image, new Point(circles[0][0], circles[0][1]), circles[0][2],
                        new MCvScalar(1, 123, 100), 5);
                    ImageView.imShow(image);                    
                }
                else
                {
                      Console.WriteLine("No Circle Found");
                      ImageView.imShow(edge.GetBinaryEdgeImage());
                }
                FastRandomizedCircleDetection frandCircleDetect = new FastRandomizedCircleDetection(edge, 30, 2000, 15, 1,
                         0.01, 0.08, 0.47, EdgeType.CANNY_EDGE, false);
                circles = frandCircleDetect.DetectCircles();
                if (circles.Count != 0)
                {
                    Console.WriteLine("Fast Randomized Circle Detection");
                    Console.WriteLine("CX, CY, R");
                    Console.Write(circles[0][0]);
                    Console.Write(", ");
                    Console.Write(circles[0][1]);
                    Console.Write(", ");
                    Console.Write(circles[0][2]);
                    Console.WriteLine(" ");

                    Image<Bgr, byte> image = edge.GetOriginalImage();
                    CvInvoke.Circle(image, new Point(circles[0][0], circles[0][1]), circles[0][2],
                        new MCvScalar(1, 123, 100), 5);
                    ImageView.imShow(image);
                }
                else
                {
                    Console.WriteLine("No Circle Found");
                    ImageView.imShow(edge.GetBinaryEdgeImage());
                }
            }
        }


        private static string OpenFile(string initialDir)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = initialDir,
                Filter = "PNG Files (.png)|*.png|All Files (*.*)|*.*"
            };
            openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }


        public static void batchProcess(string sourceDir, string destinationDir)
        {
            XLWorkbook workBook;
            workBook = File.Exists(destinationDir + "//results.xlsx") ? new XLWorkbook(destinationDir + "//results.xlsx") : new XLWorkbook();
            string[] dirs = Directory.GetDirectories(sourceDir);
            double totalDirs = dirs.Length;
            double currentdir = 0;
            Console.WriteLine("Processed 0 %");
            foreach (string dir in dirs)
            {
                currentdir++;
                string[] files = Directory.GetFiles(dir);
                Console.WriteLine("Processing Directory : " + dir);
                string destinationPath = destinationDir + @"\" + dir.Replace(sourceDir + @"\", "");
                if(Directory.Exists(destinationPath))
                    continue;
                var sheetName = Path.GetFileName(dir).Trim();
                if (sheetName.Length >= 31)
                    sheetName = sheetName.Substring(0, 9) + " " + sheetName.Substring(sheetName.Length - 10);
                var workSheet = workBook.Worksheets.Add(sheetName);
                workSheet.Cell(1, 1).Value = "File Name";
                workSheet.Cell(1, 2).Value = "Circle Status";
                workSheet.Cell(1, 3).Value = "Center";
                workSheet.Cell(1, 4).Value = "Radius";
                workSheet.Cell(1, 5).Value = "FailCount";
                workSheet.Cell(1, 6).Value = "Time Elapsed";
                workSheet.Cell(1, 7).Value = "Output";
                workSheet.Cell(1, 8).Value = "image Density";
                workSheet.Cell(1, 9).Value = "Edge Points Count";
                Directory.CreateDirectory(destinationPath);
                FileStream densityFile1 = File.Create(destinationPath + "/densities.txt");
                Directory.CreateDirectory(destinationPath + "/Undetected Circles");
                FileStream densityFile2 = File.Create(destinationPath + "/Undetected Circles" + "/densities.txt");
                Directory.CreateDirectory(destinationPath + "/ByPassed Images");
                var index = 1;
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    try
                    {
                        Convert.ToInt32(fileName);
                        index++;
                        //Console.WriteLine(file);
                        workSheet.Cell(index,1).SetValue(fileName).Hyperlink = new XLHyperlink(new Uri(file));
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        Edge edgeObject = new Edge(file, true);
                        RandomizedCircleDetection randCircleDetect = new RandomizedCircleDetection(edgeObject, 30, 2000, 15, 1,
                         0.01, 0.08, 0.47, EdgeType.CANNY_EDGE, false);
                        List<int[]> circles = randCircleDetect.DetectCircles();
                        watch.Stop();
                        string density;
                        byte[] bytes;                      
                        if (circles.Count != 0)
                        {
                            workSheet.Cell(index, 2).SetValue(true);
                            workSheet.Cell(index, 2).Style.Font.SetFontColor(XLColor.Green);
                            workSheet.Cell(index, 3).SetValue(new Point(circles[0][0], circles[0][1]));
                            workSheet.Cell(index, 4).SetValue(circles[0][2]);
                            Image<Bgr, byte> image = edgeObject.GetOriginalImage();
                            CvInvoke.Circle(image, new Point(circles[0][0], circles[0][1]), circles[0][2],
                                new MCvScalar(1, 123, 100), 5);
                            //File.Copy(file, destinationPath + "/" + fileName + ".png", true);
                            //CircleDeleter deleter = new CircleDeleter(edgeObject.GetGrayImage(), circles[0][0], circles[0][1], circles[0][2]);
                            //Image<Gray,byte> textImage = deleter.deleteCircle();
                            //CvInvoke.MedianBlur(textImage, textImage, 3);
                            CvInvoke.Imwrite(destinationPath + @"\" + fileName + ".png", image);
                           // Console.WriteLine(destinationPath + @"\" + fileName + ".png");
                           
                            var temp = destinationPath + Path.DirectorySeparatorChar + fileName + ".png";                                             
                            density = ((double)edgeObject.GetEdgePointsCount() / (double)(edgeObject.Rows * edgeObject.Cols)).ToString() + ": " + file;
                            bytes = Encoding.ASCII.GetBytes(density.ToCharArray());
                            densityFile1.Write(bytes, 0, bytes.Length);
                            bytes = Encoding.ASCII.GetBytes(Environment.NewLine);
                            densityFile1.Write(bytes, 0, bytes.Length);
                            workSheet.Cell(index, 7).SetValue(fileName).Hyperlink = new XLHyperlink(new Uri(temp));
                        }
                        else
                        {

                            workSheet.Cell(index, 2).SetValue(false);
                            workSheet.Cell(index, 2).Style.Font.SetFontColor(XLColor.Red);
                            File.Copy(file, destinationPath + "/Undetected Circles/" + fileName + ".png", true);
                            workSheet.Cell(index, 3).SetValue("NIL");
                            workSheet.Cell(index, 4).SetValue("NIL");
                            workSheet.Cell(index, 7).SetValue("NIL");
                            density = ((double)edgeObject.GetEdgePointsCount() / (double)(edgeObject.Rows * edgeObject.Cols)).ToString() + ": " + file;
                            bytes = Encoding.ASCII.GetBytes(density.ToCharArray());
                            densityFile2.Write(bytes, 0, bytes.Length);
                            bytes = Encoding.ASCII.GetBytes(Environment.NewLine);
                            densityFile2.Write(bytes, 0, bytes.Length);
                        }

                        workSheet.Cell(index, 5).SetValue(randCircleDetect.FailCount);
                        workSheet.Cell(index, 6).SetValue(watch.ElapsedMilliseconds.ToString() + "ms");
                        workSheet.Cell(index, 8).SetValue(randCircleDetect.ImageDensity);
                        workSheet.Cell(index, 9).SetValue(edgeObject.GetEdgePointsCount());
                    }
                    catch (Exception)
                    {
                        File.Copy(file, destinationPath + "/ByPassed Images/" + fileName + ".png", true);
                    }


                }
                index += 3;
                workSheet.Cell(index, 3).SetValue("Edge Type");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("Canny Edge");
                workSheet.Cell(index, 4).Style.Font.SetBold();

                index++;            
                workSheet.Cell(index, 3).SetValue("Min Edge Pts in Image Threshold");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("30");
                workSheet.Cell(index, 4).Style.Font.SetBold();

                index++;
                workSheet.Cell(index, 3).SetValue("Min Dist Btwn Edge Pts in Image Threshold");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("15");
                workSheet.Cell(index, 4).Style.Font.SetBold();

                index++;
                workSheet.Cell(index, 3).SetValue("Allowed Radius Error");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("1");
                workSheet.Cell(index, 4).Style.Font.SetBold();

                index++;
                workSheet.Cell(index, 3).SetValue("Circumfrence range");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("0.47");
                workSheet.Cell(index, 4).Style.Font.SetBold();


                index++;
                workSheet.Cell(index, 3).SetValue("Max Allowed Fail Count");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("2000");
                workSheet.Cell(index, 4).Style.Font.SetBold();


                index++;
                workSheet.Cell(index, 3).SetValue("Min Allowed Image Density");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("0.01");
                workSheet.Cell(index, 4).Style.Font.SetBold();


                index++;
                workSheet.Cell(index, 3).SetValue("Max Allowed Image Density");
                workSheet.Cell(index, 3).Style.Font.SetBold();
                workSheet.Cell(index, 4).SetValue("0.08");
                workSheet.Cell(index, 4).Style.Font.SetBold();

                densityFile1.Close();
                densityFile2.Close();                
                Console.WriteLine("Processed : " + (currentdir * 100 / totalDirs));
            }
            if(File.Exists(destinationDir+"//results.xlsx"))
                workBook.Save();
            else
                workBook.SaveAs(destinationDir+ Path.DirectorySeparatorChar +"results.xlsx");
            
        }


        public static void edgeMain(string file)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            // Console.WriteLine(file);
            Edge edge = new Edge(file, true);
            // edge.detectEdges();
            edge.DetectCannyEdges();
            watch.Stop();
            Console.WriteLine("Elapsed Time :" + watch.ElapsedMilliseconds + " ms");
            Console.WriteLine("Edge Points Count : " + edge.GetEdgePointsCount());
            ImageView.imShow(edge.GetBinaryEdgeImage());
        }

        public static void RandomizedCircleMain(string file)
        {
            var detectedTimes = 0;
            for (var c = 1; c <= 1; c++)
            {
                Stopwatch stopWatch = new Stopwatch();

                stopWatch.Start();
                using (Edge edge = new Edge(file, true))
                {
                    RandomizedCircleDetection randCircleDetect = new RandomizedCircleDetection(edge, 30, 2000, 15, 1,
                         0.01, 0.08, 0.47, EdgeType.CANNY_EDGE, false);
                    // FastRandomizedCircleDetection frandCircleDetect = new FastRandomizedCircleDetection(edge, 30, 150, 30, 1, 0.015, 0.035, 0.3, EdgeType.MORPHOLOGICAL_THINNING, false);
                    List<int[]> circles = randCircleDetect.DetectCircles();
                    //List<int[]> circles = frandCircleDetect.detectCircles();
                    stopWatch.Stop();
                    Console.WriteLine("Elapsed Time: " + stopWatch.ElapsedMilliseconds.ToString() + "ms");
                    Console.WriteLine("Fail Count = " + randCircleDetect.FailCount);
                    Console.WriteLine("Edge Count = " + edge.GetEdgePointsCount());
                    Console.WriteLine("Image Density = " + randCircleDetect.ImageDensity);
                    //Console.WriteLine("Edges Count :" + edge.GetEdgePointsCount());
                    if (circles.Count != 0)
                    {
                        detectedTimes++;
                        Console.WriteLine(circles[0][0]);
                        Console.WriteLine(circles[0][1]);
                        Console.WriteLine(circles[0][2]);
                       
                        Image<Bgr, byte> image = edge.GetOriginalImage();
                        CvInvoke.Circle(image, new Point(circles[0][0], circles[0][1]), circles[0][2],
                            new MCvScalar(1, 123, 100), 1);
                        ImageView.imShow(image,edge.GetBinaryEdgeImage());
                        //CircleDeleter deleter = new CircleDeleter(edge.GetGrayImage(), circles[0][0], circles[0][1], circles[0][2]);
                        //Image<Gray, byte> textImage = deleter.deleteCircle();
                        //Image<Gray, byte> textImage1 = new Image<Gray, byte>(edge.Rows, edge.Cols);
                        //CvInvoke.MedianBlur(textImage, textImage1, 3);
                        //ImageView.imShow(textImage, textImage1);
                    }
                    else
                    {
                      //  Console.WriteLine("No Circle Found");
                      // ImageView.imShow(edge.GetBinaryEdgeImage());
                    }
                    //    ImageView.imShow(edge.getBinaryEdgeImage());
                    Console.WriteLine("Round No.: " + c + " detected Times: " + detectedTimes);
                }
            }

        }


        public static void FastRandomizedCircleDetectionMain(string file)
        {
            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();
            Edge edge = new Edge(file, true);
            FastRandomizedCircleDetection frandCircleDetect = new FastRandomizedCircleDetection(edge, 30, 150, 30, 1, 0.015, 0.035, 0.3, EdgeType.MORPHOLOGICAL_THINNING, false);
            List<int[]> circles = frandCircleDetect.DetectCircles();
            stopWatch.Stop();
            Console.WriteLine("Elapsed Time: " + stopWatch.ElapsedMilliseconds.ToString() + "ms");
            Console.WriteLine("Edges Count :" + edge.GetEdgePointsCount());

            if (circles.Count != 0)
            {
                Console.WriteLine(circles[0][0]);
                Console.WriteLine(circles[0][1]);
                Console.WriteLine(circles[0][2]);
                Image<Bgr, Byte> image = edge.GetOriginalImage();
                CvInvoke.Circle(image, new Point(circles[0][0], circles[0][1]), circles[0][2], new MCvScalar(1, 123, 100), 5);
                ImageView.imShow(image);
            }
            else
            {
                Console.WriteLine("No Circle Found");
            }
            // ImageView.imShow(edge.getBinaryEdgeImage());
        }
    }
}
