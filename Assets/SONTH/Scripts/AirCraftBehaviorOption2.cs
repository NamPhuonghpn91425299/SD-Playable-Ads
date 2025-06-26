using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AirCraftBehaviorOption2 : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _rotateBackDuration = 10f;
    [SerializeField] private float _angle = 20f;
    [SerializeField] private float _threshold = 18;
    //[SerializeField] private float _percentPathRotateBack = 10f;
    [SerializeField] private Transform[] _lstAttachPoint;
    private Quaternion _defaultRotation;
    //private float _pathlenght;
    private float dirX, dirY, delaX;
    private int _index;
    private bool _isLastPoint;
    private bool _canMove;
    private HelperTransform _helperTransform;
    // Start is called before the first frame update
    void Start()
    {
        _defaultRotation = transform.rotation;
        _index = 0;
        SetupDetail(transform.position, _lstAttachPoint[_index].position);
        _isLastPoint = false;
        _canMove = true;
        _helperTransform = gameObject.AddComponent<HelperTransform>();
        _helperTransform.actionManager = gameObject.AddComponent<ActionsManager>();
        //HelperTransform.MovePathBySpeed(transform, _lstAttachPoint, 2, () => { });
    }

    // Update is called once per frame
    void Update()
    {
        if (_canMove)
        {
            MoveToTargetPoint(_lstAttachPoint[_index].position);
        }
    }
    void SetupDetail(Vector3 currentPos, Vector3 targetPos)
    {
        Vector3 v1 = targetPos - currentPos;
        Vector3 v2 = new Vector3(targetPos.x, currentPos.y, targetPos.z) - currentPos;
        _angle = Vector3.Angle(v1, v2);
        dirX = v1.x > 0 ? 1 : -1;
        dirY = v1.y < 0 ? 1 : -1;
        //_pathlenght = Vector3.Distance(currentPos, targetPos);
    }
    void MoveToTargetPoint(Vector3 targetPoint)
    {
        float distance = Vector3.Distance(transform.position, targetPoint);
        //float pathTraveledPercent = (distance / _pathlenght) * 100;
        //if (pathTraveledPercent <= _percentPathRotateBack)
        //{
        //}
        //else
        //{
        //}
        if (distance >= .1f)
        {
            float angle = _angle > 0 ? _threshold / 2 : 0;
            float val = 1;
            delaX = _lstAttachPoint[_index].position.x - transform.position.x;
            if (delaX <= .01) val = 0;
            if (dirY < 0)
            {
                if (dirX < 0)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(angle * dirY, _defaultRotation.eulerAngles.y + angle * dirX * dirY, angle * -1 ), _rotationSpeed * Time.deltaTime);
                    
                }
                else
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(angle * dirY, _defaultRotation.eulerAngles.y + angle * dirX * dirY * val, (angle * dirX) * val), _rotationSpeed * Time.deltaTime);
                    
                }
            }
            else
            {
                if (dirX < 0)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(angle * dirY, _defaultRotation.eulerAngles.y + angle, angle * dirY * -1), _rotationSpeed * Time.deltaTime);
                    
                }
                else
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(angle, _defaultRotation.eulerAngles.y + angle * -1 * val, (angle) * val), _rotationSpeed * Time.deltaTime);
                    
                }
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, 1 - (1- _moveSpeed * Time.deltaTime) * (1- _moveSpeed * Time.deltaTime));
        }
        else
        {
            _index++;
            //_index = Mathf.Clamp(_index, 0, _lstAttachPoint.Length - 1);
            if (_index == _lstAttachPoint.Length)
            {
                _index = 1;
            }
            if (!_isLastPoint)
            {
            }
            if (_index == _lstAttachPoint.Length - 1)
            {
                _isLastPoint = true;
            }
            SetupDetail(transform.position, _lstAttachPoint[_index].position);
            StartCoroutine(WaitToMove());
        }
    }
    void Swing(float targetAngle)
    {
        _helperTransform.MoveBySpeed(transform, new Vector3(transform.position.x, transform.position.y + .1f, transform.position.z), .05f, () =>
        {
            _helperTransform.MoveBySpeed(transform, new Vector3(transform.position.x, transform.position.y - .1f, transform.position.z), .05f, () =>
            {

            });
        });
        _helperTransform.RotateByDuration(transform, _defaultRotation, _rotateBackDuration, () =>
        {
            _helperTransform.RotateBySpeed(transform, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetAngle), 5, () =>
            {
                _helperTransform.RotateBySpeed(transform, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -targetAngle), 5, () =>
                {
                    _helperTransform.RotateBySpeed(transform, _defaultRotation, 5, () =>
                    {
                        _canMove = true;
                    });
                });
            });
        });
    }
    IEnumerator WaitToMove()
    {
        _canMove = false;
        Swing(5);
        while (!_canMove)
        {
            yield return null;
        }
        _canMove = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for(int i = 0; i < _lstAttachPoint.Length - 1; i++)
        {
            Gizmos.DrawLine(_lstAttachPoint[i].position, _lstAttachPoint[i + 1].position);
        }
    }

}
