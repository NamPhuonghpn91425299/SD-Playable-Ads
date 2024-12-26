using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunaShowEndCard : MonoBehaviour
{
    public static LunaShowEndCard Instance;

    private void Awake()
    {
        Instance = this;
    }

    IEnumerator ShowEndCard()
    {
        yield return new WaitForSeconds(0.5f);

        if (EndCardController.Instance != null)
        {
            EndCardController.Instance.OpenEndCard();
        }
        Time.timeScale = 0;
    }

    public void EndGameUI()
    {
       StartCoroutine(ShowEndCard());
    }
}
