using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirstImageTry
{
    public partial class Form1 : Form
    {
        Neuron[] Inp;
        Neuron[] Outp;
        Neuron[][] Hiden;

        //Parametres of network
        int countInput = 2;
        int countOutput = 1;

        int HidenLevels = 2;
        int countHiden = 4;
        int countShift = 1;

        double E = 0.1; //graduation speed
        double A = 0.0001; //moment

        Dictionary<double[], double[]> TrainingSet = new Dictionary<double[], double[]>();
        Dictionary<double[], double[]> TestingSet = new Dictionary<double[], double[]>();

        int epoch = 0;
        bool reverse = false; //True, if values are >1 and need to normalize em

        public Form1()
        {
            InitializeComponent();
            InitNeural();

            GetTraining();
            GetTesting();
            int b = 0;
            PerformTest();
        }

        public void InitNeural()
        {
            //Creating Levels

            Inp = new Neuron[countInput];
            Outp = new Neuron[countOutput];
            Hiden = new Neuron[HidenLevels][];
            for (int i = 0; i < HidenLevels; i++)
                Hiden[i] = new Neuron[countHiden];

            //Creating Neurons
            for (int i = 0; i < countInput; i++)
                Inp[i] = new Neuron();

            for (int j = 0; j < HidenLevels; j++)
                for (int i = 0; i < countHiden; i++)
                {
                    Hiden[j][i] = new Neuron();
                    if (i >= countHiden - countShift)
                        Hiden[j][i].isShift = true; //making shift neurons
                }

            //Creating Sinapses
            for (int i = 0; i < countInput; i++)
            {
                Inp[i].next = Hiden[0].Take(countHiden - countShift).ToArray(); //Creating sinapses without next level's shift neurons
                Inp[i].InitWeights(countHiden);
            }
            for (int j = 0; j < HidenLevels; j++)
                for (int i = 0; i < countHiden; i++)
                {
                    if (j != HidenLevels - 1)
                    {
                        Hiden[j][i].next = Hiden[j + 1].Take(countHiden-countShift).ToArray(); //Creating sinapses without next level's shift neurons
                        Hiden[j][i].InitWeights(countHiden);
                    }
                    else
                    {
                        Hiden[j][i].next = Outp;
                        Hiden[j][i].InitWeights(countOutput);
                    }
                        if (j != 0)
                            Hiden[j][i].previous = Hiden[j - 1];
                        else
                            Hiden[j][i].previous = Inp;
                }

            for (int i = 0; i < countOutput; i++)
            {
                Outp[i] = new Neuron();
                Outp[i].previous = Hiden[HidenLevels - 1];
            }
        }

        public double[] RunForward(double[] Input)
        {
            double[] result = new double[countOutput];

            for (int i = 0; i < Input.Length; i++)
                Inp[i].DoJob(Input[i]);

            for (int j = 0; j < HidenLevels; j++)
                foreach (var N in Hiden[j])
                    N.DoJob();

            foreach (var N in Outp)
                N.DoJob();

            for (int i = 0; i < countOutput; i++)
                result[i] = Outp[i].value;

            return result;
        }

        public void ClearInputs()
        {
            foreach (var N in Inp[0].next)
                N.input = 0;

            for (int i = 0; i < HidenLevels; i++)
                foreach (var N in Hiden[i][0].next)
                    N.input = 0;
        }

        public double[] GetMSE(double[] Input, double[] Ideal)
        {
            double[] Result = Ideal;
            ClearInputs();
            RunForward(Input);
            for (int i = 0; i < countOutput; i++)
                Result[i] = (Ideal[i] - Outp.ElementAt(i).value) * (Ideal[i] - Outp.ElementAt(i).value);
            return Result;
        }

        public void ChangeDeltas(double[] Ideal)
        {
            for (int i = 0; i < Ideal.Length; i++)
                Outp[i].GetDelta(Ideal[i]);
            for (int j = HidenLevels - 1; j >= 0; j--)
                for (int i = 0; i < countHiden; i++)
                    Hiden[j][i].GetDelta();
            for (int i = 0; i < countInput; i++)
                Inp[i].GetDelta();
        }
        public void DoChanges()
        {
            for (int j = HidenLevels - 1; j >= 0; j--)
                for (int i = 0; i < countHiden; i++)
                    Hiden[j][i].DoChange(E, A);
            for (int i = 0; i < countInput; i++)
                Inp[i].DoChange(E, A);
        }

        public void TrainSet(double[] Input, double[] Ideal)
        {
            ClearInputs();
            RunForward(Input);
            ChangeDeltas(Ideal);
            DoChanges();
        }

        private void GetTesting()
        {
            StreamReader f = new StreamReader(@"Testing.txt");
            ReadSet(TestingSet, f);
        }
        private void GetTraining()
        {
            StreamReader f = new StreamReader(@"Training.txt");
            ReadSet(TrainingSet, f);
        }
        private void ReadSet(Dictionary<double[], double[]> TargetSet, StreamReader f)
        {
            double[] a;
            double[] b;
            while (!f.EndOfStream)
            {
                a = new double[countInput];
                b = new double[countOutput];
                string buffer = f.ReadLine(), t;
                int sIndex;
                char[] separators = { ' ', '\t' };
                for (int i = 0; i < countInput; i++)
                {
                    sIndex = buffer.IndexOfAny(separators);
                    t = buffer.Substring(0, sIndex);
                    a[i] = Double.Parse(t);
                    buffer = buffer.Substring(sIndex + 1);
                }
                for (int i = 0; i < countOutput; i++)
                {
                    sIndex = buffer.IndexOf(' ');
                    if (sIndex > 0)
                    {
                        t = buffer.Substring(0, sIndex);
                        b[i] = Double.Parse(t);
                        buffer = buffer.Substring(sIndex + 1);
                    }
                    else
                        b[i] = Double.Parse(buffer);
                }
                if (reverse || a.Max() > 1 || b.Max() > 1)
                {
                    for (int i = 0; i < a.Length; i++) a[i] = 1 / (1 + a[i]);
                    for (int i = 0; i < b.Length; i++) b[i] = 1 / (1 + b[i]);
                    reverse = true;
                }
                TargetSet.Add(a, b);
            }
        }

        private void PerformTest()
        {
            Label[] labels = { label1, label2, label3, label4, label5, label6, label7, label8, label9, label10 };
            for (int i = 0; i < 10; i++)
            {
                string label = "";
                double MSE = 1;
                for (int j = 0; j < 10; j++)
                {
                    epoch++;
                    foreach (var key in TrainingSet.Keys)
                    {
                        TrainSet(key, TrainingSet[key]);
                    }
                    double temp = 0;
                    string t = "";
                    foreach (var key in TestingSet.Keys)
                    {
                        double a = GetMSE(key, TestingSet[key])[0];
                        temp += a;
                        t += Math.Round(a, 3).ToString() + "\n";
                    }
                    temp /= TestingSet.Count;
                    if (temp < MSE)
                    {
                        MSE = temp;
                        label = epoch + " epoch\n\n" + Math.Round(MSE, 3).ToString() + "\n\n" + t;
                    }
                }

                labels[i].Text = label;
            }
        }
    }
}
