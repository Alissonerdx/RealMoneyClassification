using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public class ImageAnalyze
    {
        private Recognition _recognition;
        private Mat _imagePreProcessed;
        private Mat _imageProcessed;

        public ImageAnalyze(Recognition recognition)
        {
            _recognition = recognition;
        }

        private bool PreProcess(Mat imageOriginal)
        {
            if (imageOriginal != null)
            {
                _imagePreProcessed = imageOriginal.Clone();
                _imageProcessed = _recognition.PreProcess(ref _imagePreProcessed);
                return true;
            }

            return false;
        }

        public bool RunEvaluateImage(Mat imageOriginal)
        {
            if(imageOriginal != null)
            {
                if(PreProcess(imageOriginal))
                {
                    _recognition.DetectBanknotesResults(ref imageOriginal);
                    return true;
                }
            }
            return false;
        }
    }
}
