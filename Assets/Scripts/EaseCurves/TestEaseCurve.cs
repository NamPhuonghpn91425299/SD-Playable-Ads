using FeaturesBase.EaseCurve;
using System.Net;
using UnityEngine;

public class TestEaseCurve : MonoBehaviour
{
    [SerializeField]
    private Transform _startPoint;

    [SerializeField]
    private Transform _stopPoint;
    
    [SerializeField]
    private Transform _objMovement;

    [SerializeField] [Range(0, 100)]
    private float _powerValue = 1;

    
    [SerializeField]
    private AnimationCurve _targetCurnveNormalize;
    
    private EaseCurveRunner _runner;

    private Vector3 endPoint;
    
    
    void Start()
    {
        endPoint = _stopPoint.position;
        _runner = new EaseCurveRunner(_objMovement)
           .WithNextCurve(EaseCurve.EaseParabol)

           .Setup(_startPoint.position, endPoint, 2);


        _runner.WithCoefficientRate(1);

        //var distance = Vector3.Distance(_startPoint.position, _stopPoint.position);
        //_runner.WithDirection(Vector3.up, distance);
    }

    void Update()
    {
        _runner.WithDirection(Vector3.up);

        if (_runner.IsComplete)
        {
            endPoint = _startPoint.position;

            _runner.Setup(endPoint, 2);
            _runner.WithCoefficientRate(1);
        }
        else
        {
            _runner.Tick(Time.deltaTime);

        }
        
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(transform.position, transform.forward * 30);

        var obs = _runner ?? new EaseCurveRunner(_objMovement)
           .WithNextCurve(EaseCurve.EaseParabol)
           .Setup(_startPoint.position, _stopPoint.position, 4);


        if (obs == null) return;
        if (!Application.isPlaying) obs.WithDirection(Vector3.up).WithCoefficient(_powerValue);
        obs.OnDrawGizmos();
        _runner?.OnDrawGizmos();
    }
}
