
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.UI;


public class GameOverDisplay : MonoBehaviour, IObserver<PlayerDeadEvent>
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Button restartButton;
    
    private IEventManager eventManager;
    
    void Start()
    {
        eventManager?.Subscribe<PlayerDeadEvent>(this);


        
        
        // Hide game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClick);
        }
    }
    
    void OnDestroy()
    {
        eventManager?.Unsubscribe<PlayerDeadEvent>(this);
    }
    
    public void OnNotify(PlayerDeadEvent data)
    {
        ShowGameOver();
    }
    
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
        }
        
        Debug.Log("Game Over!");
    }
    
    private void OnRestartButtonClick()
    {
        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Reset player health
        // PlayerController player = FindObjectOfType<PlayerController>();
        // if (player != null)
        // {
        //     player.ResetHealth();
        // }
    }
}