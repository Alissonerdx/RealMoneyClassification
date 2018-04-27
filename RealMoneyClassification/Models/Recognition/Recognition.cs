using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public class Recognition
    {

        public List<ImageCustom> ImagesTrains { get; set; }
        public List<DetectBanknote> DetectedBanknotes { get; set; }
        private PointF _contourAspectRatioRange;
        private PointF _contourCircularityRange;
        private Util _util { get; set; }
        private Configurations _configurations;
        private static SpeechSynthesizer _synthesizer;

        /// <summary>
        /// Está implementação está sendo baseado no trabalho do carlosmccosta que se encontra em 
        /// https://github.com/carlosmccosta/Currency-Recognition
        /// </summary>
        public Recognition()
        {
            DetectedBanknotes = new List<DetectBanknote>();
            _contourAspectRatioRange = new PointF(-1, -1);
            _contourCircularityRange = new PointF(-1, -1);
            _util = new Util();
            _configurations = new Configurations();
            _synthesizer = new SpeechSynthesizer();
        }

        private bool LoadBanknotesTrains(bool globalMatch)
        {
            DirectoryInfo d = new DirectoryInfo(_configurations.PATH_LIST_BANKNOTES_ASSOCIATED_LABELS);
            StreamReader reader = new StreamReader(d.FullName);
            var row = "";
            var banknotesConfiguration = new List<string>();

            while ((row = reader.ReadLine()) != null)
            {
                banknotesConfiguration.Add(row);
            }

            var numberOfFiles = banknotesConfiguration.Count;

            var separators = new string[] { "|" };

            for (int configIndex = 0; configIndex < numberOfFiles; ++configIndex)
            {
                var fileName = "";
                var value = 0;
                var color = new MCvScalar();
                var rows = banknotesConfiguration[configIndex].Split(separators, StringSplitOptions.None);
                fileName = rows[0].Trim();
                value = Convert.ToInt32(rows[1].Trim());
                rows[2] = rows[2].Trim();
                var colorTemp = rows[2].Split(' ');
                color = new MCvScalar(Convert.ToDouble(colorTemp[2]), Convert.ToDouble(colorTemp[1]), Convert.ToDouble(colorTemp[0]));

                var banknoteDetect = new DetectBanknote(value, color, false);

                var imageTrain = new Image<Gray, byte>(_configurations.PATH_BANKNOTES + fileName);
                var imageTrainMat = imageTrain.Mat;
                var imageTrainPreProcessing = PreProcess(ref imageTrainMat);

                var filenameTemp = fileName.Split('.');
                var pathMaskImageTrain = _configurations.PATH_BANKNOTES + filenameTemp[0] + _configurations.MASK + filenameTemp[1];

                var util = new Util();
                var maskROI = new Mat();

                if (util.LoadBinaryMask(pathMaskImageTrain, ref maskROI))
                {
                    banknoteDetect.ConfigRecognitionImageTrain(imageTrainPreProcessing, maskROI, true);

                    var trainKeypoints = banknoteDetect.GetKeypointsTrain();

                    if (trainKeypoints == null)
                    {
                        imageTrain = new Image<Gray, byte>(_configurations.PATH_BANKNOTES + fileName);
                    }
                    else
                    {
                        Mat imageTrainKeypoints = new Mat();
                        Features2DToolbox.DrawKeypoints(imageTrain, trainKeypoints, imageTrainKeypoints, new Bgr(0, 255, 0), Features2DToolbox.KeypointDrawType.Default);
                        ImageViewer iv = new ImageViewer(imageTrainKeypoints, "Keypoints Train Image");
                        iv.Show();
                    }
                }

                DetectedBanknotes.Add(banknoteDetect);
            }


            return true;

        }

        public Mat PreProcess(ref Mat image)
        {
            CvInvoke.BilateralFilter(image.Clone(), image, _configurations.BILATERAL_FILTER_DISTANCE, _configurations.BILATERAL_FILTER_SIGMA_COLOR, _configurations.BILATERAL_FILTER_SIGMA_SPACE);

            var viewer = new ImageViewer(image, "Bilateral Filter Applied");
            viewer.Show();

            image = HistogramEqualization(image, true);

            image.ConvertTo(image, DepthType.Default, (double)_configurations.CONTRAST_MULTIPLIED_BY_10 / 10.0, (double)_configurations.BRIGHTNESS_MULTIPLIED_BY_10 / 10.0);

            CvInvoke.BilateralFilter(image.Clone(), image, _configurations.BILATERAL_FILTER_DISTANCE, _configurations.BILATERAL_FILTER_SIGMA_COLOR, _configurations.BILATERAL_FILTER_SIGMA_SPACE);

            viewer = new ImageViewer(image, "Adjusted Contrast and Illumination");
            viewer.Show();

            return image;
        }

        private Mat HistogramEqualization(Mat image, bool useCLAHE)
        {
            var channels = new VectorOfMat();
            if (image.NumberOfChannels > 1)
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2YCrCb);
                CvInvoke.Split(image, channels);
            }

            if (useCLAHE)
            {
                if (image.NumberOfChannels > 1)
                {
                    CvInvoke.CLAHE(channels[0],
                        (_configurations.CLAEH_CLIP_LIMIT < 1 ? 1 : _configurations.CLAEH_CLIP_LIMIT),
                        new Size((_configurations.CLAEH_TILE_X_SIZE < 1 ? 1 : _configurations.CLAEH_TILE_X_SIZE),
                        (_configurations.CLAEH_TILE_Y_SIZE < 1 ? 1 : _configurations.CLAEH_TILE_Y_SIZE)),
                        channels[0]);
                }
                else
                {
                    CvInvoke.CLAHE(image,
                        (_configurations.CLAEH_CLIP_LIMIT < 1 ? 1 : _configurations.CLAEH_CLIP_LIMIT),
                        new Size((_configurations.CLAEH_TILE_X_SIZE < 1 ? 1 : _configurations.CLAEH_TILE_X_SIZE),
                        (_configurations.CLAEH_TILE_Y_SIZE < 1 ? 1 : _configurations.CLAEH_TILE_Y_SIZE)),
                        image);
                }
            }
            else
                CvInvoke.EqualizeHist(channels[0], channels[0]);

            if (image.NumberOfChannels > 1)
            {
                CvInvoke.Merge(channels, image);
                CvInvoke.CvtColor(image, image, ColorConversion.YCrCb2Bgr);
            }


            //if (useCLAHE)
            //{
            //    var viewer = new ImageViewer(image, "Histogram Equalized CLAHE");
            //    viewer.Show();
            //}
            //else
            //{
            //    var viewer = new ImageViewer(image, "Histogram Equalized DEFAULT");
            //    viewer.Show();
            //}

            return image;
        }

        private void SetupTrainsShapeRanges(string maskPath)
        {
            object locker = new object();

            Mat shapeROIs = new Mat();
            if (_util.LoadBinaryMask(maskPath, ref shapeROIs))
            {
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(shapeROIs, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                int contoursSize = (int)contours.Size;

                var pop = new ParallelOptions { MaxDegreeOfParallelism = 5 };

                Parallel.For(0, contoursSize, pop, i =>
                {
                    double contourAspectRatio = _util.ComputeContourAspectRatio(contours[i]);
                    double contourCircularity = _util.ComputeContourCircularity(contours[i]);

                    lock (locker)
                    {
                        if (_contourAspectRatioRange.X == -1 || contourAspectRatio < _contourAspectRatioRange.X)
                        {
                            _contourAspectRatioRange.X = (float)contourAspectRatio;
                        }
                    }

                    lock (locker)
                    {
                        if (_contourAspectRatioRange.Y == -1 || contourAspectRatio > _contourAspectRatioRange.Y)
                        {
                            _contourAspectRatioRange.Y = (float)contourAspectRatio;
                        }
                    }

                    lock (locker)
                    {
                        if (_contourCircularityRange.X == -1 || contourCircularity < _contourCircularityRange.X)
                        {
                            _contourCircularityRange.X = (float)contourCircularity;
                        }
                    }

                    lock (locker)
                    {
                        if (_contourCircularityRange.Y == -1 || contourCircularity > _contourCircularityRange.Y)
                        {
                            _contourCircularityRange.Y = (float)contourCircularity;
                        }
                    }

                });
            }
        }

        private List<Result> DetectBanknotesTrain(Mat image, float minimumMatchAllowed = 0.07f, float minimuTargetAreaPercentage = 0.05f, float maxDistanceRatio = 0.75f, float reprojectionThresholPercentage = 0.01f,
            double confidence = 0.99, int maxIters = 5000, int minimumNumerInliers = 8)
        {
            object locker = new object();

            List<Result> detectorResults = new List<Result>();
            MKeyPoint[] mKeyPoints;
            SIFT sift = new SIFT();
            mKeyPoints = sift.Detect(image);
            VectorOfKeyPoint keypointsEvalImage = new VectorOfKeyPoint();
            keypointsEvalImage.Push(mKeyPoints);

            if (keypointsEvalImage.Size < 4)
            {
                return detectorResults;
            }

            Mat descriptorsEvalImage = new Mat();
            sift.Compute(image, keypointsEvalImage, descriptorsEvalImage);

            Features2DToolbox.DrawKeypoints(image, keypointsEvalImage, image, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.Default);

            float bestMatch = 0;
            Result bestDetectorResult = new Result();

            int trainDetectorsSize = DetectedBanknotes.Count;
            bool validDetection = true;
            float reprojectionThreshold = image.Cols * reprojectionThresholPercentage;

            do
            {
                bestMatch = 0;

                Parallel.For(0, trainDetectorsSize, i =>
                {
                    DetectedBanknotes[(int)i].UpdateCurrentLODIndex(ref image, 0.6999999881F);
                    Result detectorResult = DetectedBanknotes[(int)i].AnalyzeImageEval(ref keypointsEvalImage, ref descriptorsEvalImage, maxDistanceRatio, reprojectionThreshold, confidence, maxIters, minimumNumerInliers);
                    if (detectorResult.GetBestROIMatch() > minimumMatchAllowed)
                    {
                        float contourArea = (float)CvInvoke.ContourArea(detectorResult.GetTrainContour());
                        float imageArea = (float)(image.Cols * image.Rows);
                        float contourAreaPercentage = contourArea / imageArea;

                        if (contourAreaPercentage > minimuTargetAreaPercentage)
                        {
                            double contourAspectRatio = _util.ComputeContourAspectRatio(detectorResult.GetTrainContour());
                            if (contourAspectRatio > _contourAspectRatioRange.X && contourAspectRatio < _contourAspectRatioRange.Y)
                            {
                                double contourCircularity = _util.ComputeContourCircularity(detectorResult.GetTrainContour());
                                if (contourCircularity > _contourCircularityRange.X && contourCircularity < _contourCircularityRange.Y)
                                {
                                    if (CvInvoke.IsContourConvex(detectorResult.GetTrainContour()))
                                    {
                                        lock (locker)
                                        {
                                            if (detectorResult.GetBestROIMatch() > bestMatch)
                                            {
                                                bestMatch = detectorResult.GetBestROIMatch();
                                                bestDetectorResult = detectorResult;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

                validDetection = bestMatch > minimumMatchAllowed && bestDetectorResult.GetInliers().Size > minimumNumerInliers;

                if (bestDetectorResult != null && validDetection)
                {
                    detectorResults.Add(bestDetectorResult);
                    _util.RemoveInliersFromKeypointsAndDescriptors(bestDetectorResult.GetInliers(), ref keypointsEvalImage, ref descriptorsEvalImage);
                }

            } while (validDetection);

            return detectorResults;
        }


       
        public void LoadImagesTrain()
        {
            if (ImagesTrains == null)
            {
                LoadBanknotesTrains(true);
            }
        }


        public List<int> DetectBanknotesResults(ref Mat imageEval)
        {
            var imageBackup = imageEval.Clone();
            List<Result> detectorResultOut = DetectBanknotesTrain(imageEval);
            List<int> results = new List<int>();
            var total = 0.0;

            for (int i = 0; i < detectorResultOut.Count; ++i)
            {
                Result detectorResult = detectorResultOut[i];
                results.Add(detectorResult.GetTrainValue());

                Features2DToolbox.DrawKeypoints(imageEval, detectorResult.GetInliersKeypoints(), imageEval, new Bgr(0, 255, 0), Features2DToolbox.KeypointDrawType.Default);

                var valorTexto = detectorResult.GetTrainValue().ToString();

                //Message in language portuguese
                _synthesizer.SpeakAsync(valorTexto + "reais");

                total += Double.Parse(valorTexto);

                Mat imageMatchesSingle = new Mat();
                imageMatchesSingle = imageBackup;



                Mat matchesInliers = detectorResult.getInliersMatches(ref imageMatchesSingle);


                Rectangle boundingBox = CvInvoke.BoundingRectangle(detectorResult.GetTrainContour());
                _util.CorrectBoundingBox(ref boundingBox, imageEval.Cols, imageEval.Rows);
                InterfaceUtil.DrawLabelInCenterOfROI(valorTexto, ref imageEval, ref boundingBox);
                InterfaceUtil.DrawLabelInCenterOfROI(valorTexto, ref matchesInliers, ref boundingBox);
                _util.DrawContour(ref imageEval, detectorResult.GetTrainContour(), detectorResult.GetTrainContourColor(), 2);
                _util.DrawContour(ref matchesInliers, detectorResult.GetTrainContour(), detectorResult.GetTrainContourColor(), 2);
            }

            if (total == 0)
            {
                //Message in language portuguese
                _synthesizer.SpeakAsync("Nenhuma cédula foi identificada");
            }
            else
            {
                ImageViewer iv = new ImageViewer();

                iv = new ImageViewer(imageEval, "Result ");
                iv.Show();

                //Message in language portuguese
                _synthesizer.SpeakAsync("Valor total é " + total + " reais");
            }

            results.Sort();

            return results;
        }
    }
}
