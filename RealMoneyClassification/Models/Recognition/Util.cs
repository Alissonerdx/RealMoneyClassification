using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public class Util
    {
        public bool LoadBinaryMask(string path, ref Mat maskBinary)
        {
            maskBinary = new Image<Gray, byte>(path).Mat;
            if (maskBinary != null)
            {
                CvInvoke.Threshold(maskBinary, maskBinary, 250.0, 255.0, Emgu.CV.CvEnum.ThresholdType.Binary);
                return true;
            }
            return false;
        }

        public double ComputeContourAspectRatio(VectorOfPoint contour)
        {
            RotatedRect contourEllipse = CvInvoke.MinAreaRect(contour);
            return contourEllipse.Size.Width / contourEllipse.Size.Height;
        }

        public double ComputeContourCircularity(VectorOfPoint contour)
        {
            double area = CvInvoke.ContourArea(contour);
            double perimeter = CvInvoke.ArcLength(contour, true);

            if (perimeter != 0)
            {
                return (4.0 * Math.PI * area) / (perimeter * perimeter);
            }
            return 0;
        }

        public bool MatchDescriptorsWithRatioTest(BFMatcher descriptorMatcher, ref Mat descriptorsEvalImage, Mat trainDescriptors, ref VectorOfDMatch matchesFilteredOut, float maxDistanceRatio)
        {
            if (trainDescriptors.Rows < 4)
            {
                return false;
            }

            matchesFilteredOut.Clear();
            descriptorMatcher.Add(trainDescriptors);

            VectorOfVectorOfDMatch matchesKNN = new VectorOfVectorOfDMatch();
            descriptorMatcher.KnnMatch(descriptorsEvalImage, matchesKNN, 2, null);
            for (int matchPos = 0; matchPos < matchesKNN.Size; ++matchPos)
            {
                if (matchesKNN[matchPos].Size >= 2)
                {
                    if (matchesKNN[matchPos][0].Distance <= maxDistanceRatio * matchesKNN[matchPos][1].Distance)
                    {
                        matchesFilteredOut.Push(new MDMatch[] { matchesKNN[matchPos][0] });
                    }
                }
            }

            return !(matchesFilteredOut.Size == 0);
        }

        public bool RefineMatchesWithHomography(VectorOfKeyPoint evalKeypoints, VectorOfKeyPoint trainKeypoints, VectorOfDMatch matches,
            ref Mat homographyOut, VectorOfDMatch inliersOut, VectorOfInt inliersMaskOut,
            float reprojectionThreshold, int minNumberMatchesAllowed)
        {
            if (matches.Size < minNumberMatchesAllowed)
            {
                return false;
            }

            PointF[] srcPoints = new PointF[matches.Size];
            PointF[] dstPoints = new PointF[matches.Size];

            for (int i = 0; i < matches.Size; ++i)
            {
                srcPoints[i] = trainKeypoints[matches[i].TrainIdx].Point;
                dstPoints[i] = evalKeypoints[matches[i].QueryIdx].Point;
            }

            inliersMaskOut.Clear();
            inliersMaskOut = new VectorOfInt(srcPoints.Count());

            //for(int i = 0; i < srcPoints.Count(); ++i)
            //{
            //    inliersMaskOut = 0;
            //}

            CvInvoke.FindHomography(srcPoints, dstPoints, homographyOut, Emgu.CV.CvEnum.HomographyMethod.Ransac, reprojectionThreshold, inliersMaskOut);

            for (int i = 0; i < inliersMaskOut.Size; ++i)
            {
                if (inliersMaskOut[i] > 0)
                {
                    inliersOut.Push(new MDMatch[] { matches[i] });
                }
            }
            return (inliersOut.Size >= minNumberMatchesAllowed);
        }


        public void RemoveInliersFromKeypointsAndDescriptors(VectorOfDMatch inliers, ref VectorOfKeyPoint keypointsQueryImageInOut, ref Mat descriptorsQueryImageInOut)
        {
            List<int> inliersKeypointsPositions = new List<int>();

            for (int inliersIndex = 0; inliersIndex < inliers.Size; ++inliersIndex)
            {
                MDMatch match = inliers[inliersIndex];
                inliersKeypointsPositions.Add(match.QueryIdx);
            }

            inliersKeypointsPositions.Sort();

            VectorOfKeyPoint keypointsQueryImageBackup = null;
            keypointsQueryImageBackup = keypointsQueryImageInOut;
            keypointsQueryImageInOut = new VectorOfKeyPoint();
            Mat filteredDescriptors = new Mat();
            for (int rowIndex = 0; rowIndex < descriptorsQueryImageInOut.Rows; ++rowIndex)
            {
                if (!inliersKeypointsPositions.Exists(i => i == rowIndex))
                {
                    keypointsQueryImageInOut.Push(new MKeyPoint[] { keypointsQueryImageBackup[rowIndex] });

                    Matrix<float> matrix = new Matrix<float>(descriptorsQueryImageInOut.Size);
                    descriptorsQueryImageInOut.ConvertTo(matrix, Emgu.CV.CvEnum.DepthType.Cv32F);
                    var linha = matrix.GetRow(rowIndex).Mat;
                    filteredDescriptors.PushBack(linha);
                }
            }
            filteredDescriptors.CopyTo(descriptorsQueryImageInOut);
        }

        public void CorrectBoundingBox(ref Rectangle boundingBoxInOut, int imageWidth, int imageHeight)
        {
            if (boundingBoxInOut.X < 0)
            {
                boundingBoxInOut.Width += boundingBoxInOut.X;
                boundingBoxInOut.X = 0;
            }

            if (boundingBoxInOut.X > imageWidth)
            {
                boundingBoxInOut.Width = 0;
                boundingBoxInOut.X = imageWidth;
            }

            if (boundingBoxInOut.Y < 0)
            {
                boundingBoxInOut.Height += boundingBoxInOut.Y;
                boundingBoxInOut.Y = 0;
            }

            if (boundingBoxInOut.Y > imageHeight)
            {
                boundingBoxInOut.Height = 0;
                boundingBoxInOut.Y = imageWidth;
            }

            int maxWidth = imageWidth - boundingBoxInOut.X;
            if (boundingBoxInOut.Width > maxWidth)
            {
                boundingBoxInOut.Width = maxWidth;
            }

            int maxHeight = imageHeight - boundingBoxInOut.Y;
            if (boundingBoxInOut.Height > maxHeight)
            {
                boundingBoxInOut.Height = maxHeight;
            }
        }

        public void DrawContour(ref Mat image, VectorOfPoint contour, MCvScalar color, int thiclness)
        {
            for (int i = 0; i < contour.Size; ++i)
            {
                Point p1 = contour[i];
                Point p2 = new Point();
                if (i == contour.Size - 1)
                {
                    p2 = contour[0];
                }
                else
                {
                    p2 = contour[i + 1];
                }

                CvInvoke.Line(image, p1, p2, color, thiclness);
            }
        }
    }
}
