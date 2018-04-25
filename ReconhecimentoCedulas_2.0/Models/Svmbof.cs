using Emgu.CV.Features2D;
using Emgu.CV.XFeatures2D;
using System.Speech.Synthesis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.ML;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ReconhecimentoCedulas_2._0.Models
{
    public class SvmBof
    {
        ///<summary>
        ///Esta classe está relacionada ao treinamento e uso do SVM (Support Vector Machine) e do BOF/BOW (Bag of Feature)
        ///</summary>

        private BFMatcher matcher = new BFMatcher(DistanceType.L2); //Distancia Euclidiana
        private SIFT extractor = new SIFT();
        private SpeechSynthesizer synthesizer;

        //Define a quantidade cluster que será utilizado
        private int dictionarySize = 1500;
        private MCvTermCriteria tc = new MCvTermCriteria(10, 0.001);
        private int retries = 1;
        private BOWKMeansTrainer bowTrainer;
        private BOWImgDescriptorExtractor bowDE;
        private Dictionary<int, int> banknoteSizes;
        private List<int> banknotes = new List<int> { 2, 5, 10, 20, 50, 100 };
        private Dictionary<string, Image<Bgr, Byte>> listaImagensTreino = new Dictionary<string, Image<Bgr, byte>>();
        private Dictionary<string, MKeyPoint[]> listaKeyPointsImagensTreino = new Dictionary<string, MKeyPoint[]>();
        private Dictionary<string, Mat> listaDescritoresImagensTreinoSIFT = new Dictionary<string, Mat>();
        private string Path;

        private Mat featureUnclustered;

        public SvmBof()
        {
            //Utiliza KMeans
            bowTrainer = new BOWKMeansTrainer(dictionarySize, tc, retries, KMeansInitType.PPCenters);
            bowDE = new BOWImgDescriptorExtractor(extractor, matcher);
            banknoteSizes = new Dictionary<int, int>();
            featureUnclustered = new Mat();
            synthesizer = new SpeechSynthesizer();
            Path = Application.StartupPath;
            Path = Directory.GetParent(Directory.GetParent(Path).ToString()).ToString();
            banknoteSizes.Add(2, 35); //2 Reais = 35 cédulas usadas para o treinamento
            banknoteSizes.Add(5, 17); //5 Reais = 17 cédulas usadas para o treinamento
            banknoteSizes.Add(10, 20); //10 Reais = 20 cédulas usadas para o treinamento
            banknoteSizes.Add(20, 22); //20 Reais = 22 cédulas usadas para o treinamento
            banknoteSizes.Add(50, 25); //50 Reais = 25 cédulas usadas para o treinamento
            banknoteSizes.Add(100, 24); //100 Reais = 24 cédulas usadas para o treinamento
        }

        /// <summary>
        /// Utilizado para carregar as imagens de treinamento e gerar o arquivo de descritores
        /// </summary>
        private void LoadTrain()
        {
            Image<Bgr, Byte> image;
            for (int i = 0; i < banknotes.Count; i++)
            {
                var banknote = banknotes[i];
                //J = 1 pois é o indice que está no nome das imagens de treinamento
                for (int j = 1; j <= banknoteSizes[banknote]; j++)
                {
                    var nameFile = $@"{Path}\Train\{banknote} ({j}).jpg";
                    image = new Image<Bgr, Byte>(nameFile);
                    listaImagensTreino.Add(nameFile, image);
                    MKeyPoint[] keypoints;
                    keypoints = extractor.Detect(image);
                    listaKeyPointsImagensTreino.Add(nameFile, keypoints);
                    Mat features = new Mat();
                    extractor.Compute(image, new VectorOfKeyPoint(keypoints), features);
                    featureUnclustered.PushBack(features);
                }
            }
            //Armazenando os descritores processados da etapa de cima em um arquivo train_descriptors.yml
            FileStorage fs = new FileStorage($@"{Path}\SVM Datasets\train_descriptors.yml", FileStorage.Mode.Write);
            //Adicionando Label train_descriptors ao arquivo train_descriptors.yml
            fs.Write(featureUnclustered, "train_descriptors");
            fs.ReleaseAndGetString();
            //Adicionando descritores não processados no BOW
            bowTrainer.Add(featureUnclustered);
        }

        /// <summary>
        /// Utilizado para gerar os arquivos de treinados para o SVM
        /// </summary>
        public void RunTrain()
        {
            Image<Bgr, Byte> image;
            Console.WriteLine("Quantização vetorial...");

            //Chama o método responsavel por carregar as imagens de treinamento e extrair os descritores
            LoadTrain();

            //Obter a quantidade de caracteristicas (descritores) adicionados no BOW 
            int count = bowTrainer.DescriptorCount;
            Console.WriteLine($"Clustering {count} características");
            Mat dictionary = new Mat();
            //Gera os vocabularios a partir dos descritores
            bowTrainer.Cluster(dictionary);

            //Cria um arquivo chamado train_clustered.yml com os vocabularios
            FileStorage fs = new FileStorage($@"{Path}\SVM Datasets\train_clustered.yml", FileStorage.Mode.Write);
            //Adiciona ao arquivo train_clustered.yml um label vocabulary
            fs.Write(dictionary, "vocabulary");
            fs.ReleaseAndGetString();
            //Realiza a extração dos histogramas em forma de BOW para cada imagem do treinamento
            bowDE.SetVocabulary(dictionary);
            Console.WriteLine("Extraindo histogramas na forma de BOW para cada imagem");
            //Labels é o nome da imagem que foi treinada, é utilizado para descobrir qual é o valor da imagem
            Matrix<int> labels;
            List<int> listLabels = new List<int>();
            Mat trainingData = new Mat(0, dictionarySize, DepthType.Cv32F, 1);
            MKeyPoint[] keypoints;
            Mat bowDescriptor = new Mat();

            //var notas = new List<int> { 2, 5, 10, 20, 50, 100 };
            for (int i = 0; i < banknotes.Count; i++)
            {
                var banknote = banknotes[i];
                for (int j = 1; j <= banknoteSizes[banknote]; j++)
                {
                    var fileName = $@"{Path}\Train\{banknote} ({j}).jpg";
                    image = new Image<Bgr, Byte>(fileName);
                    keypoints = extractor.Detect(image);
                    bowDE.Compute(image, new VectorOfKeyPoint(keypoints), bowDescriptor);
                    trainingData.PushBack(bowDescriptor);
                    listLabels.Add(banknote);
                }
            }

            //Cria um arquivo com os dados treinados chamado trained_data.yml
            fs = new FileStorage($@"{Path}\SVM Datasets\trained_data.yml", FileStorage.Mode.Write);
            //Adiciona um label data ao arquivo trained_data.yml
            fs.Write(trainingData, "data");
            fs.ReleaseAndGetString();

            labels = new Matrix<int>(listLabels.ToArray());
            //Cria o arquivo trained_labels.yml que aramaze os labels das imagens que foram treinadas
            fs = new FileStorage($@"{Path}\SVM Datasets\trained_labels.yml", FileStorage.Mode.Write);
            //Adiciona o label no arquivo trained_labels.yml chamado labels
            fs.Write(labels.Mat, "labels");
            fs.ReleaseAndGetString();

            Console.WriteLine("Treinamento Concluido");

            //Obs.: trained_data.yml, trained_labels.yml e train_clustered.yml são os arquivos mais importantes,
            //os arquivos contem todo o conhecimento necessario para o SVM
        }

        /// <summary>
        /// Executa a avaliação de todas imagens que se encontram na pasta Eval, necessario ter os arquivos trained_data.yml, trained_labels.yml e train_clustered.yml nestá etapa
        /// Está etapa realiza a classificação de uma cédula baseado nos arquivos de treinamento
        /// </summary>
        public void RunEvaluateManyImages()
        {
            //Todos estes parametros podem ser modificados para tentar melhorar a classificação
            SVM svm = new SVM();
            svm.SetKernel(SVM.SvmKernelType.Rbf);
            svm.Type = SVM.SvmType.CSvc;
            svm.Gamma = 0.50625000000000009;
            svm.C = 312.50000000000000;

            svm.TermCriteria = new MCvTermCriteria(100, 0.000001);

            var dictionary = new Mat();
            //Abre o arquivo train_clustered.yml
            FileStorage fs = new FileStorage($@"{Path}\SVM Datasets\train_clustered.yml", FileStorage.Mode.Read);
            //Obtem os dados vinculados ao label vocabulary
            fs["vocabulary"].ReadMat(dictionary);
            fs.ReleaseAndGetString();
            //Adiciona os dados para o BOW Extractor para recuperar os histogramas do BOW
            bowDE.SetVocabulary(dictionary);

            var trainedData = new Mat();
            //Abre o arquivo trained_data.yml
            fs = new FileStorage($@"{Path}\SVM Datasets\trained_data.yml", FileStorage.Mode.Read);
            //Obtem os dados vinculados ao label data
            fs["data"].ReadMat(trainedData);
            fs.ReleaseAndGetString();

            var labels = new Mat();
            //Abre o arquivo trained_labels.yml
            fs = new FileStorage($@"{Path}\SVM Datasets\trained_labels.yml", FileStorage.Mode.Read);
            //Obtem os dados vinculados ao label labels
            fs["labels"].ReadMat(labels);
            fs.ReleaseAndGetString();

            //Adiciona os parametros ao SVM para treino e diz que os valores vão estar em cada linha do arquivo trainedData
            bool resp = svm.Train(trainedData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, labels);

            //Cria uma lista para adicionar as cédulas da avaliacao
            List<float> listgroundTruth = new List<float>();
            Matrix<float> groundTruth;
            Mat evalData = new Mat();
            MKeyPoint[] keypoints = null;
            var bowDescriptor = new Mat();
            //Cria outra lista com as respostas
            List<float> listResults = new List<float>();
            Matrix<float> results;
            Image<Bgr, Byte> image;

            var messageGeneral = "";
            //Cria um dicionario para informar quais são as cédulas que serão avaliadas (informar somente as céduls que estão na pasta Eval) e as quantidades
            banknoteSizes = new Dictionary<int, int>();
            banknoteSizes.Add(2, 4);
            banknoteSizes.Add(10, 4);
            banknoteSizes.Add(20, 4);
            banknoteSizes.Add(50, 4);
            banknoteSizes.Add(100, 3);

            for (int i = 0; i < banknotes.Count; i++)
            {
                var banknote = banknotes[i];
                if (banknoteSizes.ContainsKey(banknote))
                {
                    for (int j = 1; j <= banknoteSizes[banknote]; j++)
                    {
                        var fileName = $@"{Path}\Eval\{banknote} ({j}).jpg";
                        image = new Image<Bgr, Byte>(fileName);
                        //Neste passo é extraido os pontos chaves da imagem que está sendo classificada
                        keypoints = extractor.Detect(image);
                        //Depois é extraido os descritores
                        bowDE.Compute(image, new VectorOfKeyPoint(keypoints), bowDescriptor);
                        evalData.PushBack(bowDescriptor);
                        listgroundTruth.Add(banknote);
                        //Neste passo é realizado a predição baseado nos dados do treinamento (quanto mais imagens de treinamento melhor a predição)
                        var response = svm.Predict(bowDescriptor);
                        //Verifico se o response corresponde ao valor da cédula que está sendo avaliada
                        //Se sim "Sucesso" senão "Falha"
                        var status = response == banknote ? "Sucesso" : "Falha";
                        var mensagem = $"Nota real avaliada {banknote} / nota reconhecida {response} - {status}";
                        Console.WriteLine(mensagem);
                        messageGeneral += mensagem + "\n";
                        listResults.Add(response);
                    }
                }
            }

            groundTruth = new Matrix<float>(listgroundTruth.ToArray());
            results = new Matrix<float>(listResults.ToArray());

            var resultado = new Mat();
            CvInvoke.Subtract(groundTruth.Mat, results.Mat, results);
            //Faz a conta para saber a taxa de erro
            double errorRate = (double)CvInvoke.CountNonZero(results) / evalData.Rows;
            var mensagemErro = $"Taxa de erro é {errorRate * 100} %";
            messageGeneral += mensagemErro;
            Console.WriteLine(mensagemErro);
            MessageBox.Show(messageGeneral);
        }

        /// <summary>
        /// Executa a avaliação de uma imagem de entrada, necessario ter os arquivos trained_data.yml, trained_labels.yml e train_clustered.yml nestá etapa
        /// Está etapa realiza a classificação de uma cédula baseado nos arquivos de treinamento
        /// </summary>
        public void RunEvaluateOneImage(Image<Bgr, Byte> imagem)
        {
            Stopwatch stopwatch = new Stopwatch();

            //Todos estes parametros podem ser modificados para tentar melhorar a classificação
            SVM svm = new SVM();
            svm.SetKernel(SVM.SvmKernelType.Rbf);
            svm.Type = SVM.SvmType.CSvc;
            svm.Gamma = 0.50625000000000009;
            svm.C = 312.50000000000000;

            svm.TermCriteria = new MCvTermCriteria(100, 0.000001);

            var dictionary = new Mat();
            //Abre o arquivo train_clustered.yml
            FileStorage fs = new FileStorage($@"{Path}\SVM Datasets\train_clustered.yml", FileStorage.Mode.Read);
            //Obtem os dados vinculados ao label vocabulary
            fs["vocabulary"].ReadMat(dictionary);
            fs.ReleaseAndGetString();
            //Adiciona os dados para o BOW Extractor para recuperar os histogramas do BOW
            bowDE.SetVocabulary(dictionary);

            var trainedData = new Mat();
            //Abre o arquivo trained_data.yml
            fs = new FileStorage($@"{Path}\SVM Datasets\trained_data.yml", FileStorage.Mode.Read);
            //Obtem os dados vinculados ao label data
            fs["data"].ReadMat(trainedData);
            fs.ReleaseAndGetString();

            var labels = new Mat();
            //Abre o arquivo trained_labels.yml
            fs = new FileStorage($@"{Path}\SVM Datasets\trained_labels.yml", FileStorage.Mode.Read);
            //Obtem os dados vinculados ao label labels
            fs["labels"].ReadMat(labels);
            fs.ReleaseAndGetString();

            //Iniciar Timer
            stopwatch.Start();

            //Adiciona os parametros ao SVM para treino e diz que os valores vão estar em cada linha do arquivo trainedData
            bool resp = svm.Train(trainedData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, labels);

            MKeyPoint[] keypoints = null;
            var bowDescriptor = new Mat();

            keypoints = extractor.Detect(imagem);
            bowDE.Compute(imagem, new VectorOfKeyPoint(keypoints), bowDescriptor);

            //Neste passo é realizado a predição baseado nos dados do treinamento (quanto mais imagens de treinamento melhor a predição)
            var response = svm.Predict(bowDescriptor);

            //Parar Timer
            stopwatch.Stop();

            MessageBox.Show($"Resultado {response} reais, Tempo: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}", "Resultado");

            //Aciona o speech do windows para falar o valor da cédula
            synthesizer.SpeakAsync(response + "reais");
        }
    }
}
