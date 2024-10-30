using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class BulletFly : MonoBehaviour
{
    float simulationSpeed;

    Vector3 startPoint;
    Vector3 endPoint;

    float remainingDistance, distance;

    public void Init(Vector3 startPoint, Vector3 endPoint, float simulationSpeed)
    {
        this.simulationSpeed = simulationSpeed;
        this.startPoint = startPoint;
        this.endPoint = endPoint;

        distance = Vector3.Distance(endPoint, startPoint);
        remainingDistance = 0;
    }
    private void Update()
    {
      
        remainingDistance += simulationSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01((remainingDistance / distance)));

        if (remainingDistance >= distance)
        {
            this.gameObject.SetActive(false);
        }
    }
}
