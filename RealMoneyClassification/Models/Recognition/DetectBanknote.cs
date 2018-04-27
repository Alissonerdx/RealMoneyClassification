using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public class DetectBanknote
    {
        private VectorOfMat _descriptorsImageTrain;
        private List<List<int>> _indexKeypointsImageTrainAssociatedROI;
        private List<VectorOfKeyPoint> _keypointsImageTrain;
        private int _LODIndex;
        private VectorOfMat _trainsImage;
        private List<List<int>> _numberKeypointsImageTrainInContour;
        private Util _util;
        public DetectBanknote(int valueBanknote, MCvScalar colorContour, bool globalMatch)
        {
            this.ValueBanknote = valueBanknote;
            this.ColorContour = colorContour;
            this.GlobalMatch = globalMatch;
            _LODIndex = 0;
            _keypointsImageTrain = new List<VectorOfKeyPoint>();
            _descriptorsImageTrain = new VectorOfMat();
            _indexKeypointsImageTrainAssociatedROI = new List<List<int>>();
            _numberKeypointsImageTrainInContour = new List<List<int>>();
            _trainsImage = new VectorOfMat();
            _util = new Util();
        }

        public MCvScalar ColorContour { get; set; }
        public bool GlobalMatch { get; set; }
        public int ValueBanknote { get; set; }

        public bool ConfigRecognitionImageTrain(Mat imageTrain, Mat roiTrain, bool useGlobalMatch)
        {
            _trainsImage.Push(imageTrain);


            _LODIndex = _trainsImage.Size - 1;

            SIFT sift = new SIFT();

            //Insere os pontos chaves da imagem alvo na lista de pontos chaves
            _keypointsImageTrain.Insert(_LODIndex, new VectorOfKeyPoint(sift.Detect(_trainsImage[_LODIndex], roiTrain)));
            if (_keypointsImageTrain[_LODIndex] != null && _keypointsImageTrain[_LODIndex].Size < 4)
                return false;

            //Calcula os descritores dos pontos chaves extraidos, no caso se extrair poucos descritores ele return false = não reconhecido
            sift.Compute(_trainsImage[_LODIndex], _keypointsImageTrain[_LODIndex], _descriptorsImageTrain[_LODIndex]);
            if (_descriptorsImageTrain[_LODIndex].Rows < 4)
                return false;

            if (useGlobalMatch)
                return true;
            else
                return ConfigureImageTrainROI(_keypointsImageTrain[_LODIndex], roiTrain);

        }

        public bool ConfigureImageTrainROI(VectorOfKeyPoint keypointsImageTrain, Mat roiTrain)
        {
            if (keypointsImageTrain == null)
                return false;

            VectorOfVectorOfPoint contoursImageTrainROI = new VectorOfVectorOfPoint();
            VectorOfPointF hierarchyContours = new VectorOfPointF();

            //Extrai contornos da imagem definida como ROI (Mascara) e a hierarquia destes contornos
            CvInvoke.FindContours(roiTrain.Clone(), contoursImageTrainROI, hierarchyContours, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            if (contoursImageTrainROI.Size == 0)
                return false;

            int numberKeypointsImageTrain = keypointsImageTrain.Size;

            var pop = new ParallelOptions { MaxDegreeOfParallelism = 5 };

            for (int indexKeypointImageTrain = 0; indexKeypointImageTrain < numberKeypointsImageTrain; ++indexKeypointImageTrain)
            {
                Parallel.For(0, contoursImageTrainROI.Size, pop, i =>
                {
                    PointF pointXY = keypointsImageTrain[indexKeypointImageTrain].Point;
                    if (CvInvoke.PointPolygonTest(contoursImageTrainROI, pointXY, false) >= 0)
                    {
                        _indexKeypointsImageTrainAssociatedROI[_LODIndex][indexKeypointImageTrain] = i;
                        return;
                    }
                });
            }

            _numberKeypointsImageTrainInContour[_LODIndex].Clear();
            _numberKeypointsImageTrainInContour[_LODIndex] = new List<int>(contoursImageTrainROI.Size);

            for (int i = 0; i < _indexKeypointsImageTrainAssociatedROI[_LODIndex].Count; ++i)
            {
                var indexContour = _indexKeypointsImageTrainAssociatedROI[_LODIndex][i];
                ++_numberKeypointsImageTrainInContour[_LODIndex][indexContour];
            }

            return true;
        }

        public VectorOfKeyPoint GetKeypointsTrain()
        {
            return _keypointsImageTrain[_LODIndex];
        }

        public void UpdateCurrentLODIndex(ref Mat imageToAnalyze, float trainResolutionSelectionSplitOffset)
        {
            int halfImageResolution = imageToAnalyze.Cols / 2;

            int newLODINdex = 0;
            for (int i = 1; i < _trainsImage.Size; ++i)
            {
                int previousLODWidthResolution = _trainsImage[i - 1].Cols;
                int currentLODWidthResolution = _trainsImage[i].Cols;

                if (halfImageResolution > currentLODWidthResolution)
                {
                    newLODINdex = i; //use bigger resolution
                }
                else if (halfImageResolution < previousLODWidthResolution)
                {
                    newLODINdex = i - 1; //use lower resolution
                    break;
                }
                else
                {
                    int splittingPointResolution = (int)((currentLODWidthResolution - previousLODWidthResolution) * trainResolutionSelectionSplitOffset);
                    int imageOffsetResolution = currentLODWidthResolution - halfImageResolution;

                    if (imageOffsetResolution < splittingPointResolution)
                    {
                        newLODINdex = i - 1; //use lower resolution
                        break;
                    }
                    else
                    {
                        newLODINdex = i;
                        break;
                    }
                }
            }
            _LODIndex = newLODINdex;
        }

        public Result AnalyzeImageEval(ref VectorOfKeyPoint keypointsEvalImage, ref Mat descriptorsEvalImage, float maxDistanceRatio,
           float reprojectionThreshold, double confidence, int maxIters, int minimumNumbersInliers)
        {
            var matches = new VectorOfDMatch();
            //Emgu.CV.Flann.KdTreeIndexParamses flannIndexParams = new Emgu.CV.Flann.KdTreeIndexParamses(4);
            //var flannIndex = new Index(descriptorsQueryImage, flannIndexParams);
            //DescriptorMatcher matcher = flannIndex;
            BFMatcher bfmatcher = new BFMatcher(DistanceType.L2);

            _util.MatchDescriptorsWithRatioTest(bfmatcher, ref descriptorsEvalImage, _descriptorsImageTrain[_LODIndex], ref matches, maxDistanceRatio);

            if (matches.Size < minimumNumbersInliers)
            {
                return new Result();
            }

            Mat homography = new Mat();
            VectorOfDMatch inliers = new VectorOfDMatch();
            VectorOfInt inliersMaskOut = new VectorOfInt();
            _util.RefineMatchesWithHomography(keypointsEvalImage, _keypointsImageTrain[_LODIndex], matches, ref homography, inliers, inliersMaskOut, reprojectionThreshold, minimumNumbersInliers);

            if (inliers.Size < minimumNumbersInliers)
            {
                return new Result();
            }

            float bestROIMatch = 0;
            bestROIMatch = (float)inliers.Size / (float)matches.Size;

            return new Result(ValueBanknote, new VectorOfPoint(), ColorContour, bestROIMatch, _trainsImage[_LODIndex], _keypointsImageTrain[_LODIndex], keypointsEvalImage, ref matches, ref inliers, ref inliersMaskOut, ref homography);
        }
    }
}

