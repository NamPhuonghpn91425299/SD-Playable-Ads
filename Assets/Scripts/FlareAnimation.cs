using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareAnimation : MonoBehaviour
{
    public RectTransform flare;
    public float rotationSpeed = 90f; // Tốc độ xoay, đơn vị là độ/giây

    private void Update()
    {
        if (this.enabled)
        {
            flare.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}
