using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using ReconhecimentoCedulas_2._0.Models;
using ReconhecimentoCedulas_2._0.Models.Recognition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReconhecimentoCedulas_2._0
{
    public partial class MainView : Form
    {
        private string _pathImageMain;
        private Bitmap _imageMain;
        private SvmBof _svmBof;
        private Recognition _recognition;
        private ImageAnalyze _imageAnalyze;

        public MainView()
        {
            InitializeComponent();
            _pathImageMain = String.Empty;
            _imageMain = null;
            _svmBof = new SvmBof();
            _recognition = new Recognition();
        }
        

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _pathImageMain = ofd.FileName;
                _imageMain = new Bitmap(_pathImageMain);
                var image = new Image<Bgr, Byte>(_imageMain);
                ImageViewer imv = new ImageViewer(image, "Image Main");
                imv.Show();
            }
        }

        private void sVMTrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _svmBof.RunTrain();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void sVMEvaluateOneImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_imageMain != null)
                {
                    var image = new Image<Bgr, Byte>(_pathImageMain);
                    _svmBof.RunEvaluateOneImage(image);
                }
                else
                {
                    MessageBox.Show("Select One Image for Evaluate");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void sVMEvaluateEvalPasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _svmBof.RunEvaluateManyImages();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// Recognition (LoadBanknotesTrain) => ImageAnalyze (PreProcess) => Recognition (DetectBanknotesResults)
        /// </summary>
        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_pathImageMain != null)
            {
                _recognition = new Recognition();
                _recognition.LoadImagesTrain();

                Stopwatch watch = new Stopwatch();

                _imageAnalyze = new ImageAnalyze(_recognition);
                var imageResult = new Mat(_pathImageMain, LoadImageType.Color);

                watch.Start();

                _imageAnalyze.RunEvaluateImage(imageResult);

                watch.Stop();

                MessageBox.Show($"Time Evaluate: {watch.Elapsed}");
            }
            else
            {
                MessageBox.Show("Select One Image for Evaluate");
            }
        }
    }
}
