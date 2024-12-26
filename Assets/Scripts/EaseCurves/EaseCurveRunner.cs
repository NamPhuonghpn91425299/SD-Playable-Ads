using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FeaturesBase.EaseCurve
{
    public class EaseCurveRunner
    {
        #region <====================| Properties |====================>
        
        private readonly Transform         _sourceMove;
        private readonly List<IEeaseCurve> _easeCurves = new List<IEeaseCurve>();
        private          IEeaseCurve       _runningCurve;
        
        private float   _avgTime;
        private Vector3 _direction;
        private float   _coefficient;
        private float   _timeCount;
        private float   _timeEnd;
        private int     _indexRead;
    
        #endregion <=============================================>
        
        #region <====================| Builder |====================>
    
        public EaseCurveRunner(Transform sourceMove) => _sourceMove = sourceMove;

        public bool  IsComplete  => _indexRead >= _easeCurves.Count;
        public float ExecuteTime;
        
        /// <summary>
        /// Thêm một curve vào danh sách
        /// </summary>
        /// <param name="curve">Hàm đánh giá curve</param>
        /// <param name="normalizeTime">tỉ lệ thời gian hoàn thành curve dựa trên avgTime của Runner</param>
        /// <param name="isInverseTime">lật curve theo chiều ngang</param>
        /// <param name="isInverseValue">lật curve theo chiều dọc</param>
        /// <returns></returns>
        public EaseCurveRunner WithNextCurve(Func<float, float> curve, float normalizeTime = 1, bool isInverseTime = false, bool isInverseValue = false)
        {
            _easeCurves.Add(new EaseCurveFunction(curve, normalizeTime, isInverseTime, isInverseValue));
            return this;
        }
        
        /// <summary>
        /// Thêm một curve vào danh sách
        /// </summary>
        /// <param name="curve">AnimationCurve đã normalize</param>
        /// <param name="normalizeTime">tỉ lệ thời gian hoàn thành curve dựa trên avgTime của Runner</param>
        /// <param name="isInverseTime">lật curve theo chiều ngang</param>
        /// <param name="isInverseValue">lật curve theo chiều dọc</param>
        /// <returns></returns>
        public EaseCurveRunner WithNextCurve(AnimationCurve curve, float normalizeTime = 1, bool isInverseTime = false, bool isInverseValue = false)
        {
            _easeCurves.Add(new EaseCurveAnimation(curve, normalizeTime, isInverseTime, isInverseValue));
            return this;
        }
        
        /// <summary>
        /// Thêm hướng di chuyển Curve
        /// </summary>
        /// <param name="direction">Hướng di chuyển</param>
        /// <returns></returns>
        public EaseCurveRunner WithDirection(Vector3 direction)
        {
            _direction = direction;
            return this;
        }

        /// <summary>
        /// Hệ số chuyển động trên hướng với giá trị cố định
        /// </summary>
        /// <param name="coefficient">Lực tác động</param>
        /// <returns></returns>
        public EaseCurveRunner WithCoefficient(float coefficient)
        {
            _coefficient = coefficient;
            return this;
        }


        /// <summary>
        /// Hệ số chuyển động trên hướng với giá trị tỉ lệ dựa trên khoảng cách
        /// </summary>
        /// <param name="rateByDistance">tỉ lệ với khoảng cách của vật thể</param>
        /// <returns></returns>
        public EaseCurveRunner WithCoefficientRate(float rateByDistance)
        {
            if (_easeCurves.Count == 0) return this;
            var from = _easeCurves[0].Tick(_direction, 0);
            var to = _easeCurves[^1].Tick(_direction, 1);
            _coefficient = Vector3.Distance(from, to) * rateByDistance;
            return this;
        }

        /// <summary>
        /// Cần chạy để đánh giá laị các curve được thêm
        /// </summary>
        /// <param name="from">Điểm đầu</param>
        /// <param name="to">Điểm cuối</param>
        /// <param name="avgTime">thời gian trung bình của các curve</param>
        public EaseCurveRunner Setup(Vector3 from, Vector3 to, float avgTime)
        {
            _avgTime = avgTime;
            Reset();

            var eFrom = from;
            for (var idx = 0; idx < _easeCurves.Count; idx++)
            {
                var eTo = Vector3.Lerp(from, to, (float)(idx + 1) / _easeCurves.Count);
                _easeCurves[idx].AddDestination(eFrom, eTo);
                eFrom = eTo;
            }
            ExecuteTime = _easeCurves.Sum(e => e.TimeNomalize * _avgTime);
            return this;
        }

        /// <summary>
        /// Cần chạy để đánh giá laị các curve được thêm
        /// với điểm đầu là điểm hiện tại
        /// </summary>
        /// <param name="to">Điểm cuối</param>
        /// <param name="avgTime">thời gian trung bình của các curve</param>
        public EaseCurveRunner Setup(Vector3 to, float avgTime)
        {
            return Setup(_sourceMove.position, to, avgTime);
        }

        public void Reset()
        {
            _indexRead = 0;
            EvaluteCurve();
        }

        public void RemoveCurve(AnimationCurve curve)
        {
            var index = _easeCurves.FindIndex(e => e is EaseCurveAnimation ease && ease.Curve.Equals(curve));
            if (index >= 0) _easeCurves.RemoveAt(index);
        }

        public void RemoveCurve(Func<float, float> curve)
        {
            var index = _easeCurves.FindIndex(e => e is EaseCurveFunction ease && ease.Curve.Equals(curve));
            if (index >= 0) _easeCurves.RemoveAt(index);
        }
        
        public void RemoveCurveAt(int index) => _easeCurves.RemoveAt(index);
        public void CleanCurve()             => _easeCurves.Clear();
 
        #endregion <=============================================>
        
        #region <====================| MainHandlers |====================>
        
        /// <summary>
        /// di chuyển curve về phía trước
        /// </summary>
        /// <param name="deltaTime">[1/time] thời gian hoạt động của curve</param>
        public void Tick(float deltaTime)
        {
            if (_indexRead >= _easeCurves.Count) return;
            _timeCount           = Mathf.Min(_timeCount + deltaTime, _timeEnd);
            _sourceMove.position = _runningCurve.Tick(_direction * _coefficient, _timeCount / _timeEnd);
            
            if (_timeCount >= _timeEnd)
            {
                _indexRead += 1;
                EvaluteCurve();
            }
        }


        /// <summary>
        /// di chuyển curve về phía trước
        /// </summary>
        /// <param name="deltaTime">[1/time] thời gian hoạt động của curve</param>
        /// <param name="runParallel">sự kiện chạy đòng thời với Tick(), giá trị truyền vào là timeNomalized</param>
        public void Tick(float deltaTime, Action<float> runParallel)
        {
            Tick(deltaTime);
            runParallel.Invoke(_timeCount / _timeEnd);
        }
    
        /// <summary>
        /// Vẽ các curve ra Gizmos (Không cần đưa hàm này vào #if UNITY_EDITOR)
        /// - scaleTime: càng lớn thì Curve càng rõ nét, và Editor sẽ chậm hơn
        /// - ScaleTime: tương ứng với deltaTime của Tick(float) 
        /// </summary>
        /// <param name="scaleTime">thời gian chuẩn hóa chuyển động (deltaTime)</param>
        public void OnDrawGizmos(float scaleTime = 0.01f)
        {
            foreach (var curve in _easeCurves)
            {
                curve.OnDrawGizmos(_direction * _coefficient, scaleTime);
            }
        }

        /// <summary>
        /// di chuyển curve về phía trước bằng coroutine
        /// </summary>
        /// <param name="host">Mono thực hiện Coroutine</param>
        /// <param name="deltaTime">[1/time] thời gian hoạt động của curve</param>
        public Coroutine RunOnRoutine(MonoBehaviour host, float deltaTime)
        {
            return host.StartCoroutine(Runer());

            IEnumerator Runer()
            {
                while (!IsComplete)
                {
                    Tick(deltaTime);
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// di chuyển curve về phía trước bằng coroutine
        /// </summary>
        /// <param name="host">Mono thực hiện Coroutine</param>
        /// <param name="deltaTime">[1/time] thời gian hoạt động của curve</param>
        /// <param name="runParallel">sự kiện chạy đòng thời với Tick(), giá trị truyền vào là timeNomalized</param>
        public Coroutine RunOnRoutine(MonoBehaviour host, float deltaTime, Action<float> runParallel)
        {
            return host.StartCoroutine(Runer());

            IEnumerator Runer()
            {
                while (!IsComplete)
                {
                    Tick(deltaTime, runParallel);
                    yield return null;
                }
            }
        }
    
        #endregion <=============================================>
        
        #region <====================| Supports |====================>

        private void EvaluteCurve()
        {
            _timeCount    = 0;
            if (_indexRead < _easeCurves.Count)
            {
                _timeEnd      = _avgTime * _easeCurves[_indexRead].TimeNomalize;
                _runningCurve = _easeCurves[_indexRead];
            }
        }
 
        #endregion <=============================================>
    }
}
