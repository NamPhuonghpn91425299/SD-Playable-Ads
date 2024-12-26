using System;
using UnityEngine;

namespace FeaturesBase.EaseCurve
{
    public static class EaseCurveHelper
    {
        public static AnimationCurve ToAnimationCurve(this Func<float, float> easeCurve, float startTime = 0, float endTime = 1, int sampleCount = 10)
        {
            var   keyframes = new Keyframe[sampleCount];
            float step      = (endTime - startTime) / (sampleCount - 1);

            // get keyframe value
            for (int i = 0; i < sampleCount; i++)
            {
                float time  = startTime + i * step;
                float value = easeCurve(time);
                keyframes[i] = new Keyframe(time, value);
            }

            // setup tangents for keyframe
            for (int i = 1; i < sampleCount - 1; i++)
            {
                float deltaTime  = keyframes[i + 1].time - keyframes[i - 1].time;
                float deltaValue = keyframes[i + 1].value - keyframes[i - 1].value;

                keyframes[i].inTangent  = deltaValue / deltaTime;
                keyframes[i].outTangent = deltaValue / deltaTime;
            }

            // setup tangents for first last keyframe
            keyframes[0].outTangent              = (keyframes[1].value - keyframes[0].value) / (keyframes[1].time - keyframes[0].time);
            keyframes[sampleCount - 1].inTangent = (keyframes[sampleCount - 1].value - keyframes[sampleCount - 2].value) / (keyframes[sampleCount - 1].time - keyframes[sampleCount - 2].time);

            return new AnimationCurve(keyframes);
        }
        
        public static AnimationCurve NormalizeCurve(this AnimationCurve curve)
        {
            if (curve.keys.Length < 2) return curve;

            // Tìm min/max của time và value
            float minTime  = float.MaxValue;
            float maxTime  = float.MinValue;
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            foreach (Keyframe key in curve.keys)
            {
                minTime  = Mathf.Min(minTime, key.time);
                maxTime  = Mathf.Max(maxTime, key.time);
                minValue = Mathf.Min(minValue, key.value);
                maxValue = Mathf.Max(maxValue, key.value);
            }

            float timeRange  = maxTime - minTime;
            float valueRange = maxValue - minValue;

            var normalizedCurve = new AnimationCurve();
    
            foreach (Keyframe key in curve.keys)
            {
                float normalizedTime = timeRange > 0 
                    ? (key.time - minTime) / timeRange 
                    : 0;
            
                float normalizedValue = valueRange > 0 
                    ? (key.value - minValue) / valueRange 
                    : 0;

                float normalizedInTangent = valueRange > 0 && timeRange > 0 
                    ? key.inTangent * (timeRange / valueRange) 
                    : key.inTangent;
            
                float normalizedOutTangent = valueRange > 0 && timeRange > 0 
                    ? key.outTangent * (timeRange / valueRange) 
                    : key.outTangent;

                var newKey = new Keyframe()
                {
                    time         = normalizedTime,
                    value        = normalizedValue,
                    inTangent    = normalizedInTangent,
                    outTangent   = normalizedOutTangent,
                    inWeight     = key.inWeight,
                    outWeight    = key.outWeight,
                    weightedMode = key.weightedMode
                };

                normalizedCurve.AddKey(newKey);
            }

            return normalizedCurve;
        }
    }
}
