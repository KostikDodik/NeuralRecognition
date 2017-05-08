using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstImageTry
{
    class CNMatrix
    {
        public double[,] matrix;
        public double[][] Layer;
        public CNMatrix[] nextStage;

        private static Random rand = new Random();
        public CNMatrix(int size, int countOfNext)
        {
            matrix = new double[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    matrix[x, y] = rand.Next(10);
            nextStage = new CNMatrix[countOfNext];
        }
        public CNMatrix(int size)
        {
            matrix = new double[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    matrix[x, y] = rand.Next(10);
            nextStage = null;
        }


        public static double[][] Convolution(double[][] inputLayer,double[,] matrix)
        {
            int width=inputLayer.Length;
            int height=inputLayer[0].Length;
            double matrixSize=(int)Math.Sqrt(matrix.Length);
            int skipBorder=(int)Math.Floor(matrixSize/2);


            double[][] output = new double[width][];

            for (int i = 0; i < width; i++)
            {
                output[i] = new double[height];
                for (int j = 0; j < height; j++)
                {
                    if (i < skipBorder || j < skipBorder ||
                            i >= width - skipBorder || j >= height - skipBorder)
                        output[i][j] = 0;
                    else
                    {
                        int sX = i - skipBorder;
                        int sY = j - skipBorder;
                        double value = 0;
                        for (int shiftX = 0; shiftX < matrixSize; shiftX++)
                            for (int shiftY = 0; shiftY < matrixSize; shiftY++)
                                value += matrix[shiftX, shiftY] * inputLayer[sX + shiftX][sY + shiftY];
                        output[i][j] = value;
                    }
                }
            }
            return ReLU(output);
        }

        private static double[][] ReLU(double[][] inputLayer)
        {
            int width = inputLayer.Length;
            int height = inputLayer[0].Length;
            double[][] output = new double[width][];
            for (int i = 0; i < width; i++)
            {
                output[i] = new double[height];
                for (int j = 0; j < height; j++)
                    output[i][j] = Math.Max(0, inputLayer[i][j]);
            }
            return output;
        }

        public static double[][] SubDiscretisation(double[][] inputLayer, int frameSize)
        {
            int inpWidth=inputLayer.Length;
            int inpHeight=inputLayer[0].Length;
            int width = (int)Math.Ceiling((double)inpWidth / frameSize);
            int height = (int)Math.Ceiling((double)inpHeight / frameSize);
            double[][] output = new double[width][];
            for (int i = 0; i < width; i++)
            {
                output[i] = new double[height];
                for (int j = 0; j < height; j++)
                {
                    int sX = frameSize * i;
                    int sY = frameSize * j;
                    double max = inputLayer[sX][sY];
                    for (int shiftX = 0; shiftX < frameSize && sX + shiftX < inpWidth; shiftX++)
                        for (int shiftY = 0; shiftY < frameSize && sY + shiftY < inpHeight; shiftY++)
                        {
                            double temp = inputLayer[sX + shiftX][sY + shiftY];
                            if (temp > max)
                                max = temp;
                        }
                    output[i][j] = max;
                }
            }
            return output;
        }

        public double[][] RunMatrix(double[][] inputLayer, double[,] matrix)
        {
            Layer = Convolution(inputLayer, matrix);
            GC.Collect();
            return Layer;
        }

        public double[][] RunMatrix(double[][] inputLayer, double[,] matrix, int frameSize)
        {
            Layer = SubDiscretisation(Convolution(inputLayer, matrix), frameSize);
            GC.Collect();
            return Layer;
        }
    }
}
