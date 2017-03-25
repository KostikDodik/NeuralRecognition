using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstImageTry
{
    public class Neuron
    {
        private static Random rand = new Random();
        public double[] weights;
        public double[] changes;
        public Neuron[] next;
        public Neuron[] previous;
        public double value;
        public double input;
        public double delta;
        public bool isShift;

        public double sigmoid(double Input)
        {
            return 1 / (1 + Math.Exp(-Input));
        }

        public void DoJob()
        {
            if (isShift)
                value = 1;
            else
                value = sigmoid(input);
            if (next != null)
                for (int i = 0; i < next.Length; i++)
                    next[i].input += value * weights[i];
            input = 0;

        }

        public void DoJob(double Output)
        {
            value = Output;
            for (int i = 0; i < next.Length; i++)
                next[i].input += value * weights[i];
            input = 0;
        }

        public double GetDelta()
        {
            delta = 0;
            for (int i = 0; i < next.Length; i++)
                delta += weights.ElementAt(i) * next.ElementAt(i).delta;
            delta *= (1 - value) * value;
            return delta;
        }

        public double GetDelta(double ideal)
        {
            delta =(ideal - value) * (1 - value) * value;
            return delta;
        }

        public void DoChange(double E, double A)
        {
            for (int i = 0; i < next.Length; i++)
            {
                changes[i] = E * value * next[i].delta + A * changes[i];
                weights[i] += changes[i];
            }
        }

        public void InitWeights(int count)
        {
            weights = new double[count];
            changes = new double[count];
            for (int i = 0; i < count; i++)
                weights[i] = rand.NextDouble();
        }
    }
}
