using System;
using System.Collections;
using System.Collections.Generic;
using static GameConstants;
using UnityEngine;
using Assets._Develop_.ThanhNT.Scripts.Observer;

public class MissileControlPlayer : GameUnit<MissileControl>, Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<RocketEvent>
{
    [SerializeField] private Transform[] parents;
    private bool isPortrait;
    [SerializeField] private Transform TF_Camera;
    [SerializeField] private MissileSO missileSO;
    [SerializeField] private Animation _animation;
    [SerializeField] private Transform _pointSpawnMissile;
    [SerializeField] private float _fov = 60f; // Góc nhìn của tên lửa
    [SerializeField] private int currentMissileCount = 0; // Số lượng tên lửa hiện tại
    bool IsReload = false;
    bool CanCheckScreen = false;
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_animation == null)
        {
            _animation = GetComponent<Animation>();
            if(_animation == null)
                _animation = GetComponentInChildren<Animation>();
        }
    }
#endif

    private void Start()
    {
        EventManager.Instance?.Subscribe<RocketEvent>(this);
        OnInit();
    }

    void LogOrientation(bool portrait)
    {
        if (portrait)
        {
            // Debug.Log("Portrait mode");
            TF.position = parents[0].position;
        }
        else
        {
            TF.position = parents[1].position;
            // Debug.Log("Landscape mode");
        }
    }

    public virtual void OnInit()
    {
        _animation.AddClip(missileSO.Idle, "Idle");
        _animation.AddClip(missileSO.Fire, "Fire");
        _animation.AddClip(missileSO.Reload, "Reload");
        _animation.Play("Idle");
        currentMissileCount = missileSO.AmountRocket; // Khởi tạo số lượng tên lửa
    }

    

    private void Update()
    {
        if(!CanCheckScreen)
            return;
        if (isPortrait != (Screen.height > Screen.width))
        {
            isPortrait = !isPortrait;
            LogOrientation(isPortrait);
        }

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Space))
            Fire();
#endif
    }

    private Transform  FindLegitList(List<Transform> tars)
    {
        if (tars == null || tars.Count == 0)
        {
            return null;
        }

        float fov = _fov;
        float minHorAngle = float.MaxValue;
        Transform temp = null;

        for (int i = 0;
             i < tars.Count;
             i++)
        {
            if (tars[i] == null || tars[i] == TF_Camera)
            {
                continue;
            }

            float horAngle = Vector3.Angle(TF_Camera.forward, tars[i].position - TF_Camera.position);
            if (horAngle <= fov)
            {
                if (horAngle < minHorAngle)
                {
                    minHorAngle = horAngle;
                    temp = tars[i];
                }
            }
        }

        return temp;
    }
    
    public virtual void OnDespawn()
    {
        SimplePool<MissileControl>.Despawn(this);
    }

    public virtual void Fire()
    {
        //TODO: logic bắn tên lửa
        if (IsReload)
            return;
        Transform legitTarget = FindLegitList(BotSpawnManager.Instance.botInScene);
        SimplePool<Missile_Player>.Spawn<MissileUnit>(Missile_Player.Missile,_pointSpawnMissile.position,_pointSpawnMissile.rotation).OnInit(missileSO.isFollow && legitTarget != null ? legitTarget : null,GameController.Instance.CurrentWeapon.GizmodCaculatorPointShoot());
        currentMissileCount--;
        EventManager.Instance?.Publish(new RocketEvent(missileSO.isFollow, "UpdateIndex", missileSO.timeReload, currentMissileCount));
        Reload(); 
        _animation.Play("Fire");
    }

    public virtual void Reload()
    {
        //TODO: logic nạp tên lửa
        StartCoroutine(IEReload());
    }

    public void PlayMoveOninit()
    {
        StartCoroutine(IEMoveOninit());
    }
    
    private IEnumerator IEMoveOninit()
    {
        isPortrait = Screen.height > Screen.width;
        TF.parent = isPortrait ? parents[0] : parents[1];
        while (Vector3.Distance(TF.localPosition,Vector3.zero) >= 0.01f)
        {
            TF.localPosition = Vector3.MoveTowards(TF.localPosition,Vector3.zero,5f*Time.deltaTime);
            yield return null;
        }
        TF.localPosition = Vector3.zero;
        CanCheckScreen = true;
    }
    private IEnumerator IEReload()
    {
        IsReload = true;
        float timer = missileSO.timeReload;
        bool canPlayReload = true;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            if (canPlayReload && timer <= .3f)
            {
                canPlayReload = false;
                if(currentMissileCount>0)
                    _animation.Play("Reload");
            }
            yield return null;
        }
        IsReload = false;
    }
    
    // Thay đổi loại tên lửa, true là tên lửa follow, false là tên lửa thường
    public virtual void ChangeMissileFollow(bool _isFollow)
    {
        missileSO.isFollow = _isFollow;
    }

    public void OnNotify(RocketEvent data)
    {
//        Debug.Log($"RocketToggle: {data.IsRocketOn}");
        if (data.State == "ChangeStateRocketAim")
        {
            ChangeMissileFollow(data.IsRocketOn);
        }

        if (data.State == "Fire")
        {
//            Debug.Log("RocketToggle: Fire");
            Fire();
        }
    }

}