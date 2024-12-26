using System.Collections;
using System.Collections.Generic;
using FeaturesBase.EaseCurve;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonoBehaviourTool.PreviewMotion
{
    public class PreviewMotionObject : MonoBehaviour
    {
        #region <====================| Properties |====================>
        
        [SerializeField]
        private List<Transform> _destinations;
    
        [SerializeField]
        private List<PathPreviewer> _pathPerviewers = new List<PathPreviewer>();

        [SerializeField]
        private bool _isLoop;

        private Coroutine _previewRotine;
        private int       _previewIdx    = 0;
        private float     _timeStopCount = 0;
    
        private EaseCurveRunner _easeCurve          => _pathPerviewers?.Count > _previewIdx ? _pathPerviewers[_previewIdx].EaseCurve : null;
        private float           _rollTimePerSeconds => _pathPerviewers?.Count > _previewIdx ? _pathPerviewers[_previewIdx].RollTimePerSeconds : 0;
        private float           _stopTimeSeconds    => _pathPerviewers?.Count > _previewIdx ? _pathPerviewers[_previewIdx].DelayTime : 0;
    
        #endregion <=============================================>

        #region <====================| UnityCores |====================>

        private void OnValidate()
        {
            if (Application.isPlaying || !this.isActiveAndEnabled || _pathPerviewers.Count == 0) return;
            OnEnable();
            Awake();
            _previewRotine = StartCoroutine(PreviewRoutine());
        }
    
        private void OnDrawGizmos() => _pathPerviewers.ForEach(e => e.EaseCurve?.OnDrawGizmos());

        private void Awake()
        {
            if (_previewRotine != null) StopCoroutine(_previewRotine);
            transform.rotation = Quaternion.identity;
        }
    
        private void OnEnable()
        {
            _previewIdx = 0;
            _pathPerviewers.ForEach(e => e?.Setup(transform));
        }

        private void Update()
        {
            if (_easeCurve == null) return;
            _easeCurve.Tick(Time.deltaTime, OnRotate);

            if (_easeCurve.IsComplete)
            {
                _timeStopCount -= Time.deltaTime;
                if (_timeStopCount <= 0 && ++_previewIdx >= _pathPerviewers.Count && _isLoop)
                {
                    ResetCurves();
                    _timeStopCount = _stopTimeSeconds;
                }
            }
        }

        #endregion <=============================================>
        
        #region <====================| Supports |====================>
    
        private IEnumerator PreviewRoutine()
        {
            if (_easeCurve == null) yield break;
            while (!_easeCurve.IsComplete)
            {
                _easeCurve.Tick(Time.deltaTime, OnRotate);
                yield return null;
            }
            
            yield return HelperCoroutine.WaitSeconds(_stopTimeSeconds);
            if (++_previewIdx >= _pathPerviewers.Count)
            {
                if (!_isLoop) yield break;
                ResetCurves();
            }
            yield return PreviewRoutine();
        }

        private void ResetCurves()
        {
            _previewIdx = 0;
            _pathPerviewers.ForEach(e => e.EaseCurve.Reset());
        }

        private void OnRotate(float deltatime)
        {
            if (_rollTimePerSeconds <= 0) return;
            transform.Rotate(transform.forward, 360f * _rollTimePerSeconds * Time.deltaTime, Space.Self);
        }

        public void UpdateByListTransform()
        {
            if (_destinations.Count < 2) return;
            for (var idx = 1; idx < _destinations.Count; idx++)
            {
                if (_pathPerviewers.Count >= idx)
                {
                    _pathPerviewers[idx - 1].WithRange(_destinations[idx - 1], _destinations[idx]);
                }
                else
                {
                    var pathPreviewer = new PathPreviewer();
                    pathPreviewer.WithRange(_destinations[idx - 1], _destinations[idx]);
                    _pathPerviewers.Add(pathPreviewer);
                }
            }
            
            if (_destinations.Count < 3) return;
            if (_pathPerviewers.Count >= _destinations.Count)
            {
                _pathPerviewers[_destinations.Count - 1].WithRange(_destinations[^1], _destinations[0]);
            }
            else
            {
                var pathPreviewer = new PathPreviewer();
                pathPreviewer.WithRange(_destinations[^1], _destinations[0]);
                _pathPerviewers.Add(pathPreviewer);
            }
            
        }
    
        #endregion <=============================================>
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(PreviewMotionObject))]
    public class PreviewMotionObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var previewMotionObject = target as PreviewMotionObject;
            if (previewMotionObject == null) return;
            
            if (GUILayout.Button("Apply List Transforms"))
            {
                previewMotionObject.UpdateByListTransform();
            }
        }
    }
#endif
}