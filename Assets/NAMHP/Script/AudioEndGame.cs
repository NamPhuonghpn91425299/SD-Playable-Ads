using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEndGame : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip endGameClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip gameThemeClip;
    // Start is called before the first frame update
    void Start()
    {
        audioSource.clip = gameThemeClip;
        audioSource.Play();
        EventManager.AddListener<bool>(EventName.OnGameWon, OnGameWon);
        EventManager.AddListener<bool>(EventName.OnGameLost, OnGameLost);
        EventManager.AddListener<float>(EventName.OnTimeOut, OnTimeOut);
    }


    private void OnDisable()
    {
        EventManager.RemoveListener<bool>(EventName.OnGameWon, OnGameWon);
        EventManager.RemoveListener<bool>(EventName.OnGameLost, OnGameLost);
        EventManager.RemoveListener<float>(EventName.OnTimeOut, OnTimeOut);
    }

    private void OnTimeOut(float timeOut)
    {
        if (timeOut <= 0)
        {
            audioSource.clip = gameOverClip;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = gameThemeClip;
            audioSource.Play();
        }
    }
    private void OnGameLost(bool lostGame)
    {
        if (lostGame)
        {
            audioSource.clip = gameOverClip;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = gameThemeClip;
            audioSource.Play();
        }
    }

    private void OnGameWon(bool wonGame)
    {
        if (!wonGame)
        {
            audioSource.clip = endGameClip;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = gameThemeClip;
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
