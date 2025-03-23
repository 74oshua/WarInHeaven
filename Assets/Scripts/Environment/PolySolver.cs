using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// algorithms for solving polynomials, useful for projectile intercepts
public static class PolySolver
{
    // algebra
    // coefficients start from highest power
    // get derivative of a real polynomial function
    public static List<float> PolyDeriv(List<float> coefficients)
    {
        int degree = coefficients.Count - 1;
        List<float> ret = new List<float>();
        for (int i = 0; i < degree; i++)
        {
            ret.Add(0);
        }

        for (int i = 0; i < degree; i++)
        {
            ret[i] = (degree - i) * coefficients[i];
        }

        return ret;
    }

    // calculate the value of a polynomial given the coefficents and a variable value
    public static float CalcPoly(List<float> coefficients, float value)
    {
        float ret = 0;
        int degree = coefficients.Count - 1;
        for (int i = 0; i < coefficients.Count; i++)
        {
            ret += coefficients[i] * Mathf.Pow(value, degree - i);
        }
        return ret;
    }

    // newton's method
    public static float SolvePoly(List<float> coefficients, int iterations, float initial_value = 0)
    {
        float x = initial_value;
        for (int i = 0; i < iterations; i++)
        {
            x -= CalcPoly(coefficients, x) / CalcPoly(PolyDeriv(coefficients), x);
        }
        return x;
    }

    // trig
    public static float Acosh(float x)
    {
        return Mathf.Log(x + Mathf.Sqrt(Mathf.Pow(x, 2) - 1));
    }
}