using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public static class InterfaceUtil
    {
        public const int TEXT_MIN_SIZE = 12;

        public static void DrawEvalLabel(string text, ref Mat image, ref Rectangle imageBoundingRect, float labelHeightPercentage = 0.399999994f, float textThicknessPercentage = 0.1000000015f)
        {
            int textBoxHeight = (int)(imageBoundingRect.Height * labelHeightPercentage);
            double scale = (double)textBoxHeight / 46.0;
            int thickness = Math.Max(1, (int)(textBoxHeight * textThicknessPercentage));
            Rectangle textBoundingRect = imageBoundingRect;
            textBoundingRect.Height = Math.Max(textBoxHeight, TEXT_MIN_SIZE);

            Size textSize = new Size(143, 8);
            Point textBottomLeftPoint = new Point(textBoundingRect.X + (textBoundingRect.Width - textSize.Width) / 2, textBoundingRect.Y + (textBoundingRect.Height + textSize.Height) / 2);

            //CvInvoke.Rectangle(image, imageBoundingRect, new MCvScalar(45, 255, 255), 2);
            //CvInvoke.Rectangle(image, textBoundingRect, new MCvScalar(45, 255, 255), 2);
            CvInvoke.PutText(image, text, textBottomLeftPoint, Emgu.CV.CvEnum.FontFace.HersheySimplex, scale, new MCvScalar(214, 60, 5));
        }

        public static void DrawLabelInCenterOfROI(string text, ref Mat image, ref Rectangle roiBoundingRect, float labelHeightPercentage = 0.399999994f, float textThicknessPercentage = 0.1000000015f)
        {
            int textBoxHeight = (int)(roiBoundingRect.Height * labelHeightPercentage);
            double scale = (double)textBoxHeight / 46.0;
            int thickness = Math.Max(1, (int)(textBoxHeight * textThicknessPercentage));

            Rectangle textBoundingRect = roiBoundingRect;
            textBoundingRect.Height = Math.Max(textBoxHeight, TEXT_MIN_SIZE);
            textBoundingRect.Y += (int)((roiBoundingRect.Height - textBoundingRect.Height) / 2.0);

            Size textSize = new Size(143, 8);
            Point textBottomLeftPoint = new Point(textBoundingRect.X + (textBoundingRect.Width - textSize.Width) / 2, textBoundingRect.Y + (textBoundingRect.Height + textSize.Height) / 2);

            //CvInvoke.Rectangle(image, roiBoundingRect, new MCvScalar(45, 255, 255), 2);
            //CvInvoke.Rectangle(image, textBoundingRect, new MCvScalar(45, 255, 255), 2);
            CvInvoke.PutText(image, text, textBottomLeftPoint, Emgu.CV.CvEnum.FontFace.HersheySimplex, scale, new MCvScalar(214, 60, 5));
        }
    }
}
