using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResultInstance : MonoBehaviour
{
    public static GameResultInstance Instance;
    public GameResultData gameResultData;
   // public string url = "https://play.google.com/store/apps/details?id=com.horus.sky.defense";

    private void Awake()
    {
        Instance = this;
    }

    public GameResultData GetGameResultData()
    {
        return gameResultData;
    }    

    //public void OpenURLStore()
    //{
    //    Application.OpenURL(url);
    //}    
}
