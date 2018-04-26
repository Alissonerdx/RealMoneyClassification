using Emgu.CV;
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
        private VectorOfMat _targetsImage;
        private List<VectorOfKeyPoint> _keypointsImageTarget;
        private VectorOfMat _descriptorsImageTarget;
        private List<List<int>> _indexKeypointsImageTargetAssociatedROI;
        private List<List<int>> _numberKeypointsImageTargetInContour;
        private Util _util;
        public int ValueBanknote { get; set; }
        public MCvScalar ColorContour { get; set; }
        public bool GlobalMatch { get; set; }

        private int _LODIndex;

        public DetectBanknote(int valueBanknote, MCvScalar colorContour, bool globalMatch)
        {
            this.ValueBanknote = valueBanknote;
            this.ColorContour = colorContour;
            this.GlobalMatch = globalMatch;
            _LODIndex = 0;
            _keypointsImageTarget = new List<VectorOfKeyPoint>();
            _descriptorsImageTarget = new VectorOfMat();
            _indexKeypointsImageTargetAssociatedROI = new List<List<int>>();
            _numberKeypointsImageTargetInContour = new List<List<int>>();
            _targetsImage = new VectorOfMat();
            _util = new Util();
        }

        public VectorOfKeyPoint GetKeypointsTarget()
        {
            return _keypointsImageTarget[_LODIndex];
        }

        public bool ConfigRecognitionImageTarget(Mat imageTarget, Mat roiTarget, bool useGlobalMatch)
        {
            _targetsImage.Push(imageTarget);


            _LODIndex = _targetsImage.Size - 1;

            SIFT sift = new SIFT();

            //Insere os pontos chaves da imagem alvo na lista de pontos chaves
            _keypointsImageTarget.Insert(_LODIndex, new VectorOfKeyPoint(sift.Detect(_targetsImage[_LODIndex], roiTarget)));
            if (_keypointsImageTarget[_LODIndex] != null && _keypointsImageTarget[_LODIndex].Size < 4)
                return false;

            //Calcula os descritores dos pontos chaves extraidos, no caso se extrair poucos descritores ele return false = não reconhecido
            sift.Compute(_targetsImage[_LODIndex], _keypointsImageTarget[_LODIndex], _descriptorsImageTarget[_LODIndex]);
            if (_descriptorsImageTarget[_LODIndex].Rows < 4)
                return false;

            if (useGlobalMatch)
                return true;
            else
                return ConfigureImageTargetROI(_keypointsImageTarget[_LODIndex], roiTarget);

        }


        public bool ConfigureImageTargetROI(VectorOfKeyPoint keypointsImageTarget, Mat roiTarget)
        {
            if (keypointsImageTarget == null)
                return false;

            VectorOfVectorOfPoint contoursImageTargetROI = new VectorOfVectorOfPoint();
            VectorOfPointF hierarchyContours = new VectorOfPointF();

            //Extrai contornos da imagem definida como ROI (Mascara) e a hierarquia destes contornos
            CvInvoke.FindContours(roiTarget.Clone(), contoursImageTargetROI, hierarchyContours, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            if (contoursImageTargetROI.Size == 0)
                return false;

            int numberKeypointsImageTarget = keypointsImageTarget.Size;

            var pop = new ParallelOptions { MaxDegreeOfParallelism = 5 };

            for (int indexKeypointImageTarget = 0; indexKeypointImageTarget < numberKeypointsImageTarget; ++indexKeypointImageTarget)
            {
                Parallel.For(0, contoursImageTargetROI.Size, pop, i =>
                {
                    PointF pointXY = keypointsImageTarget[indexKeypointImageTarget].Point;
                    if (CvInvoke.PointPolygonTest(contoursImageTargetROI, pointXY, false) >= 0)
                    {
                        _indexKeypointsImageTargetAssociatedROI[_LODIndex][indexKeypointImageTarget] = i;
                        return;
                    }
                });
            }

            _numberKeypointsImageTargetInContour[_LODIndex].Clear();
            _numberKeypointsImageTargetInContour[_LODIndex] = new List<int>(contoursImageTargetROI.Size);

            for (int i = 0; i < _indexKeypointsImageTargetAssociatedROI[_LODIndex].Count; ++i)
            {
                var indexContour = _indexKeypointsImageTargetAssociatedROI[_LODIndex][i];
                ++_numberKeypointsImageTargetInContour[_LODIndex][indexContour];
            }

            return true;
        }
    }
}

