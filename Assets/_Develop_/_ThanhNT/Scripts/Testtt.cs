using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Testtt : MonoBehaviour
{
    [SerializeField] private Button _switchButton;

    void Start()
    {
        _switchButton.onClick.AddListener(OnSwitchButtonClicked);
    }

    private void OnSwitchButtonClicked()
    {
        Debug.Log("Switch button clicked!");
        // Add your logic here for what happens when the switch button is clicked
    }
}
