using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MedalsUI : MonoBehaviour
{
    
    [SerializeField] private Animator _animator;
    [SerializeField] private Image _medalImage;
    [SerializeField] private Image[] _medalFx;
    [SerializeField] private Text _medalTitle;
    [SerializeField] private Sprite[] _medalSprites;
    [SerializeField] private bool[] _hasBorderEffect;
    [SerializeField] private Color[] _medalTitleColors;
    [SerializeField] private string[] _medalTexts;
    [SerializeField] protected Animator[] _medalAnimators;
    [SerializeField] protected Text[] _medalAmountTexts;
    [SerializeField] private int[] amount = new int[5];
    [SerializeField] protected VariableReference<int>[] _medalAmounts;

    [SerializeField] private AudioClip[] _medalAudioClips;
    // [SerializeField] private AudioSource _medalAudioSource;

    [SerializeField] private Animator archieAnimator;
    
    [SerializeField] protected Animator animReward;
    [SerializeField] protected GameObject _planeStrikePrefab;
    GameObject _planeStrikeObject;
    
    [SerializeField] private  AudioSource _audioSource;

    /// <summary>
    /// Display for UI
    /// </summary>
    [ContextMenu("OnGetMedal 1")]
    public void OnGetMedal_0()=> OnGetMedal(0);
    
    [ContextMenu("OnGetMedal 2")]
    public void OnGetMedal_1()=> OnGetMedal(1);
    
    [ContextMenu("OnGetMedal 3")]
    public void OnGetMedal_2()=> OnGetMedal(2);
    
    [ContextMenu("OnGetMedal 4")]
    public void OnGetMedal_3()=> OnGetMedal(3);
    
    [ContextMenu("OnGetMedal 5")]
    public void OnGetMedal_4()=> OnGetMedal(4);
    public void OnGetMedal(int medalId)
    {
        if (medalId == (int)MedalType.DroneAdBtn)
        {
            //StartCoroutine(ShowMedalGetGold(medalId));
            return;
        }
        amount[medalId]++;
        _medalTitle.text = _medalTexts[medalId];
        _medalImage.sprite = _medalSprites[medalId];
        _medalImage.enabled = true;
        _animator.Play(AnimatorHashLib.Main);
        _audioSource.PlayOneShot(_medalAudioClips[medalId]);

        foreach (var fx in _medalFx)
            fx.enabled = _hasBorderEffect[medalId];
        _medalTitle.color = _medalTitleColors[medalId];

        StartCoroutine(ShowMedal(medalId));

        // EXPLAIN: 1.833 is length of notification animation
        // this.Delay(2.833f, () =>
        // {
        //     _medalAnimators[medalId].SetTrigger(AnimatorHashLib.Play);
        //     reward.SetActive(true);
        //     this.Delay(0.8f, () =>
        //     {
        //         _medalAmountTexts[medalId].text = _medalAmounts[medalId].Value.ToString();
        //     });
        // });
    }

    public void PlayFullSetAnimation()
    {
        StartCoroutine(ActionAnimMedal());
    }
    
    public void SpawnPlaneStrikeDropBomb()
    {
        if (_planeStrikeObject == null)
        {
            _planeStrikeObject = Instantiate(_planeStrikePrefab);
        }
        else
        {
            _planeStrikeObject.SetActive(true);
        }
    }
    
    private IEnumerator ActionAnimMedal()
    {
        yield return new WaitForSeconds(2.5f);
        if (archieAnimator != null)
        {
            archieAnimator.enabled = true;
            archieAnimator.Play("ArchimentFullShow");
        }
    }
    
    protected virtual IEnumerator ShowMedal(int medalId)
    {
        yield return new WaitForSeconds(1f);
        animReward.SetTrigger("ScaleIcon");
        animReward.SetInteger("MoveTo", medalId);
        yield return new WaitForSeconds(1.8f);
        _medalAnimators[medalId].SetTrigger(AnimatorHashLib.Play);
        yield return new WaitForSeconds(0.8f);
        _medalAmountTexts[medalId].text = amount[medalId].ToString();
        
        //_medalAmountTexts[medalId].text = _medalAmounts[medalId].Value.ToString();
    }
    
    private void OnDisable()
    {
        archieAnimator.enabled = false;
    }
}