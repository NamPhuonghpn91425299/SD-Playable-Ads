
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.UI;
using static GameConstants;

public class IntroUI : UIBase
{
    [SerializeField] private Button _startButton;

    // Start is called before the first frame update

    void Awake()
    {
        _startButton.onClick.AddListener(OnStartButtonClicked);
    }

    void OnDisable()
    {
        _startButton.onClick.RemoveListener(OnStartButtonClicked);
    }


    
    

    public void OnStartButtonClicked()
    {
        EventManager.Instance?.Publish(new GameStateChangedEvent(GameState.InGame));
    }

    
}
