using System;
using System.Collections;
using UnityEngine;

public static class LerpUtils
{
    private static AnimationCurve _EASE_CURVE = AnimationCurve.EaseInOut(0, 0, 1, 1);
    /// <summary>
    /// Simple ease in/out curve
    /// </summary>
    /// <value></value>
    public static AnimationCurve EASE_CURVE 
    {
        get { return _EASE_CURVE; }
    }

    /// <summary>
    /// Lerps a color from starting to end value over a given number of seconds 
    /// </summary>
    /// <param name="start">Start value</param>
    /// <param name="end">End value</param>
    /// <param name="seconds">Number of seconds to lerp over</param>
    /// <param name="curve">Curve to use for time evaluation</param>
    /// <param name="resultCallback">Function to execute each time the value changes</param>
    /// <returns></returns>
    public static IEnumerator ColorEaseLerp(Color start, Color end, float seconds, AnimationCurve curve, Action<Color> resultCallback)
    {
        float t = 0.0f;
        Color val = start;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            val = Color.Lerp(start, end, curve.Evaluate(t));
            resultCallback(val);
            yield return null;
        }
        val = end;
        resultCallback(val);
        yield return null;
    }

    /// <summary>
    /// Lerps a float from starting to end value over a given number of seconds 
    /// </summary>
    /// <param name="start">Start value</param>
    /// <param name="end">End value</param>
    /// <param name="seconds">Number of seconds to lerp over</param>
    /// <param name="resultCallback">Function to execute each time the value changes</param>
    public static IEnumerator FloatEaseLerp(float start, float end, float seconds, Action<float> resultCallback)
    {
        float t = 0.0f;
        float val = start;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            val = Mathf.Lerp(start, end, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t)));
            resultCallback(val);
            yield return null;
        }
        val = end;
        resultCallback(val);
        yield return null;
    }

    public static IEnumerator FloatLinearLerp(float start, float end, float seconds, Action<float> resultCallback)
    {
        float t = 0.0f;
        float val = start;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            val = Mathf.Lerp(start, end, t);
            resultCallback(val);
            yield return null;
        }
        val = end;
        resultCallback(val);
        yield return null;
    }

   
    public static IEnumerator AnimationCurveEaseLerp(AnimationCurve xCurve, AnimationCurve yCurve, float seconds, Action<Vector2> resultCallback)
    {
        float t = 0.0f;
        Vector2 val = Vector2.zero;
        while (t <= 1.0)
        {
            t += (Time.deltaTime / seconds);
            val = new Vector2(xCurve.Evaluate(t), yCurve.Evaluate(t));
            resultCallback(val);
            yield return null;
        }
        resultCallback(val);
        yield return null;
    }


        public static IEnumerator QuaternionEaseLerp(Quaternion start, Quaternion end, float seconds, Action<Quaternion> resultCallback)
    {

        
        float t = 0.0f;
        Quaternion val = start;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            val = Quaternion.Slerp(start, end, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t)));
            resultCallback(val);
            yield return null;
        }
        val = end;
        resultCallback(val);
        yield return null;
    }
}

