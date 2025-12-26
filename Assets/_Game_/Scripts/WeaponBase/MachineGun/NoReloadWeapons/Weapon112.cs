using System.Collections;
using UnityEngine;
public class Weapon112 : NoReloadWeapons
{
    [SerializeField] GameObject vfxElectricball; // Hiệu ứng caauf điện
    
    [Header("Pincer Animation")]
    [SerializeField] Transform _body;
    [SerializeField] RotateFactor[] _pincerChilds;
    /// <summary>0: Left, 1: Right, 2: Center</summary>
    [SerializeField] Transform[] _pincerRoots;
    
    [SerializeField] float _animPincersOpenSpeed = 30;  // tốc độ mở càng - 30
    [SerializeField] float _animPincersCloseSpeed = 50; // tốc độ đóng càng - 50
    [SerializeField] float _desiredAngle = 30; // - 30
    [SerializeField] float _animBodyShoodSpeed = 2;// - 2
    [SerializeField] float _animBodyHouldSpeed = .1f;// -.1
    [SerializeField] float _multiplierSpdOnRealease = 1; // -10
    [SerializeField] float maxZHold = 0;  // Max Z value when holding the weapon -0
    [SerializeField] float maxZShoot = .15f; // Max Z value when shooting -.15

    [Header("Electric Ball")]
    [SerializeField] Transform electricBall;  // Transform của quả cầu điện
    [SerializeField] float electricBallMaxScale = 1f;  // Scale max của quả cầu
    Vector3 _bodyOrigin;
    bool canShoot = false;
    int _precentBallScale = 0; // Tỉ lệ % scale của quả cầu điện
    
    [SerializeField] CrossHairRotate _crossHair;
    public override void OnInit()
    {
        base.OnInit();
        _bodyOrigin = _body.localPosition;
    
        if (electricBall != null)
            electricBall.localScale = Vector3.zero; // ẩn quả cầu lúc bắt đầu
    }

    #region Play - StopGun
    private bool canPlayEventGun = false;
    private bool canHold = true;
    private Coroutine _coroutineGiatSung;
    protected override void LogicPlayGun()
    {
        base.LogicPlayGun();
        if(!canHold)
            return;
        if(_coroutineGiatSung != null)
            StopCoroutine(_coroutineGiatSung);
        UpdateElectricBallScale();
        OnOpenPincers();
        _crossHair.ScaleUp();
        _precentBallScale = (int)(GetOverallOpenRatio() * 100f);
        vfxElectricball.SetActive(true);
        canPlayEventGun = true;
        if (_precentBallScale > 18)
        {
            // _audioSource.clip = weaponInfo.AudioStartBarrel;
            // _audioSource.Play();
            canShoot = true;
        }
        else
        {
            _audioSource.clip = weaponInfo.audioClip;
            _audioSource.Play();
        }
    }

    protected override void LogicStopGun()
    {
        base.LogicStopGun();
        vfxElectricball.SetActive(false);
        UpdateElectricBallScale();
        OnRealeasePincers();
        if (canShoot)
        {
            Shoot();
            _audioSource.Stop();
            _audioSource.clip = weaponInfo.AudioEndBarrel;
            _audioSource.PlayOneShot(weaponInfo.AudioEndBarrel);
            canShoot = false;
            canPlayEventGun = false;
        }

        if (canPlayEventGun)
        {
            if(_coroutineGiatSung!=null)
                StopCoroutine(_coroutineGiatSung);
            // Recoil logic: move backward to maxZShoot, then forward to maxZHold
            _coroutineGiatSung = StartCoroutine(RecoilAnimation_DontShoot());
            _crossHair.ScaleDown();
            canPlayEventGun = false;
        }
    }

    #endregion
    protected override void Shoot()
    {
       // UICrosshairItem.Instance.Narrow_Crosshair();
        if (this == null || _cameraTransform == null) return;
        Vector3 forward = _cameraTransform.forward;
        
        forward += new Vector3(
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount)
        );
        FireFromMuzzle(_muzzleTrans_1, forward);
    }
    
    protected override void FireFromMuzzle(Transform muzzle, Vector3 forward)
    {
        print("fire");
        var shotRotation = Quaternion.Euler(Random.insideUnitCircle * weaponInfo.inaccuracy) * forward;
        var ray = new Ray(_cameraTransform.position, shotRotation);
        // Weapon112_ProjectileElectricBall weapon112ProjectileElectricBall = SimplePool.Spawn<Weapon112_ProjectileElectricBall>(_bulletType, muzzle.position, muzzle.rotation);
        // Vector3 posGizmod = GizmodCaculatorPointShoot();
        // weapon112ProjectileElectricBall.OnInit((posGizmod - muzzle.position).normalized,_precentBallScale);
        //
        // if(_coroutineGiatSung!=null)
        //     StopCoroutine(_coroutineGiatSung);
        // // Recoil logic: move backward to maxZShoot, then forward to maxZHold
        // _coroutineGiatSung = StartCoroutine(RecoilAnimation());
        // _crossHair.ScaleDown();
        // rocketController.SnakeCameraRocket();
    }

    IEnumerator RecoilAnimation()
    {
        canHold = false;
        // Move backward to maxZShoot
        while (_body.localPosition.z > maxZShoot)
        {
            _body.localPosition = new Vector3(_bodyOrigin.x, _bodyOrigin.y, Mathf.MoveTowards(_body.localPosition.z, maxZShoot, _animBodyShoodSpeed * Time.deltaTime));
            yield return null;
        }

        // Move forward to _bodyOrigin.z
        while (_body.localPosition.z < _bodyOrigin.z)
        {
            _body.localPosition = new Vector3(_bodyOrigin.x, _bodyOrigin.y, Mathf.MoveTowards(_body.localPosition.z, _bodyOrigin.z, _animBodyShoodSpeed * Time.deltaTime));
            yield return null;
        }
        canHold = true;
    }
    IEnumerator RecoilAnimation_DontShoot()
    {
        canHold = false;
        // Move forward to _bodyOrigin.z
        while (_body.localPosition.z < _bodyOrigin.z)
        {
            _body.localPosition = new Vector3(_bodyOrigin.x, _bodyOrigin.y, Mathf.MoveTowards(_body.localPosition.z, _bodyOrigin.z, _animBodyShoodSpeed * Time.deltaTime));
            yield return null;
        }
        canHold = true;
    }

    #region Animations
    void OnOpenPincers()
    {
        AnimPerform();

        void AnimPerform()
        {
            // Move body backward
            if (_body.localPosition.z > maxZHold)
            {
                _body.localPosition = new Vector3(_bodyOrigin.x, _bodyOrigin.y, Mathf.MoveTowards(_body.localPosition.z, maxZHold, _animBodyHouldSpeed * Time.deltaTime));
            }

            // Rotate pincer roots and child objects using open speed
            if (Quaternion.Angle(_pincerRoots[0].localRotation, Quaternion.identity) < _desiredAngle)
            {
                _pincerRoots[0].Rotate(0, -_animPincersOpenSpeed * Time.deltaTime, 0, Space.Self);
                _pincerChilds[0].ObjRotate.RotateToward(
                    Quaternion.LookRotation(_pincerChilds[0].Destination.position - _pincerChilds[0].ObjRotate.position),
                    _pincerChilds[0].Speed * Time.deltaTime
                );
            }

            if (Quaternion.Angle(_pincerRoots[1].localRotation, Quaternion.identity) < _desiredAngle)
            {
                _pincerRoots[1].Rotate(0, _animPincersOpenSpeed * Time.deltaTime, 0, Space.Self);
                _pincerChilds[1].ObjRotate.RotateToward(
                    Quaternion.LookRotation(_pincerChilds[1].Destination.position - _pincerChilds[1].ObjRotate.position),
                    _pincerChilds[1].Speed * Time.deltaTime
                );
            }

            if (Quaternion.Angle(_pincerRoots[2].localRotation, Quaternion.identity) < _desiredAngle)
            {
                _pincerRoots[2].Rotate(-_animPincersOpenSpeed * Time.deltaTime, 0, 0, Space.Self);
                _pincerChilds[2].ObjRotate.RotateToward(
                    Quaternion.LookRotation(_pincerChilds[2].Destination.position - _pincerChilds[2].ObjRotate.position),
                    _pincerChilds[2].Speed * Time.deltaTime
                );
            }
        }
    }

    void OnRealeasePincers()
    {
        // Reset body position (lerp) using body speed and multiplier
        // if (Vector3.Distance(_body.localPosition, _bodyOrigin) > 0.01f)
        // {
        //     _body.localPosition = Vector3.Lerp(_body.localPosition, _bodyOrigin, _animBodySpeed * _multiplierSpdOnRealease * Time.deltaTime);
        // }

        // Rotate pincers back to identity using close speed
        for (int i = 0; i < 3; i++)
        {
            if (Quaternion.Angle(_pincerRoots[i].localRotation, Quaternion.identity) > 0.5f)
                _pincerRoots[i].RotateLocalToward(Quaternion.identity, _animPincersCloseSpeed * Time.deltaTime * _multiplierSpdOnRealease);

            _pincerChilds[i].ObjRotate.RotateLocalToward(
                Quaternion.identity,
                _pincerChilds[i].Speed * _multiplierSpdOnRealease * Time.deltaTime
            );
        }
    }

    float GetPincerOpenRatio(Transform pincerRoot)
    {
        float angle = Quaternion.Angle(pincerRoot.localRotation, Quaternion.identity);
        return Mathf.Clamp01(angle / _desiredAngle);
    }

    float GetOverallOpenRatio()
    {
        float ratio0 = GetPincerOpenRatio(_pincerRoots[0]);
        float ratio1 = GetPincerOpenRatio(_pincerRoots[1]);
        float ratio2 = GetPincerOpenRatio(_pincerRoots[2]);

        return Mathf.Min(ratio0, ratio1, ratio2);
    }

    void UpdateElectricBallScale()
    {
        if (electricBall == null) return;

        float openRatio = GetOverallOpenRatio();  // 0..1

        electricBall.localScale = Vector3.one * (electricBallMaxScale * openRatio);
    }

    // Kiểm tra đã thu nhỏ hoàn toàn (càng về rotation gốc và thân về vị trí gốc)
    public bool IsFullyClosed()
    {
        for (int i = 0; i < _pincerRoots.Length; i++)
        {
            if (Quaternion.Angle(_pincerRoots[i].localRotation, Quaternion.identity) > 0.5f)
                return false;
        }

        if (Vector3.Distance(_body.localPosition, _bodyOrigin) > 0.01f)
            return false;

        return true;
    }
    #endregion
}
