using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MedalChild : MonoBehaviour
{
    public MedalsUI medalsUI;
    public Animator anim;
    public int _medalID;
    public Image _medalImage;
    public Image _medalTitle;

    public void SetupMedalAndGetAnim(Sprite medalImage, Sprite medalTitle, int iconMedalID)
    {
        _medalID = iconMedalID;
        _medalImage.sprite = medalImage;
        _medalTitle.sprite = medalTitle;
    }

    public void ActiveFalseThis()
    {
        medalsUI.GetMedalTop(_medalID);
        gameObject.SetActive(false);
    }
}
