using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public class Result
    {
        private int _trainValue;
        private VectorOfPoint _trainContour;
        private MCvScalar _trainContourColor;
        private float _bestROIMatch;
        private Mat _referenceTrainImage;
        private VectorOfKeyPoint _referenceTrainKeyPoints;
        private VectorOfKeyPoint _keypointsEvalImag;
        private VectorOfDMatch _matches;
        private VectorOfDMatch _inliers;
        private VectorOfInt _inliersMatcheMask;
        private Mat _homography;
        private VectorOfKeyPoint _inliersKeyPoints;

        public Result()
        {
            _bestROIMatch = 0;
        }

        public Result(int trainValue, VectorOfPoint trainContour, MCvScalar trainContourColor, float bestROIMatch,
            Mat referenceTrainImage, VectorOfKeyPoint referenceTrainKeyPoints, VectorOfKeyPoint keypointsEvalImage, ref
             VectorOfDMatch matches, ref VectorOfDMatch inliers, ref VectorOfInt inliersMatcheMask, ref Mat homography)
        {
            this._trainValue = trainValue;
            this._trainContour = trainContour;
            this._trainContourColor = trainContourColor;
            this._bestROIMatch = bestROIMatch;
            this._referenceTrainImage = referenceTrainImage;
            this._referenceTrainKeyPoints = referenceTrainKeyPoints;
            this._keypointsEvalImag = keypointsEvalImage;
            this._matches = matches;
            this._inliers = inliers;
            this._inliersMatcheMask = inliersMatcheMask;
            this._homography = homography;
            this._inliersKeyPoints = new VectorOfKeyPoint();
        }

        public VectorOfPoint GetTrainContour()
        {
            if (_trainContour.Size == 0)
            {
                VectorOfPointF corners = new VectorOfPointF();
                corners.Push(new PointF[] { new PointF(0.0f, 0.0f) });
                corners.Push(new PointF[] { new PointF(_referenceTrainImage.Cols, 0.0f) });
                corners.Push(new PointF[] { new PointF(_referenceTrainImage.Cols, _referenceTrainImage.Rows) });
                corners.Push(new PointF[] { new PointF(0.0f, _referenceTrainImage.Rows) });

                VectorOfPointF transformedCorners = new VectorOfPointF();
                CvInvoke.PerspectiveTransform(corners, transformedCorners, _homography);

                for (int i = 0; i < transformedCorners.Size; ++i)
                {
                    _trainContour.Push(new Point[] { new Point((int)transformedCorners[i].X, (int)transformedCorners[i].Y) });
                }
            }
            return _trainContour;
        }

        public VectorOfKeyPoint GetInliersKeypoints()
        {
            if (_inliersKeyPoints.Size == 0)
            {
                for (int i = 0; i < _inliers.Size; ++i)
                {
                    MDMatch match = _inliers[i];

                    if (match.QueryIdx < _keypointsEvalImag.Size)
                    {
                        _inliersKeyPoints.Push(new MKeyPoint[] { _keypointsEvalImag[match.QueryIdx] });
                    }
                }
            }
            return _inliersKeyPoints;
        }

        public Mat getInliersMatches(ref Mat queryImage)
        {
            Mat inliersMatches = new Mat();
            if (_inliers == null)
            {
                return queryImage;
            }
            else
            {
                var handle1 = GCHandle.Alloc(queryImage);
                var handle2 = GCHandle.Alloc(inliersMatches);
                var handle3 = GCHandle.Alloc(_keypointsEvalImag);
                var handle4 = GCHandle.Alloc(_referenceTrainImage);
                var handle5 = GCHandle.Alloc(_referenceTrainKeyPoints);

                //Features2DToolbox.DrawMatches(queryImage, keypointsQueryImag, referenceImage, referenceImageKeyPoints, new VectorOfVectorOfDMatch(inliers), inliersMatches, new MCvScalar(0, 255, 0), new MCvScalar(0, 0, 255));


                handle1.Free();
                handle2.Free();
                handle3.Free();
                handle4.Free();
                handle5.Free();

                return inliersMatches;
            }
        }

        public Mat GetReferenceTrainImage()
        {
            return _referenceTrainImage;
        }

        public float GetBestROIMatch()
        {
            return _bestROIMatch;
        }

        public int GetTrainValue()
        {
            return _trainValue;
        }

        public MCvScalar GetTrainContourColor()
        {
            return _trainContourColor;
        }

        public VectorOfDMatch GetInliers()
        {
            return _inliers;
        }
    }
}
