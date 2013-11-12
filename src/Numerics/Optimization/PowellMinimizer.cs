﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Options for Powell Minimization.
    /// </summary>
    public class PowellOptions
    {
        public int? MaximumIterations = null;
        public int? MaximumFunctionCalls = null;
        public double PointTolerance = 1e-4;
        public double FunctionTolerance = 1e-4;
    }

    public enum PowellConvergenceType { Success, MaxIterationsExceeded, MaxFunctionCallsExceeded };

    /// <summary>
    /// Result of Powell Minimization.
    /// </summary>
    public class PowellResult
    {
        public int NumberOfIterations;
        public int NumberOfFunctionCalls;
        public double[] MinimumPoint;
        public double MinimumFunctionValue;
        public PowellConvergenceType ConvergenceType; 
    }
    
    /// <summary>
    /// Minimizes f(p) where p is a vector of model parameters using the Powell method.
    /// </summary>
    public class PowellMinimizer
    {
        public PowellResult Result { get; private set; }

        public readonly PowellOptions Options = new PowellOptions();

        public double[] CurveFit(double[] x, double[] y, Func<double, double[], double> f,
            double[] pStart)
        {
            // Need to minimize sum of squares of residuals; create this function:
            Func<double[], double> function = (p) =>
            {
                double sum = 0;
                for (int i = 0; i < x.Length; ++i)
                {
                    double temp = y[i] - f(x[i], p);
                    sum += temp * temp;
                }
                return sum;
            };
            return Minimize(function, pStart);
        }

        public double[] Minimize(Func<double[], double> function, double[] p)
        {
            BrentMinimizer brentMinimizer = new BrentMinimizer();
            // used in closure:
            double[] point = new double[p.Length];
            double[] startingPoint = new double[p.Length];
            double[] direction = new double[p.Length];
            double lineMiniumum = 0;
            int functionCalls = 0;
            Func<double, double> functionAlongLine = (u) =>
            {
                for (int i = 0; i < point.Length; ++i)
                    point[i] = startingPoint[i] + direction[i] * u;
                lineMiniumum = function(point);
                functionCalls++;
                return lineMiniumum;
            };

            int n = p.Length; // number of dimensions 
            double fval;

            int iterations = 0;
            int maxIterations = (Options.MaximumIterations == null) ? n * 1000 : (int)Options.MaximumIterations;
            int maxFunctionCalls = (Options.MaximumFunctionCalls == null) ? n * 1000 : (int)Options.MaximumFunctionCalls;

            // An array of n directions:
            double[][] direc = new double[n][];
            for (int i = 0; i < n; ++i)
            {
                direc[i] = new double[n];
                direc[i][i] = 1.0;
            }
            double[] x = p;
            double[] x1 = (double[])x.Clone(); 
            
            brentMinimizer.Options.FunctionTolerance = Options.PointTolerance * 100;

            fval = function(x);

            double[] x2 = new double[n];
            double fx;
            double[] direc1 = new double[n];
            double[] xnew;
            while (true)
            {
                fx = fval;
                int bigind = 0;
                double delta = 0.0;
                double fx2;
                for (int i = 0; i < n; ++i)
                {
                    direc1 = direc[i];
                    fx2 = fval;

                    // Do a linesearch with specified starting point and direction.
                    direction = direc1;
                    startingPoint = x;
                    double u = brentMinimizer.Minimize(functionAlongLine);
                    fval = functionAlongLine(u);
                    xnew = point; 

                    for (int j = 0; j < n; ++j) x[j] = xnew[j];

                    if ((fx2 - fval) > delta)
                    {
                        delta = fx2 - fval;
                        bigind = i;
                    }
                }
                iterations++;
                if (2.0 * (fx - fval) <= Options.FunctionTolerance * ((Math.Abs(fx) + Math.Abs(fval)) + 1e-20)) break;
                if (functionCalls >= maxFunctionCalls) break;
                if (iterations >= maxIterations) break;

                // Construct the extrapolated point  
                direc1 = new double[n];
                for (int i = 0; i < n; ++i)
                {
                    direc1[i] = x[i] - x1[i];
                    x2[i] = 2.0 * x[i] - x1[i];
                    x1[i] = x[i];
                }

                fx2 = function(x2);
                if (fx > fx2)
                {
                    double t = 2.0 * (fx + fx2 - 2.0 * fval);
                    double temp = (fx - fval - delta);
                    t *= temp * temp;
                    temp = fx - fx2;
                    t -= delta * temp * temp;
                    if (t < 0.0)
                    {
                        direction = direc1;
                        startingPoint = x;
                        double u = brentMinimizer.Minimize(functionAlongLine);
                        fval = functionAlongLine(u);
                        xnew = point;

                        direc1 = new double[n];
                        for (int i = 0; i < n; ++i)
                        {
                            direc1[i] = xnew[i] - x[i];
                            x[i] = xnew[i];
                        }

                        direc[bigind] = direc[n - 1];
                        direc[n - 1] = direc1;
                    }
                }
            }
            var convergenceType = PowellConvergenceType.Success;
            if (functionCalls >= maxFunctionCalls) 
                convergenceType = PowellConvergenceType.MaxFunctionCallsExceeded;
            else if (iterations > maxIterations) 
                convergenceType = PowellConvergenceType.MaxFunctionCallsExceeded;

            Result = new PowellResult()
            {
                MinimumPoint = (double[])x.Clone(),
                MinimumFunctionValue = fx,
                ConvergenceType = convergenceType,
                NumberOfIterations = iterations,
                NumberOfFunctionCalls = functionCalls
            };
            
            return Result.MinimumPoint;
        }
    }
}