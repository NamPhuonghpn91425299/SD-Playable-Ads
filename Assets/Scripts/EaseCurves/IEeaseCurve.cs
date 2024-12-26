using System;
using UnityEngine;

namespace FeaturesBase.EaseCurve
{
    public interface IEeaseCurve
    {
        float TimeNomalize { get; }

        void    AddDestination(Vector3 from, Vector3 to);
        Vector3 Tick(Vector3 direction, float timeNomalizeCurve);

        void OnDrawGizmos(Vector3 direction, float scaleTime);
    }
    
    
    public class EaseCurveFunction : IEeaseCurve
    {
        public readonly  Func<float, float> Curve;
        private readonly bool               _isInverseTime;
        private readonly bool               _isInverseValue;
        private          Vector3            _from;
        private          Vector3            _to;

        public EaseCurveFunction(Func<float, float> curve, float timeNomalize = 1, bool isInverseTime = false, bool isInverseValue = false)
        {
            Curve          = curve;
            TimeNomalize    = timeNomalize;
            _isInverseTime  = isInverseTime;
            _isInverseValue = isInverseValue;
        }
        
        #region <====================| IEeaseCurve |====================>
    
        public float   TimeNomalize { get; }

        public void AddDestination(Vector3 from, Vector3 to) => (_from, _to) = (from, to);

        public Vector3 Tick(Vector3 direction, float timeNomalizeCurve)
        {
            return EaseCurve.GetCurvePoint(_from, _to, timeNomalizeCurve, direction, Curve, _isInverseTime, _isInverseValue);
        }

        public void OnDrawGizmos(Vector3 direction, float scaleTime)
        {
#if UNITY_EDITOR
            EaseCurve.GizmosDrawCurve(_from, _to, direction, Curve, scaleTime: scaleTime,
                isInverseTime: _isInverseTime, isInverseValue: _isInverseValue);
#endif
        }
 
        #endregion <=============================================>
    }

    public class EaseCurveAnimation : IEeaseCurve
    {
        public readonly  AnimationCurve Curve;
        private readonly bool           _isInverseTime;
        private readonly bool           _isInverseValue;
        private          Vector3        _from;
        private          Vector3        _to;

        public EaseCurveAnimation(AnimationCurve curve, float timeNomalize = 1, bool isInverseTime = false, bool isInverseValue = false)
        {
            Curve           = curve;
            TimeNomalize    = timeNomalize;
            _isInverseTime  = isInverseTime;
            _isInverseValue = isInverseValue;
        }

        #region <====================| IEeaseCurve |====================>

        public float   TimeNomalize { get; }

        public void AddDestination(Vector3 from, Vector3 to) => (_from, _to) = (from, to);

        public Vector3 Tick(Vector3 direction, float timeNomalizeCurve)
        {
            return EaseCurve.GetCurvePoint(_from, _to, timeNomalizeCurve, direction, Curve, _isInverseTime, _isInverseValue);
        }

        public void OnDrawGizmos(Vector3 direction, float scaleTime)
        {
#if UNITY_EDITOR
            EaseCurve.GizmosDrawCurve(_from, _to, direction, Curve, scaleTime: scaleTime,
                isInverseTime: _isInverseTime, isInverseValue: _isInverseValue);
#endif
        }

        #endregion <=============================================>
    }
}
