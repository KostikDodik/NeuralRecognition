using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace FirstImageTry
{
    class BitmapNormalizer
    {
        private static Bitmap Resize(Bitmap input)
        {
            Bitmap output;
            if (input.Height <= 64 && input.Width <= 64)
                output = input;
            else
            {
                double k = (input.Height > input.Width) ? input.Height / 64 : input.Width / 64;
                int h = (int)(input.Height / k);
                int w = (int)(input.Width / k);
                output = new Bitmap(input, w, h);
            }
            return output;
        }

        private static double Uncolor(Color c)
        {
            return (double)(c.R+c.G+c.B)/3;
        }


        public static double[][] GetMatrix(Bitmap input)
        {
            input = Resize(input);
            int height = input.Height;
            int width = input.Width;
            double[][] output = new double[width][];
            for (int i = 0; i < width; i++)
            {
                output[i] = new double[height];
                for (int j = 0; j < height; j++)
                    output[i][j] = Uncolor(input.GetPixel(i, j));
            }
            return output;
        }
    }
}
