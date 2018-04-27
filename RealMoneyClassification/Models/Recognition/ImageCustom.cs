using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public class ImageCustom
    {
        public string FileName { get; set; }
        public VectorOfKeyPoint KeyPoints { get; set; }
        public Mat Descriptors { get; set; }
        public Mat Image { get; set; }
    }
}
