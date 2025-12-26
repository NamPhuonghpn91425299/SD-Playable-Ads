
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;



public class GameController : Singleton<GameController>,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<PlayerDeadEvent>,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<GameStateChangedEvent>
{
    public GameConstants.Weapon WeaponSpawnToStart;
    public MissileControlPlayer MissileControlPlayer;
    public Transform CameraMainTF;
    private WeaponBase currentWeapon;
    private GameState currentGameState = GameState.None;
    public GameState CurrentGameState => currentGameState;


    public Transform pointSpawnWeaponDEMO;
    private void Start()
    {

        CameraMainTF = Camera.main.transform;
        EventManager.Instance?.Subscribe<GameStateChangedEvent>(this);
    }

    private void OnDisable()
    {
        EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
    }

    public void SetState(GameState newState, bool isTrigger = false)
    {
        if (currentGameState == newState) return;

        currentGameState = newState;

        if (isTrigger)
        {
            // Notify observers about the game state change
            EventManager.Instance?.Publish(new GameStateChangedEvent(newState));
        }
    }

    public void OnNotify(PlayerDeadEvent data)
    {
        //EventManager.Instance?.Publish(new GameStateChangedEvent(GameState.GameOver));
    }

    public void OnNotify(GameStateChangedEvent data)
    {
        SetState(data.NewState);
//        Debug.Log($"Game state changed to: {data.NewState}");
        if (data.NewState == GameState.InGame)
        {
            currentWeapon = SimplePool<Weapon>.Spawn<WeaponBase>(WeaponSpawnToStart, Vector3.zero, Quaternion.identity);
            currentWeapon.OnInit();
            MissileControlPlayer.PlayMoveOninit();
        }
    }


    public WeaponBase CurrentWeapon
    {
        get => currentWeapon;
        set => currentWeapon = value;
    }


    public Vector3 GetPosLocalPlayer()
    {
        return currentWeapon.TF.position;
    }
}

