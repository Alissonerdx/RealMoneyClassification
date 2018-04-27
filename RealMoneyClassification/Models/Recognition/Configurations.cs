using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReconhecimentoCedulas_2._0.Models.Recognition
{
    public class Configurations
    {
        public string Path { get; set; }

        public string PATH_LIST_BANKNOTES_ASSOCIATED_LABELS = String.Empty;
        public string PATH_BANKNOTES = String.Empty;
        public string PATH_BANKNOTE_SHAPE = String.Empty;
        public string MASK = "_mask.";
        public int BILATERAL_FILTER_DISTANCE = 8;
        public int BILATERAL_FILTER_SIGMA_COLOR = 16;
        public int BILATERAL_FILTER_SIGMA_SPACE = 8;
        public int CLAEH_CLIP_LIMIT = 2;
        public int CLAEH_TILE_X_SIZE = 4;
        public int CLAEH_TILE_Y_SIZE = 4;
        public int CONTRAST_MULTIPLIED_BY_10 = 9;
        public int BRIGHTNESS_MULTIPLIED_BY_10 = 24;

        public Configurations()
        {
            Path = Application.StartupPath;
            Path = Directory.GetParent(Directory.GetParent(Path).ToString()).ToString();
            PATH_LIST_BANKNOTES_ASSOCIATED_LABELS = $@"{Path}\Models\Recognition\Train\BanknotesConfig.txt";
            PATH_BANKNOTES = $@"{Path}\Models\Recognition\Train\";
            PATH_BANKNOTE_SHAPE = $@"{Path}\Models\Recognition\Train\BanknoteShapes.png";
        }
    }
}
