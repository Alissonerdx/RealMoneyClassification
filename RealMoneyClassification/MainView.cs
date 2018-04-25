using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using ReconhecimentoCedulas_2._0.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReconhecimentoCedulas_2._0
{
    public partial class MainView : Form
    {
        private string pathImageMain;
        private Bitmap imageMain;
        private SvmBof SvmBof;

        public MainView()
        {
            InitializeComponent();
            pathImageMain = String.Empty;
            imageMain = null;
            SvmBof = new SvmBof();
        }
        

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pathImageMain = ofd.FileName;
                imageMain = new Bitmap(pathImageMain);
                var image = new Image<Bgr, Byte>(imageMain);
                ImageViewer imv = new ImageViewer(image, "Image Main");
                imv.Show();
            }
        }

        private void sVMTrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SvmBof.RunTrain();
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
                var image = new Image<Bgr, Byte>(pathImageMain);
                SvmBof.RunEvaluateOneImage(image);
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
                SvmBof.RunEvaluateManyImages();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
