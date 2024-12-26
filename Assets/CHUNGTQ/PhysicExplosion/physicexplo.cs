using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class physicexplo : MonoBehaviour
{
    [Header("Phạm Vi Vụ nổ")] public float radius = 25f;

    [Header("Sức Mạnh vụ nổ (Càng to bay càng xa)")]
    public float power = 500.0F;

    [Header("Trọng lực (Càng lớn vật rơi càng nhanh)")] [Range(-50f, 10f)]
    public float gravityforce = -20;

    [Header("Thời gian tối đa sử dụng vật lý")]
    public float physicTime = 6;

    [Header("Tắt Obj khi hết thời gian hiệu lực")]
    public bool disableObjAffterStopPhysic = false;

    [Header("Bot là máy bay")] 
    public bool isAirplane = false;
    
    [Tooltip("Will the break parts detect collision if it's a airplane")]
    public bool isCollisionsDetecting = true;
    
    public float timedisableBoxesAirplane = 0.2f;

    [Header("Tâm của vụ nổ")] public Transform centerOfExplosion;

    public BoxCollider[] key;
    public Rigidbody[] physic;

    [HideInInspector] public Transform[] physicstrans;
    public Vector3[] previousLocation;
    [HideInInspector] public Quaternion[] previousRotation;

    protected float _time;
    protected Transform _myTrans;
    protected int countFrame;
    protected float _startPosY;
    protected int _typeBot;

    protected virtual void OnEnable()
    {
        _myTrans = transform;
        _time = 0;
        countFrame = 0;
        if (AllTypeBot.CarsAndTanksType.Contains(_typeBot))
        {
            if (_startPosY > 12)
            {
                isAirplane = true;
            }
            else
                isAirplane = false;
        }
        StartBreak();
    }

    public void Setting(float posY, int TypeBot)
    {
        _startPosY = posY;
        _typeBot = TypeBot;
    }

    public virtual void StartBreak()
    {
        if (centerOfExplosion == null)
        {
            var gameObject = new GameObject(name = "CenterOfExplosion");
            gameObject.transform.SetParent(_myTrans);
            gameObject.transform.position = _myTrans.position;
        }

        for (var i = 0; i < physic.Length; i++)
        {
            if (key[i])
            {
                key[i].gameObject.SetActive(true);
                key[i].enabled = true;
            }

            if (physic[i])
            {
                physic[i].velocity = Vector3.zero;
                physic[i].angularVelocity = Vector3.zero;
                physic[i].detectCollisions = true;
                physic[i].isKinematic = false;
                physic[i].useGravity = true;
            }
        }

        foreach (var rb in physic)
        {
            if (rb != null)
                rb.AddExplosionForce(power, centerOfExplosion.position, radius);
        }

        if (isAirplane && !isCollisionsDetecting) Invoke(nameof(DisableBoxes), timedisableBoxesAirplane);
    }

    protected virtual void DisableBoxes()
    {
        for (int i = 0; i < key.Length; i++)
        {
            if (key[i]) key[i].enabled = false;
            if (physic[i])
            {
                physic[i].detectCollisions = false;
                //physic[i].isKinematic = true;
            }
        }
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        UpdateExplosion();
    }

    protected virtual void UpdateExplosion()
    {
        _time += Time.fixedDeltaTime;
        if (_time >= physicTime)
        {
            _time = -10000;

            for (var i = 0; i < physic.Length; i++)
            {
                try
                {
                    if (disableObjAffterStopPhysic)
                    {
                        key[i].gameObject.SetActive(false);
                    }

                    physic[i].detectCollisions = false;
                    physic[i].isKinematic = true;
                    key[i].enabled = false;
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogError($"{transform.root.name} : Key hoặc physic của index {i} null rùi");
#endif
                }
            }
        }
        else
        {
            if (_time > 0.1f)
            {
                for (var i = 0; i < physic.Length; i++)
                {
                    try
                    {
                        if (!isAirplane)
                        {
                            var inversePos = (_myTrans.InverseTransformPoint(physic[i].transform.position));

                            if (inversePos.y > 0 && !physic[i].IsSleeping())
                            {
                                physic[i].AddForce(new Vector3(0, gravityforce, 0) * physic[i].mass);
                            }

                            else if (physic[i].angularVelocity.sqrMagnitude < 0.025 && inversePos.y < 0.5f ||
                                     inversePos.y < -0.15f)
                            {
                                physic[i].Sleep();
                            }
                        }
                        else
                        {
                            physic[i].AddForce(new Vector3(0, gravityforce, 0) * physic[i].mass);
                        }
                    }
                    catch (Exception e)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"{transform.root.name} : Key hoặc physic của index {i} null rùi");
#endif
                    }
                }
            }
        }
    }

    protected virtual void OnDisable()
    {
        ResetAllPartToBase();
        CancelInvoke();
    }

    protected virtual void ResetAllPartToBase()
    {
        for (var i = 0; i < physicstrans.Length; i++)
        {
            if (physicstrans[i])
            {
                physicstrans[i].localPosition = previousLocation[i];
                physicstrans[i].localRotation = previousRotation[i];
            }
        }
    }

    public void SetPower(float p)
    {
        power = p;
    }
}
public static class AllTypeBot
{
    public static int[] CarsAndTanksType = new int[] { 11, 4, 18, 23, 34, 39, 41, 42, 35, 50, 5, 45, 46 };
    public static int[] TanksType = new int[] { 5, 45, 46 };
    public static int[] CarsType = new int[] { 11, 4, 18, 23, 34, 39, 41, 42, 35, 50 };
    public static int[] Soldiers = new int[] { 0, 3, 10, 24, 25, 26, 27, 28, 30, 47 };
    public static int[] Airplanes = new int[] { 15, 16, 21, 29, 37, 170, 43, 14 };
    public static int[] HeliSupportT = new int[] { 20, 22, 15, 29, 37, 9, 21, 29, 8, 35 };
    public static int[] HeliSupportVehicleT = new int[] { 4, 5, 11, 18, 23, 34, 38, 39, 41, 42, 45 };
    public static int[] HeliSupportFlyT = new int[] { 14, 15, 29, 37, 43 };
    public static int[] BoatType = new int[] { 8, 32, 33 };
}
