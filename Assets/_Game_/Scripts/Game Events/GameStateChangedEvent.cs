
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;



[System.Serializable]
public class GameStateChangedEvent : IGameEvent
{
    public GameState NewState { get; }
    public float Timestamp => Time.time;

    // additional properties can be added here if needed

    public GameStateChangedEvent(GameState newState)
    {
        NewState = newState;
    }

    
}
