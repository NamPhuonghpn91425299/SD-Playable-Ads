
using UnityEngine;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using static GameConstants;
using System.Collections.Generic;

public class UIController : MonoBehaviour, IObserver<GameStateChangedEvent>
{
    public static UIController Instance { get; private set; }
    private List<UIBase> _uIBases;
    [SerializeField] private float timerDelayEndGame = 1f;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    private void Start()
    {

        EventManager.Instance?.Subscribe<GameStateChangedEvent>(this);
        // Get all UIBase components, including on inactive objects
        UIBase[] uiComponents = GetComponentsInChildren<UIBase>(true);
        _uIBases = new List<UIBase>();

        foreach (var ui in uiComponents)
        {
            if (ui != null)
            {
                _uIBases.Add(ui);
            }
        }

//        Debug.Log($"Found {_uIBases.Count} UI components in UIController.");
    }

    private void OnDestroy()
    {
        EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
    }

    public void ShowUI<T>() where T : UIBase
    {
        foreach (var ui in _uIBases)
        {
            if (ui == null) continue; // Tránh lỗi nếu object đã bị destroy

            if (ui is T)
            {
                if (!ui.gameObject.activeSelf)
                    ui.gameObject.SetActive(true);
                ui.Show();
            }
            else
            {
                if (ui.gameObject.activeSelf)
                    ui.Hide();
            }
        }
    }

    public void OnNotify(GameStateChangedEvent data)
    {
        if (data == null) return;
        switch (data.NewState)
        {
            case GameState.Loading:
                ShowUI<IntroUI>();
                break;
            case GameState.InGame:
                ShowUI<GameplayUI>();
                break;
            case GameState.GameWin:
                StartCoroutine(EndGameDelay<WinGameUI>());
                break;
            case GameState.GameOver:
                ShowUI<LoseGameUI>();
                break;
            default:
                Debug.LogWarning($"Unhandled game state: {data.NewState}");
                break;
        }
    }

    private System.Collections.IEnumerator EndGameDelay<T>() where T : UIBase
    {
            yield return HelperCoroutine.GetWait(timerDelayEndGame);
        ShowUI<T>();
    }
}