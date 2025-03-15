using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAchivement : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.AddListener<bool>(EventName.OnStaticBotDead,OnBotDeath);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<bool>(EventName.OnStaticBotDead, OnBotDeath);
    }

    private void OnBotDeath(bool isShow)
    {
        if (isShow)
        {
            Debug.Log("Bot Death");
            animator.Rebind(); // Reset toàn bộ trạng thái Animator
            animator.Play("Multi Kill");
            //animator.SetBool("Mutil", false); // Đảm bảo reset trước khi set lại true
            //animator.SetBool("Mutil", true);
            audioSource.Play();
        }
        else
        {
            //animator.SetBool("Mutil", false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
