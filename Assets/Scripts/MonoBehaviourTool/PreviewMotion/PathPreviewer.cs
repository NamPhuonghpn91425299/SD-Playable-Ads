using System.Collections.Generic;
using FeaturesBase.EaseCurve;
using UnityEngine;

namespace MonoBehaviourTool.PreviewMotion
{
    [System.Serializable]
    public class PathPreviewer
    {
        [SerializeField]
        private Transform _startPreview;

        [SerializeField]
        private Transform _stopPreview;

        [SerializeField]
        private List<AnimationCurve> _motionCurves = new List<AnimationCurve>();

        [Range(1, 10)]
        [SerializeField]
        private float _secondRuntime = 1;
    
        [Range(-50, 50)]
        [SerializeField]
        private float _coefficientValue = 1;

        [Range(0, 5)]
        [SerializeField]
        private float _rollTimePerSeconds = 0;

        [Range(0, 5)]
        public float DelayTime;

        [SerializeField]
        private Vector3 _direction = Vector3.up;
    
        private EaseCurveRunner _easeCurve;
        public  EaseCurveRunner EaseCurve          => _easeCurve;
        public  float           RollTimePerSeconds => _rollTimePerSeconds;

        public void Setup(Transform target)
        {
            _easeCurve = new EaseCurveRunner(target);
            _easeCurve.WithDirection(_direction.normalized);
        
            foreach (var curve in _motionCurves)
                _easeCurve.WithNextCurve(curve);
        
            _easeCurve.Setup(_startPreview.position, _stopPreview.position, _secondRuntime);
            _easeCurve.WithCoefficient(_coefficientValue);
        }
        
        public void WithRange(Transform start, Transform stop)
        {
            _startPreview = start;
            _stopPreview  = stop;
        }
    }
}
