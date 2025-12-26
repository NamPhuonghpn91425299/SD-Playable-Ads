using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public static class AnimatorHashLib
{
    public static readonly string TypeMedal = "type";
    public static readonly string PlayMedal = "replay";
}

[System.Serializable]
public struct IconMedalTopGet
{
    public Image iconMedalTop;
    public Text amountText;
    public Animator[] animator;
}

public class MedalsUI : MonoBehaviour
{
    //[SerializeField] private Image _medalImage;
    //[SerializeField] private Image[] _medalFx;
    [FormerlySerializedAs("PoolGetMedalTopChild")]
    public IconMedalTopGet[] pooliconMedalTopGets;

    private bool indesEqual0;
    [SerializeField] private Sprite[] _medalTitle;
    [SerializeField] private Sprite[] _medalSprites;
    [SerializeField] private bool[] _hasBorderEffect;
    [SerializeField] protected MedalChild[] _medalAnimatorsPool;
    [SerializeField] private int[] amount = new int[5];

    [SerializeField] private AudioClip[] _medalAudioClips;

    // [SerializeField] private Animator archieAnimator;
    //
    // [SerializeField] protected Animator animReward;
    //[SerializeField] protected GameObject _planeStrikePrefab;
    // GameObject _planeStrikeObject;

    [SerializeField] private AudioSource _audioSource;

    /// <summary>
    /// Display for UI
    /// </summary>
    [ContextMenu("OnGetMedal 1")]
    public void OnGetMedal_0() => OnGetMedal(0);

    [ContextMenu("OnGetMedal 2")]
    public void OnGetMedal_1() => OnGetMedal(1);

    [ContextMenu("OnGetMedal 3")]
    public void OnGetMedal_2() => OnGetMedal(2);

    [ContextMenu("OnGetMedal 4")]
    public void OnGetMedal_3() => OnGetMedal(3);

    [ContextMenu("OnGetMedal 5")]
    public void OnGetMedal_4() => OnGetMedal(4);
#if UNITY_EDITOR

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            OnGetMedal_0();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            OnGetMedal_1();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            OnGetMedal_2();
        if (Input.GetKeyDown(KeyCode.Alpha4))
            OnGetMedal_3();
        if (Input.GetKeyDown(KeyCode.Alpha5))
            OnGetMedal_4();
    }

#endif
    public void OnGetMedal(int medalId)
    {
        _audioSource.Stop();
        foreach (MedalChild medalChild in _medalAnimatorsPool)
        {
            if (!medalChild.gameObject.activeSelf)
            {
                medalChild.SetupMedalAndGetAnim(_medalSprites[medalId], _medalTitle[medalId], medalId);
                medalChild.gameObject.SetActive(true);
                _audioSource.PlayOneShot(_medalAudioClips[medalId]);
                StartCoroutine(ShowMedal(medalId, medalChild));

                return;
            }
        }

       
    }

    // public override void Play<T>(T parameter)
    // {
    //     if (parameter is GameConstants.AchievementAnimationParameter.Killmark_center_1)
    //     {
    //         OnGetMedal_0();
    //     }
    //     else if (parameter is GameConstants.AchievementAnimationParameter.Killmark_center_2)
    //     {
    //         OnGetMedal_1();
    //     }
    //     else if (parameter is GameConstants.AchievementAnimationParameter.Killmark_center_3)
    //     {
    //         OnGetMedal_2();
    //     }
    //     else if (parameter is GameConstants.AchievementAnimationParameter.Killmark_center_4)
    //     {
    //         OnGetMedal_3();
    //     }
    //     else if (parameter is GameConstants.AchievementAnimationParameter.Killmark_center_5)
    //     {
    //         OnGetMedal_4();
    //     }
    // }

    protected virtual IEnumerator ShowMedal(int medalId, MedalChild medalChild)
    {
        medalChild.anim.SetInteger(AnimatorHashLib.TypeMedal, medalId);
        yield return HelperCoroutine.GetWait(.1f);
        medalChild.anim.SetTrigger(AnimatorHashLib.PlayMedal);
        //_medalAmountTexts[medalId].text = _medalAmounts[medalId].Value.ToString();
    }

    // private void OnDisable()
    // {
    //     archieAnimator.enabled = false;
    // }
    public void GetMedalTop(int i)
    {
        StartCoroutine(IEGetMedalTop(i));
    }

    IEnumerator IEGetMedalTop(int i)
    {
        IconMedalTopGet iconMedalTopGet = pooliconMedalTopGets[i];
        if(indesEqual0)
            iconMedalTopGet.animator[0].Play("111", 0, 0f);
        else
            iconMedalTopGet.animator[1].Play("111", 0, 0f);
        
        indesEqual0 = !indesEqual0;
        amount[i]++;
        iconMedalTopGet.amountText.text = amount[i].ToString();
        if (amount[i] <= 1)
        {   float elapsed = 0f;

            // Lấy màu hiện tại và set alpha về 0
            Color c = iconMedalTopGet.iconMedalTop.color;
            c.a = 0f;
            c.a = 0f;
            iconMedalTopGet.iconMedalTop.color = c;

            while (elapsed < .16f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / .16f);
                c.a = alpha;
                iconMedalTopGet.iconMedalTop.color = c;
                yield return null;
            }

            // Đảm bảo alpha cuối là 1
            c.a = 1f;
            iconMedalTopGet.iconMedalTop.color = c;
        }
    }
}