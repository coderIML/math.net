﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;

using Complex = System.Numerics.Complex;

namespace Integration
{
    class Program
    {
        static void Main(string[] args)
        {
            // Code touching all providers
            Control.UseManaged();
            Matrix<Complex> matrix = CreateMatrix.Random<Complex>(10, 10, 100);
            Vector<Complex> vector = matrix.Svd().S;
            Fourier.Forward(vector.AsArray());
            Console.WriteLine($"Framework={RuntimeInformation.FrameworkDescription}; OS={RuntimeInformation.OSDescription}");
            Console.WriteLine($"LinearAlgebra={Control.LinearAlgebraProvider}; FFT={Control.FourierTransformProvider}");
            Console.WriteLine($"DC={vector[0].Magnitude}; Low={vector[1].Magnitude}; Hight={vector[5].Magnitude}");

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }
    }
}
