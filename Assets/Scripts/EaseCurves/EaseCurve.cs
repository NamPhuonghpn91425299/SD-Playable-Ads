using System;
using UnityEngine;

namespace FeaturesBase.EaseCurve
{
    public struct EaseCurve
    {
        #region <====================| Helpers |====================>
    
#if UNITY_EDITOR
        public static void GizmosDrawCurve(Vector3 start, Vector3 end, Vector3 direction, Func<float, float> easeCurve,
            bool isInverseTime = false, bool isInverseValue = false, float scaleTime = 0.01f)
        {
            var previousPoint = start + direction * GetValue(0);
            for (float t = 0; t <= 1; t += scaleTime)
            {
                Vector3 point = Vector3.Lerp(start, end, t) + direction * GetValue(t);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            float GetValue(float time)
            {
                var timeExecute = isInverseTime ? 1 - time : time;
                return isInverseValue ? 1 - easeCurve.Invoke(timeExecute) : easeCurve.Invoke(timeExecute);
            }
        }

        public static void GizmosDrawCurve(Vector3 start, Vector3 end, Vector3 direction, AnimationCurve easeCurve,
            bool isInverseTime = false, bool isInverseValue = false, float scaleTime = 0.01f)
        {
            var previousPoint = start + direction * GetValue(0);
            for (float t = 0; t <= 1; t += scaleTime)
            {
                Vector3 point = Vector3.Lerp(start, end, t) + direction * GetValue(t);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            float GetValue(float time)
            {
                var timeExecute = isInverseTime ? 1 - time : time;
                return isInverseValue ? 1 - easeCurve.Evaluate(timeExecute) : easeCurve.Evaluate(timeExecute);
            }
        }
#endif
        public static Vector3 GetCurvePoint(Vector3 start, Vector3 end, float timeNomalize, Vector3 avgValue, Func<float, float> easeCurve,
            bool isInverseTime = false, bool isInverseValue = false)
        {
            var point = Vector3.Lerp(start, end, timeNomalize);
            point = new Vector3(point.x + avgValue.x * GetValue(timeNomalize), point.y + avgValue.y * GetValue(timeNomalize), point.z + avgValue.z * GetValue(timeNomalize));
            return point;

            float GetValue(float time)
            {
                var timeExecute = isInverseTime ? 1 - time : time;
                return isInverseValue ? 1 - easeCurve.Invoke(timeExecute) : easeCurve.Invoke(timeExecute);
            }
        }

        public static Vector3 GetCurvePoint(Vector3 start, Vector3 end, float timeNomalize, Vector3 avgValue, AnimationCurve curve,
            bool isInverseTime = false, bool isInverseValue = false)
        {
            var point = Vector3.Lerp(start, end, timeNomalize);
            point = new Vector3(point.x + avgValue.x * GetValue(timeNomalize), point.y + avgValue.y * GetValue(timeNomalize), point.z + avgValue.z * GetValue(timeNomalize));
            return point;

            float GetValue(float time)
            {
                var timeExecute = isInverseTime ? 1 - time : time;
                return isInverseValue ? 1 - curve.Evaluate(timeExecute) : curve.Evaluate(timeExecute);
            }
        }

        #endregion <=============================================>

        #region <====================| FuncionCurves |====================>

        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInSine">EaseInSine</a>
        /// </summary>
        public static float EaseInSine(float t)
        {
            return 1 - Mathf.Cos(t * Mathf.PI / 2);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutSine">EaseOutSine</a>
        /// </summary>
        public static float EaseOutSine(float t)
        {
            return Mathf.Sin(t * Mathf.PI / 2);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutSine">EaseInOutSine</a>
        /// </summary>
        public static float EaseInOutSine(float t)
        {
            return 0.5f * (1 - Mathf.Cos(t * Mathf.PI));
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInQuad">EaseInQuad</a>
        /// </summary>
        public static float EaseInQuad(float t)
        {
            return t * t;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutQuad">EaseOutQuad</a>
        /// </summary>
        public static float EaseOutQuad(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutQuad">EaseInOutQuad</a>
        /// </summary>
        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f
                ? 2 * t * t
                : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInCubic">EaseInCubic</a>
        /// </summary>
        public static float EaseInCubic(float t)
        {
            return Mathf.Pow(t, 3);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutCubic">EaseOutCubic</a>
        /// </summary>
        public static float EaseOutCubic(float t)
        {
            return 1 - Mathf.Pow(1 - t, 3);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutCubic">EaseInOutCubic</a>
        /// </summary>
        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4 * Mathf.Pow(t, 3)
                : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInQuart">EaseInQuart</a>
        /// </summary>
        public static float EaseInQuart(float t)
        {
            return Mathf.Pow(t, 4);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutQuart">EaseOutQuart</a>
        /// </summary>
        public static float EaseOutQuart(float t)
        {
            return 1 - Mathf.Pow(1 - t, 4);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutQuart">EaseInOutQuart</a>
        /// </summary>
        public static float EaseInOutQuart(float t)
        {
            return t < 0.5f
                ? 8 * Mathf.Pow(t, 4)
                : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInQuint">EaseInQuint</a>
        /// </summary>
        public static float EaseInQuint(float t)
        {
            return Mathf.Pow(t, 5);
        }
        
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutQuint">EaseOutQuint</a>
        /// </summary>
        public static float EaseOutQuint(float t)
        {
            return 1 - Mathf.Pow(1 - t, 5);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutQuint">EaseInOutQuint</a>
        /// </summary>
        public static float EaseInOutQuint(float t)
        {
            return t < 0.5f
                ? 16 * Mathf.Pow(t, 5)
                : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInExpo">EaseInExpo</a> 
        /// </summary>
        public static float EaseInExpo(float t)
        {
            return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutExpo">EaseOutExpo</a>
        /// </summary>
        public static float EaseOutExpo(float t)
        {
            return 1 - EaseInExpo(1 - t);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutExpo">EaseInOutExpo</a>
        /// </summary>
        public static float EaseInOutExpo(float t)
        {
            return t < 0.5f ? EaseInExpo(t) : EaseOutExpo(t);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInCirc">EaseInCirc</a>
        /// </summary>
        public static float EaseInCirc(float t)
        {
            return 1 - Mathf.Sqrt(1 - t * t);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutCirc">EaseOutCirc</a>
        /// </summary>
        public static float EaseOutCirc(float t)
        {
            return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutCirc">EaseInOutCirc</a>
        /// </summary>
        public static float EaseInOutCirc(float t)
        {
            return t < 0.5f
                ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t,      2))) / 2
                : (1 + Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2))) / 2;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInBack">EaseInBack</a>
        /// </summary>
        public static float EaseInBack(float t)
        {
            var c1 = 1.70158f;
            var c3 = c1 + 1;
            return t * t * (c3 * t - c1);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutBack">EaseOutBack</a>
        /// </summary>
        public static float EaseOutBack(float t)
        {
            var c1 = 1.70158f;
            var c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutBack">EaseInOutBack</a>
        /// </summary>
        public static float EaseInOutBack(float t)
        {
            var c1 = 1.70158f;
            var c2 = c1 * 1.525f;
            return t < 0.5f
                ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInElastic">EaseInElastic</a>
        /// </summary>
        public static float EaseInElastic(float t)
        {
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * (2 * Mathf.PI) / 3) + 1;
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutElastic">EaseOutElastic</a>
        /// </summary>
        public static float EaseOutElastic(float t)
        {
            return 1 - (Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * (2 * Mathf.PI) / 3) + 1);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutElastic">EaseInOutElastic</a>
        /// </summary>
        public static float EaseInOutElastic(float t)
        {
            return t < .5f ? EaseInElastic(t) : EaseOutElastic(t);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInBounce">EaseInBounce</a>
        /// </summary>
        public static float EaseInBounce(float t)
        {
            return 1 - EaseOutBounce(1 - t);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeOutBounce">EaseOutBounce</a>
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            var d1 = 2.75f;
            var n1 = 7.5625f;
            
            if (t < (1 / d1))
            {
                return (n1 * t * t);
            }
            
            if (t < (2 / d1))
            {
                t -= (1.5f / d1);
                return (n1 * t * t + 0.75f);
            }
            
            if (t < (2.5f / d1))
            {
                t -= (2.25f / d1);
                return (n1 * t * t + 0.9375f);
            }
            
            t -= (2.625f / d1);
            return (n1 * t * t + 0.984375f);
        }
        
        /// <summary>
        /// ref FuncionCurves: <a href="https://easings.net/#easeInOutBounce">EaseInOutBounce</a>
        /// </summary>
        public static float EaseInOutBounce(float t)
        {
            return t < 0.5f
                ? (1 - EaseOutBounce(1 - 2 * t)) / 2
                : (1 + EaseOutBounce(2 * t - 1)) / 2;
        }
 
        #endregion <=============================================>
        
        #region <====================| CustomFuncionCurve |====================>
    
        // đường cong parabol khép kín
        public static float EaseParabol(float time)
        {
            return 4 * time * (1 - time);
        }
 
        #endregion <=============================================>
    }
}
