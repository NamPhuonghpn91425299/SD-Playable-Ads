using System.Collections;
using UnityEngine;
using static GameConstants;

public class PowerUpDisplay : MonoBehaviour
{
    public RewardNetwork rewardNetwork;
    public RewardType rewardType;
    public ParticleSystem[] CollectedEffect;
    public Transform mytrans;
    public GameObject myBody;
    public RectTransform ContentObj;
    public CanvasGroup ContentCanvasGroup;
    public float rewardValue;
    public float rotationSpeed = 50f; // Rotation speed in degrees/second
    public float initialYPosition = 0.5f; // Initial Y position
    public float moveDuration = 1f; // Duration for moving ContentObj from a to b
    public float displayDuration = 2f; // Duration for keeping ContentObj fully visible
    public Vector2 moveStartEndPositions = new Vector2(0f, 100f); // Start (a) and end (b) positions on the y-axis
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    int CountTakeDamage;

    private void Start()
    {
        // Ensure mytrans is assigned, if not use the transform of this GameObject
        if (mytrans == null)
        {
            mytrans = transform;
        }
        audioSource.clip = audioClips[0];
        audioSource.Play();
        // Set the initial position on the y-axis
        Vector3 initialPosition = mytrans.position;
        initialPosition.y = initialYPosition;
        this.transform.position = initialPosition;
    }

    private void OnEnable()
    {
        if (rewardNetwork != null)
        {
            rewardNetwork.OnRewardCollected += HandleRewardCollected;
        }
    }

    private void OnDisable()
    {
        if (rewardNetwork != null)
        {
            rewardNetwork.OnRewardCollected -= HandleRewardCollected;
        }
    }

    private void HandleRewardCollected()
    {
        StartCoroutine(ShowEffectCollected());
        StartCoroutine(ShowFireRateContent());
    }


    private IEnumerator ShowEffectCollected()
    {
        CollectedEffect[0].Play();
        yield return new WaitForSeconds(0.2f);
        OnSetEventPerRewardType();
        CollectedEffect[1].Play();
        audioSource.clip = audioClips[1];
        audioSource.Play();
        yield return new WaitForSeconds(0.2f);
        myBody.SetActive(false);
    }

    private void OnSetEventPerRewardType()
    {
        if (rewardType == RewardType.RapidFire)
        {
            EventManager.Invoke(EventName.OnUpgradeFireRate, rewardValue);
        }
        if (rewardType == RewardType.ChangeMachineGun)
        {
            EventManager.Invoke(EventName.OnChangeMachineGun, true);
        }
    }    

    private IEnumerator ShowFireRateContent()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = new Vector3(ContentObj.anchoredPosition.x, moveStartEndPositions.x, 0);
        Vector3 endPosition = new Vector3(ContentObj.anchoredPosition.x, moveStartEndPositions.y, 0);
        ContentCanvasGroup.alpha = 0;

        // Move ContentObj from position a to position b while fading in
        while (elapsedTime < moveDuration)
        {
            ContentObj.anchoredPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
            ContentCanvasGroup.alpha = elapsedTime / moveDuration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches the final position and alpha is set to 1
        ContentObj.anchoredPosition = endPosition;
        ContentCanvasGroup.alpha = 1;

        // Wait for displayDuration
        yield return new WaitForSeconds(displayDuration);

        // Fade out over moveDuration
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            ContentCanvasGroup.alpha = 1 - (elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure alpha is set to 0 after fade out
        ContentCanvasGroup.alpha = 0;
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Rotate mytrans around the Y-axis
        mytrans.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
